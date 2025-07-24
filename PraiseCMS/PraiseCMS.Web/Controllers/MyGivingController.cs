using PraiseCMS.API.Helpers;
using PraiseCMS.API.Models;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PraiseCMS.Shared.Methods.ExtensionMethods;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "039942579226fbdffc28c8429886fd")]
    public class MyGivingController : BaseController
    {
        private const string EnabledGivingMessage = " has not enabled digital giving. You are unable to give through Praise CMS at this time.";

        public async Task<ActionResult> Index()
        {
            if (SessionVariables.CurrentMerchant.IsNullOrEmpty())
            {
                SessionVariables.CurrentMerchant = work.ChurchMerchantAccount.GetByChurchId(SessionVariables.CurrentChurch.Id);
            }

            if (SessionVariables.CurrentMerchant.IsNullOrEmpty() || !SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage(SessionVariables.CurrentChurch.Display + EnabledGivingMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return RedirectToAction("Index", "Home");
            }

            // Get dashboard data
            var result = await work.Giving.GetGivingDashboard(SessionVariables.CurrentMerchant, SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, 10);

            // Check the result type and handle accordingly
            if (result.ResultType != ResultType.Success)
            {
                CreateAlertMessage(result.Message, result.ResultType, result.ResultIcon);
            }

            return View(result.Data);
        }

        //Get Last Gift in Give Now Modal
        [AllowAnonymous]
        public ActionResult GetPayment(string id)
        {
            return Json(work.Payment.Get(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GiveNow()
        {
            var donorGuid = SessionVariables.CurrentUser.UserMerchantAccounts.Find(x => x.Merchant.Equals(MerchantProviders.Nuvei)).DonorGUID;
            var model = work.Giving.GetViewModel(donorGuid);

            return PartialView("_GiveNow", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiveNow(GivingViewModel model)
        {
            model.DonorGUID = SessionVariables.CurrentUser.UserMerchantAccounts.FirstOrDefault().DonorGUID;

            if (ModelState.IsValid)
            {
                var fund = new Fund();
                if (model.Amount.IsNullOrEmpty())
                {
                    model.Amount = "0";
                }

                model.Payment.Amount = decimal.Parse(model.Amount);

                if (!string.IsNullOrEmpty(model.Payment.FundId))
                {
                    fund = work.Fund.Get(model.Payment.FundId);

                    if (fund.IsNullOrEmpty() || fund.Closed)
                    {
                        fund = work.Fund.GetByName(model.Church.Id, GivingFunds.General);
                        model.Payment.FundId = fund.Id;
                    }
                }

                var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(model.Payment.PaymentMethod);

                model.Payment.ProcessingFee = work.Giving.CalculateProcessingFee(model.Church.Id, model.Payment.Amount.ToString(), paymentMethodAccount.AccountType);
                model.Payment.DonorPaidMerchantFee = model.IncludeProcessingFee;
                model.AccountType = paymentMethodAccount.AccountType;
                model.ChurchId = model.Payment.ChurchId;
                model.AccountGUID = model.Payment.PaymentMethod;
                model.Payment.TransactionType = TransactionType.Payment;
                model.Payment.Frequency = PaymentOccurrence.OneTime;
                model.Payment.DigitalPaymentMethod = paymentMethodAccount.AccountType;
                model.Payment.DigitalPaymentType = DigitalPaymentTypes.Online;

                if (!string.IsNullOrEmpty(model.DonorGUID) && !string.IsNullOrEmpty(model.AccountGUID))
                {
                    ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

                    var transactionResponse = new TransactionResponse();

                    if (model.AccountType == DigitalPaymentMethods.Card)
                    {
                        var cardTransactionRequest = new CardTransactionRequest
                        {
                            tokenized_card = new TokenizedCard
                            {
                                card_info_key = model.AccountGUID,
                                amount = model.Amount,
                                transaction_type = TransactionTypeShortCode.RepeatSale,
                                pos_environment_indicator = "R"
                            }
                        };

                        transactionResponse = await nuveiHelper.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials);

                        model.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);

                        if (model.Payment.PaymentStatus != APIStatuses.Success)
                        {
                            string apiErrorMessage = Responses.HandleApiTransactionFailure(transactionResponse);
                            CreateAlertMessage("There was a problem submitting your donation. Please try again with a different payment method. Thank you. (Error: " + apiErrorMessage + ")", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                            return AjaxRedirectTo("/mygiving");
                        }

                        model.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;

                        var result = work.Payment.Create(model.Payment);
                        model.Payment = result.Data;

                        work.Giving.SendPaymentStatusEmail(model, transactionResponse);
                    }
                    else
                    {
                        var checkTransactionRequest = new CheckTransactionRequest
                        {
                            tokenized_check = new TokenizedCheck
                            {
                                check_info_key = model.AccountGUID,
                                amount = model.Amount,
                                transaction_type = TransactionTypeShortCode.Auth,
                                standard_entry_class_codes_type = "WEB"
                            }
                        };

                        transactionResponse = await nuveiHelper.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials);

                        model.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);

                        if (model.Payment.PaymentStatus != APIStatuses.Success)
                        {
                            string apiErrorMessage = Responses.HandleApiTransactionFailure(transactionResponse);
                            CreateAlertMessage("There was a problem submitting your donation. Please try again with a different payment method. Thank you. (Error: " + apiErrorMessage + ")", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                            return AjaxRedirectTo("/mygiving");
                        }

                        model.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : null;

                        var result = work.Payment.Create(model.Payment);
                        model.Payment = result.Data;

                        work.Giving.SendPaymentStatusEmail(model, transactionResponse);
                    }

                    CreateAlertMessage("Thank you. Your payment has been submitted (Transaction #:" + model.Payment.TransactionId + ").", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return AjaxRedirectTo("/mygiving");
                }
                else
                {
                    CreateAlertMessage("Please fill out all required fields and try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }
            }

            model.Accounts = work.Payment.GetPaymentMethodsDropdownList(model.DonorGUID);
            model.Funds = work.Fund.GetAll(model.Church.Id).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id
            }).ToList();
            model.Campuses = work.Campus.GetAll(model.Church.Id).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id
            }).ToList();
            model.LastGift = work.Payment.GetAllByUserId(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id).FirstOrDefault();

            return PartialView("_GiveNow", model);
        }

        #region Payment Methods
        public ActionResult PaymentMethods()
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage(SessionVariables.CurrentChurch.Display + EnabledGivingMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return RedirectToAction("Index", "MyGiving");
            }

            var methods = work.Payment.GetPaymentMethods(SessionVariables.CurrentUser.User, SessionVariables.CurrentUser.UserMerchantAccounts, SessionVariables.CurrentChurch.DonorGUID);

            return View(methods);
        }

        [HttpGet]
        public ActionResult AddCard()
        {
            var model = new PaymentMethodViewModel
            {
                User = SessionVariables.CurrentUser.User,
                UserMerchantAccount = SessionVariables.CurrentUser.UserMerchantAccounts.FirstOrDefault(),
                PaymentMethod = DigitalPaymentMethods.Card,
                ChurchId = SessionVariables.CurrentChurch.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCard(PaymentMethodViewModel model)
        {
            if (TryValidateModel(model.PaymentCard))
            {
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);
                model.DonorGUID = model.UserMerchantAccount.DonorGUID;

                if (await work.Giving.CardExistsForDonor(model, apiCredentials))
                {
                    CreateAlertMessage(PaymentMethodAlertMessages.CardExists, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(model);
                }

                //add card details to payment provider
                var mappedCard = work.Giving.MapToCardRequestModel(model);

                var cardData = await nuveiHelper.CreateCardAsync(mappedCard, apiCredentials);

                var createCardResponse = Responses.GetApiTransactionResponse(cardData?.result);

                if (createCardResponse != APIStatuses.Success)
                {
                    string apiErrorMessage = Responses.HandleApiTransactionFailure(cardData);
                    CreateAlertMessage(PaymentMethodAlertMessages.CardAddError + " (Error: " + apiErrorMessage + ")", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", apiErrorMessage, "DonorGUID", model.DonorGUID);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Card", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : string.Empty, LogStatuses.Error, logObj);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(cardData.card_key))
                {
                    var accountGUID = cardData.card_key;
                    var donorGUID = cardData.customer_key;
                    var method = model.PaymentCard.NickName.IsNotNullOrEmpty() ? model.PaymentCard.NickName : model.PaymentCard.CcType;
                    var paymentMethodAccount = new PaymentMethodAccount
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Type = PaymentMethodAccountTypes.User,
                        TypeId = SessionVariables.CurrentUser.User.Id,
                        DonorGUID = !string.IsNullOrEmpty(donorGUID) ? donorGUID : string.Empty,
                        AccountGUID = !string.IsNullOrEmpty(accountGUID) ? accountGUID : string.Empty,
                        Merchant = MerchantProviders.Nuvei,
                        AccountType = DigitalPaymentMethods.Card,
                        AccountProvider = Utilities.GetCardProvider(model.PaymentCard.CcNumber),
                        PaymentMethodPreview = $"{method}: {model.PaymentCard.CcNumber.GetLastCharacters(4)}",
                        NickName = model.PaymentCard.NickName.IsNotNullOrEmpty() ? model.PaymentCard.NickName : work.Giving.GetNickNameByType(model.PaymentCard.CcType),
                        AccountSubType = model.PaymentCard.CcType,
                        IsActive = true,
                        IsPrimary = false,
                        ExpMonth = model.PaymentCard.CcExpMonth,
                        ExpYear = model.PaymentCard.CcExpYear,
                        CreatedDate = DateTime.Now,
                        CreatedBy = SessionVariables.CurrentUser.User.Id
                    };

                    work.PaymentMethodAccount.Create(paymentMethodAccount);

                    var content = EmailTemplates.PaymentMethod_body.Replace("{message}", PaymentMethodAlertMessages.CardAddSuccess)
                        .Replace("{user_display}", SessionVariables.CurrentUser.User.FirstName)
                        .Replace("{paymentmethod}", paymentMethodAccount.PaymentMethodPreview)
                        .Replace("{nickName}", paymentMethodAccount.NickName);
                    var email = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = content,
                        To = SessionVariables.CurrentUser.User.Email,
                        Attachments = null,
                        Subject = "New payment method added.",
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    };

                    if (SessionVariables.CurrentUser.User.Email.IsNotNullOrEmpty())
                    {
                        Emailer.SendEmail(email);
                    }

                    CreateAlertMessage($"Your credit card <b>ending in: {model.PaymentCard.CcNumber.GetLastCharacters(4)}</b> has been added.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return RedirectToAction(nameof(PaymentMethods));
                }
            }

            DisplayErrors();

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> EditCard(string id)
        {
            var userMerchantAccount = work.UserMerchantAccount.GetByUserId(SessionVariables.CurrentUser.User.Id, MerchantProviders.Nuvei);
            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

            var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(id);

            var model = await PaymentMethodHelpers.GetEditCardViewModel(paymentMethodAccount, apiCredentials, this, work, nuveiHelper);

            if (model == null)
            {
                return RedirectToAction(nameof(PaymentMethods));
            }

            return View("AddCard", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCard(PaymentMethodViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PaymentCard.NickName))
            {
                model.DonorGUID = model.UserMerchantAccount.DonorGUID;

                var PaymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(model.PaymentCard.AccountGUID);

                if (PaymentMethodAccount == null) return View("AddBankAccount", model);

                PaymentMethodAccount.NickName = model.PaymentCard.NickName;
                PaymentMethodAccount.ModifiedDate = DateTime.Now;
                PaymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.PaymentMethodAccount.Update(PaymentMethodAccount);

                CreateAlertMessage($"The nickname for card ending in {model.PaymentCard.CcNumber.GetLastCharacters(4)} has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction(nameof(PaymentMethods));
            }

            CreateAlertMessage("Please enter a nickname for your card.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return View("AddBankAccount", model);
        }

        [HttpGet]
        public ActionResult AddBankAccount()
        {
            var model = new PaymentMethodViewModel
            {
                User = SessionVariables.CurrentUser.User,
                UserMerchantAccount = SessionVariables.CurrentUser.UserMerchantAccounts.FirstOrDefault(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                PaymentMethod = DigitalPaymentMethods.ACH
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddBankAccount(PaymentMethodViewModel model)
        {
            if (ValidatePaymentModel(model.PaymentAccount))
            {
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

                model.DonorGUID = model.UserMerchantAccount.DonorGUID;

                var verifyRouting = await nuveiHelper.VerifyBankRoutingAsync(model.PaymentAccount.RoutingNumber, apiCredentials);

                //Problem with routing number
                if (verifyRouting.result != null && verifyRouting.result != "0")
                {
                    CreateAlertMessage(verifyRouting.result_message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(model);
                }

                var bankName = verifyRouting.aba.bank_name;

                if (await work.Giving.CheckAccountExists(model, apiCredentials))
                {
                    CreateAlertMessage(PaymentMethodAlertMessages.BankAccountExists, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(model);
                }

                if (verifyRouting.aba != null)
                {
                    //add bank account details to payment provider
                    var mappedCheck = work.Giving.MapToCheckRequestModel(model);

                    var bankData = await nuveiHelper.CreateCheckAsync(mappedCheck, apiCredentials);

                    var createCheckResponse = Responses.GetApiTransactionResponse(bankData?.result);

                    if (createCheckResponse != APIStatuses.Success)
                    {
                        string apiErrorMessage = Responses.HandleApiTransactionFailure(bankData);
                        CreateAlertMessage(PaymentMethodAlertMessages.CardAddError + " (Error: " + apiErrorMessage + ")", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                        var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", apiErrorMessage, "DonorGUID", model.DonorGUID);
                        logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Bank Account", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : string.Empty, LogStatuses.Error, logObj);

                        return View(model);
                    }

                    if (!string.IsNullOrEmpty(bankData.check_key))
                    {
                        var accountGUID = bankData.check_key;
                        var donorGUID = bankData.customer_key;
                        var method = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : "BANK";
                        var PaymentMethodAccount = new PaymentMethodAccount
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Type = PaymentMethodAccountTypes.User,
                            TypeId = !string.IsNullOrEmpty(SessionVariables.CurrentUser.User.Id) ? SessionVariables.CurrentUser.User.Id : string.Empty,
                            DonorGUID = !string.IsNullOrEmpty(donorGUID) ? donorGUID : string.Empty,
                            AccountGUID = !string.IsNullOrEmpty(accountGUID) ? accountGUID : string.Empty,
                            Merchant = MerchantProviders.Nuvei,
                            AccountType = DigitalPaymentMethods.ACH,
                            AccountProvider = !string.IsNullOrEmpty(bankName) ? bankName : string.Empty,
                            PaymentMethodPreview = $"{method}: {model.PaymentAccount.AccountNumber.GetLastCharacters(4)}",
                            NickName = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : work.Giving.GetNickNameByType(model.PaymentAccount.AccountType),
                            AccountSubType = model.PaymentAccount.AccountType,
                            IsActive = true,
                            IsPrimary = false,
                            CreatedDate = DateTime.Now,
                            CreatedBy = SessionVariables.CurrentUser.User.Id
                        };

                        work.PaymentMethodAccount.Create(PaymentMethodAccount);

                        var content = EmailTemplates.PaymentMethod_body.Replace("{message}", PaymentMethodAlertMessages.BankAccountAddSuccess)
                             .Replace("{user_display}", SessionVariables.CurrentUser.User.FirstName);
                        var email = new Email()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Message = content,
                            To = SessionVariables.CurrentUser.User.Email,
                            Attachments = null,
                            Subject = "New payment method added.",
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now
                        };

                        if (SessionVariables.CurrentUser.User.Email.IsNotNullOrEmpty())
                        {
                            Emailer.SendEmail(email);
                        }

                        CreateAlertMessage($"Your bank account <b>ending in: {model.PaymentAccount.AccountNumber.GetLastCharacters(4)}</b> has been added.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                        return RedirectToAction(nameof(PaymentMethods));
                    }
                }
            }
            else
            {
                DisplayErrors();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> EditBankAccount(string id)
        {
            var userMerchantAccount = work.UserMerchantAccount.GetByUserId(SessionVariables.CurrentUser.User.Id, MerchantProviders.Nuvei);
            var PaymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(id);
            var bankAccount = new BankAccount();

            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

            var checkResponse = await nuveiHelper.GetCheckDetailsAsync(PaymentMethodAccount?.AccountGUID, apiCredentials);
            bool checksMatch = false;

            if (checkResponse?.result == "0" && checkResponse.paymentsafe_check != null)
            {
                checksMatch = PaymentMethodAccount?.AccountGUID == checkResponse.paymentsafe_check.check_key;
                bankAccount.AccountGUID = PaymentMethodAccount?.AccountGUID;
                bankAccount.AccountNumber = PaymentMethodAccount?.PaymentMethodPreview;
                bankAccount.AccountType = PaymentMethodAccount?.AccountType;
                bankAccount.Nickname = PaymentMethodAccount?.NickName;
                bankAccount.MaskedAccountNumber = $"****{checkResponse.paymentsafe_check.account_number_last_four_digits}";
            }

            if (!checksMatch)
            {
                //Mark inactive in our tables since it is not found at merchant. We don't delete so we can use in giving history
                if (PaymentMethodAccount != null)
                {
                    PaymentMethodAccount.IsActive = false;
                    PaymentMethodAccount.ModifiedDate = DateTime.Now;
                    PaymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.PaymentMethodAccount.Update(PaymentMethodAccount);
                }

                CreateAlertMessage($"Your bank account ending in {PaymentMethodAccount?.PaymentMethodPreview} could not be found and has been removed from this page. Please try adding this payment method again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return RedirectToAction(nameof(PaymentMethods));
            }

            if (bankAccount == null) return null;

            var model = new PaymentMethodViewModel
            {
                User = SessionVariables.CurrentUser.User,
                PaymentMethod = DigitalPaymentMethods.ACH,
                PaymentAccount = new PaymentAccount()
                {
                    AccountNumber = bankAccount.AccountNumber,
                    AccountType = bankAccount.AccountType,
                    NickName = PaymentMethodAccount.NickName,
                    RoutingNumber = bankAccount.RoutingNumber,
                    AccountGUID = bankAccount.AccountGUID
                }
            };

            return View("AddBankAccount", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBankAccount(PaymentMethodViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PaymentAccount.NickName))
            {
                var method = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : "BANK";
                var PaymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(model.PaymentAccount.AccountGUID);

                if (PaymentMethodAccount.IsNotNullOrEmpty())
                {
                    PaymentMethodAccount.NickName = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : work.Giving.GetNickNameByType(model.PaymentAccount.AccountType);
                    PaymentMethodAccount.ModifiedDate = DateTime.Now;
                    PaymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.PaymentMethodAccount.Update(PaymentMethodAccount);

                    CreateAlertMessage($"Your bank account ending in {model.PaymentAccount.AccountNumber.GetLastCharacters(4)} has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return RedirectToAction(nameof(PaymentMethods));
                }
            }

            CreateAlertMessage("Please enter a nickname for your bank account.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return View("AddBankAccount", model);
        }
        #endregion

        #region Scheduled Giving
        public ActionResult ScheduleGift(string id = null)
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage(SessionVariables.CurrentChurch.Display + EnabledGivingMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return RedirectToAction("Index", "Home");
            }

            var model = work.Giving.GetGivingViewModelById(id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScheduleGift(ScheduledGiftViewModel model)
        {
            var hasAmount = decimal.TryParse(model.Amount, out var amount);

            if (hasAmount)
            {
                model.ScheduledPayment.Amount = amount;
            }

            if (ModelState.IsValid)
            {
                if (model.ScheduledPayment.FundId.IsNullOrEmpty())
                {
                    var fund = work.Fund.GetByName(model.ChurchId, GivingFunds.General);
                    model.ScheduledPayment.FundId = fund?.Id;
                }

                if (model.ScheduledPayment.Id.IsNotNullOrEmpty())
                {
                    model.ScheduledPayment.ModifiedDate = DateTime.Now;
                    model.ScheduledPayment.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.ScheduledPayment.Update(model.ScheduledPayment);

                    CreateAlertMessage("Your scheduled gift has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    work.Giving.SendScheduledGiftEmail(model);

                    return RedirectToAction("EditScheduled");
                }

                model.ScheduledPayment.Id = Utilities.GenerateUniqueId();
                var result = work.ScheduledPayment.Create(model.ScheduledPayment);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("Your scheduled gift has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    work.Giving.SendScheduledGiftEmail(model);

                    return RedirectToAction("Index");
                }

                CreateAlertMessage("There was a problem creating your scheduled gift. Please try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            model.Accounts = work.Payment.GetPaymentMethodsDropdownList(model.DonorGUID);
            model.Campuses = work.Campus.GetCampusSelectList(SessionVariables.Campuses);
            model.Funds = work.Fund.GetDigitalFundsByChurch(model.ChurchId);

            return View(model);
        }

        public ActionResult EditScheduled()
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage(SessionVariables.CurrentChurch.Display + EnabledGivingMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return RedirectToAction("Index", "Home");
            }

            var scheduledPayments = work.ScheduledPayment.GetAll(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id);

            return View(scheduledPayments);
        }

        public ActionResult DeleteScheduleGift(string id)
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage(SessionVariables.CurrentChurch.Display + EnabledGivingMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return RedirectToAction("Index", "Home");
            }

            work.ScheduledPayment.Deactivate(id);

            CreateAlertMessage("Your scheduled gift has been canceled.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return AjaxRedirectTo("/MyGiving");
        }
        #endregion

        #region Giving History
        public ActionResult History(string startDate, string endDate, string fundId, string campusId)
        {
            var dashboard = work.Giving.GetHistory(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, null, startDate, endDate, fundId, campusId);
            return View(dashboard.Data);
        }

        public ActionResult DownloadGivingHistory(string startDate, string endDate, string fundId, string campusId)
        {
            var payments = work.Giving.GetHistory(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, null, startDate, endDate, fundId, campusId);
            var rows = new List<string>() { "Date,Fund,Campus,Payment Method,Amount" };

            foreach (var payment in payments.Data.MyGiving)
            {
                var columns = new List<string> { payment.CreatedDate.ToShortDateString() };

                //Add Fund Name
                if (!string.IsNullOrEmpty(payment.FundId))
                {
                    var fundName = payments.Data.Funds.FirstOrDefault(x => x.Id.Equals(payment.FundId))?.Name ?? string.Empty;
                    columns.Add(fundName);
                }
                else
                {
                    columns.Add(string.Empty);
                }

                //Add Campus Name
                if (!string.IsNullOrEmpty(payment.CampusId))
                {
                    var campusName = SessionVariables.Campuses.FirstOrDefault(x => x.Id.Equals(payment.CampusId))?.Name ?? string.Empty;
                    columns.Add(campusName);
                }
                else
                {
                    columns.Add(string.Empty);
                }

                var paymentMethod = "-";

                if (payment.OfflinePaymentMethod.IsNotNullOrEmpty())
                {
                    paymentMethod = payment.OfflinePaymentMethod;

                    if (payment.OfflinePaymentMethod == OfflinePaymentMethods.Check)
                    {
                        paymentMethod = $"Check # {payment.CheckNumber}";
                    }
                }
                else
                {
                    if (payments.Data.PaymentMethods.Any(x => x.AccountGUID == payment.PaymentMethod))
                    {
                        paymentMethod = payments.Data.PaymentMethods.FirstOrDefault(x => x.AccountGUID == payment.PaymentMethod)?.PaymentMethodPreview;
                    }
                }

                columns.Add(paymentMethod);
                columns.Add(payment.Amount.ToCurrencyString());
                rows.Add(string.Join(",", columns.Select(x => "\"" + x + "\"")));
            }

            var totalsRow = new List<string>() { "Total", string.Empty, string.Empty, string.Empty, payments.Data.MyGiving.Sum(x => x.Amount).ToCurrencyString() };
            rows.Add(string.Join(",", totalsRow.Select(x => "\"" + x + "\"")));

            var data = string.Join("\r\n", rows);
            var suggestedFilename = $"givinghistory_{SessionVariables.CurrentChurch.Name.FilenameFriendlyLower()}.csv";

            Utilities.AddToCookies("loadingProcess", DateTime.Now.AddHours(1), true);

            return ExportToCsv(data, suggestedFilename);
        }
        #endregion

        #region Giving Statements
        [HttpGet]
        public ActionResult EnablePaperlessGivingStatements()
        {
            var userSettings = SessionVariables.CurrentUser.Settings;

            if (userSettings.PaperlessGiving)
            {
                userSettings.PaperlessGiving = false;
                CreateAlertMessage("You've opted out of receiving paperless giving statements.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                userSettings.PaperlessGiving = true;
                CreateAlertMessage("You've opted in to receiving paperless giving statements.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            userSettings.ModifiedDate = DateTime.Now;
            userSettings.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            work.UserSetting.Update(userSettings);

            return AjaxRedirectTo("/mygiving/history");
        }

        [HttpGet]
        public ActionResult Statements()
        {
            var paymentYears = work.Payment.GetFilteredPaymentYears(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id);

            return View(paymentYears);
        }

        public ActionResult GivingStatementPDF(int year)
        {
            return new PartialViewAsPdf("DownloadGivingStatementPDF", GetGivingStatement(year, SessionVariables.CurrentChurch, SessionVariables.CurrentUser.User));
        }

        public ActionResult DownloadGivingStatementPDF(int year)
        {
            return new PartialViewAsPdf(GetGivingStatement(year, SessionVariables.CurrentChurch, SessionVariables.CurrentUser.User))
            {
                FileName = $"{$"{year}AnnualGivingStatement_{SessionVariables.CurrentChurch.Display}".FilenameFriendly()}.pdf"
            };
        }

        [AllowAnonymous]
        public ActionResult Preview(string token)
        {
            if (token.IsNullOrEmpty())
            {
                return Json("Uh-oh! This link has expired.", JsonRequestBehavior.AllowGet);
            }

            var _token = WebUtility.UrlDecode(token).Decrypt().Split('-').ToArray();

            if (_token.IsNullOrEmpty() || _token.Length < 3 || _token[0].IsNullOrEmpty() || _token[1].IsNullOrEmpty() || _token[2].IsNullOrEmpty())
            {
                return Json("Uh-oh! This link has expired.", JsonRequestBehavior.AllowGet);
            }

            var user = work.User.Get(_token[0].Trim());
            var year = Convert.ToInt32(_token[1].Trim());
            var church = work.Church.Get(_token[2].Trim());

            return new PartialViewAsPdf("DownloadGivingStatementPDF", GetGivingStatement(year, church, user));
        }

        private GivingStatementVM GetGivingStatement(int year, Church church, ApplicationUser user)
        {
            var payments = work.Payment.GetAllForYear(church.Id, year, user.Id).OrderBy(x => x.CreatedDate).ToList();
            var funds = work.Fund.GetAll(church.Id);
            var paymentMethod = payments.Select(x => x.PaymentMethod).Distinct().ToList();
            var PaymentMethodAccount = work.PaymentMethodAccount.GetAllByPaymentMethod(paymentMethod);
            var statementVM = new GivingStatementVM
            {
                User = user,
                Year = year,
                Church = work.Church.Get(church.Id),
                Total = payments.Select(x => x.Amount).Sum()
            };
            statementVM.Church.Logo = statementVM.Church.Logo; //HtmlHelpers.AmazonLink(statementVM.Church.Logo, "Uploads/Logos");
            statementVM.Statement = new List<GivingStatementModel>();
            statementVM.Statement.AddRange(payments.Select(x => new GivingStatementModel()
            {
                Date = x.CreatedDate.ToShortDateString(),
                Amount = x.Amount.ToCurrencyString(),
                Fund = x.FundId.IsNotNullOrEmpty() ? funds.FirstOrDefault(f => f.Id.Equals(x.FundId))?.Name ?? string.Empty : string.Empty,
                Method = x.PaymentMethod.IsNotNullOrEmpty() ? PaymentMethodAccount.FirstOrDefault(u => u.AccountGUID == x.PaymentMethod)?.PaymentMethodPreview ?? string.Empty : string.Empty
            }).ToList());

            return statementVM;
        }

        // here is the example to send statement email
        public void SendStatementViaEmail(int year)
        {
            var domain = ApplicationCache.Instance.EnvironmentConfiguration.Url;

            foreach (var user in work.Church.GetAllUsers())
            {
                var statementUrl = $"{domain}/statement/preview?t={Constants.GenerateStatementPreviewToken(user.UserId, user.ChurchId, year)}";
                var emailBody = $"<a href='{statementUrl}'>View giving statement</a>";

                // Email functionality here
            }
        }
        #endregion

        #region Helper Methods
        [AllowAnonymous]    //Why are we allowing anonymous?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public bool ClearCardExpiredNotification(string accountGUID)
        {
            return work.Giving.ClearCardExpiredNotification(accountGUID);
        }

        [AllowAnonymous]    //Why are we allowing anonymous?
        public ActionResult CalculateProcessingFee(string churchId, string amount, string accountGUID)
        {
            var PaymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(accountGUID);
            var fees = churchId.IsNotNullOrEmpty() && amount.IsNotNullOrEmpty() ? work.Giving.CalculateProcessingFee(churchId, amount, PaymentMethodAccount.IsNotNullOrEmpty() ? PaymentMethodAccount.AccountType : DigitalPaymentMethods.Card) : 0m;

            return Json(fees.ToCurrencyString(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MakePrimary(string id, bool updateScheduledPayments)
        {
            var donorGuid = SessionVariables.CurrentUser.UserMerchantAccounts.Find(x => x.Merchant.Equals(MerchantProviders.Nuvei)).DonorGUID;
            work.Giving.MakePrimary(id, donorGuid, updateScheduledPayments);
            CreateAlertMessage("Your primary account has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToActionPermanent(nameof(PaymentMethods));
        }

        public JsonResult CheckCardExpiration(string accountGUID)
        {
            var model = new CardExpirationModel();
            var paymentAccount = work.PaymentMethodAccount.GetByAccountGUID(accountGUID);

            if (paymentAccount != null)
            {
                var days = Utilities.CardExpirationCalculateInDays(paymentAccount.ExpMonth, paymentAccount.ExpYear);

                if (days <= 30)
                {
                    model.IsExpiring = true;
                    model.ExpiryDate = DateTime.Now.AddDays(days);
                    model.Message = $"Heads up! It looks like your card is expiring on {model.ExpiryDate.ToShortDateString()}. Do you want to choose a different payment method?";
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DeleteAccount(string accountID)
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage(SessionVariables.CurrentChurch.Display + EnabledGivingMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return RedirectToAction("Index", "MyGiving");
            }

            var donorGuid = SessionVariables.CurrentUser.UserMerchantAccounts.Find(x => x.Merchant.Equals(MerchantProviders.Nuvei)).DonorGUID;

            if (!string.IsNullOrEmpty(donorGuid) && !string.IsNullOrEmpty(accountID))
            {
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

                var paymentMethodAccountsList = work.PaymentMethodAccount.GetAll(donorGuid);
                var paymentMethodAccount = paymentMethodAccountsList.FirstOrDefault(x => x.AccountGUID == accountID);

                if (paymentMethodAccount.IsNotNullOrEmpty() && paymentMethodAccount.IsPrimary)
                {
                    CreateAlertMessage("Please specify another primary payment method before removing this one.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);

                    return RedirectToActionPermanent(nameof(PaymentMethods));
                }

                var scheduledPayments = work.ScheduledPayment.GetAllByMethod(accountID);

                if (scheduledPayments.Count > 0)
                {
                    var accountGuid = paymentMethodAccountsList.Where(x => x.IsPrimary).Select(x => x.AccountGUID).FirstOrDefault();

                    foreach (var item in scheduledPayments)
                    {
                        item.PaymentMethod = accountGuid;
                        item.ModifiedDate = DateTime.Now;
                        item.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        work.ScheduledPayment.Update(item);
                    }
                }

                //Delete from API
                var deleteResponse = await DeletePaymentAccountAsync(paymentMethodAccount.DonorGUID, paymentMethodAccount.AccountGUID, paymentMethodAccount.AccountType, apiCredentials);

                //Mark inactive in our tables. We don't delete so we can use in giving history
                if (paymentMethodAccount != null)
                {
                    paymentMethodAccount.IsActive = false;
                    paymentMethodAccount.ModifiedDate = DateTime.Now;
                    paymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.PaymentMethodAccount.Update(paymentMethodAccount);
                }

                CreateAlertMessage(PaymentMethodAlertMessages.PaymentMethodRemoveSuccess, AlertMessageTypes.Success, AlertMessageIcons.Success);
            }

            return RedirectToActionPermanent(nameof(PaymentMethods));
        }
        #endregion
    }
}