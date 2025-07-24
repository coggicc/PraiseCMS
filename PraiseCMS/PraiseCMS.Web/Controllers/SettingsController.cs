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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static PraiseCMS.Shared.Methods.ExtensionMethods;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    public class SettingsController : BaseController
    {
        //public SettingsController()
        //{
        //}

        [RequirePermission(ModuleId = "958595693250ac1c90e1ed47c98ba8")]
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Tab = "info";
            var settings = work.Church.GetSettings(SessionVariables.CurrentChurch.Id);

            return View(settings);
        }

        [RequirePermission(ModuleId = "958595693250ac1c90e1ed47c98ba8")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(SettingsViewModel model, HttpPostedFileBase ImageFile)
        {
            if (!ModelState.IsValid)
            {
                DisplayErrors();
                ViewBag.Tab = "info";
                return View("index", model);
            }
            else
            {
                EnsureWebsiteHasHttpPrefix(model.Church);

                model.Church.GivingAccountSetupCompleted = work.Church.UpdateChurchSetting(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, model);

                if (ImageFile?.ContentLength > 0)
                {
                    if (Utilities.IsImage(ImageFile.FileName))
                    {
                        var newFileName = Utilities.GenerateUniqueFileName(ImageFile.FileName);
                        var success = AwsHelpers.UploadImage(newFileName, ImageFile, "Uploads/Logos");

                        if (success)
                        {
                            if (!string.IsNullOrEmpty(model.Church.Logo))
                            {
                                var fileName = model.Church.Logo.Split('/');
                                AwsHelpers.DeleteImage(fileName[fileName.Length - 1], "Uploads/Logos");
                            }

                            model.Church.Logo = newFileName;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(model.Church.Logo))
                {
                    AwsHelpers.DeleteImage(model.Church.Logo, "Uploads/Logos");
                    model.Church.Logo = null;
                }

                await SetChurchLocationInfoAsync(model.Church);

                // Update current Church Merchant Account Tax ID if it is changed
                if (!string.IsNullOrEmpty(model.Church.TaxIdNumber) &&
                    SessionVariables.CurrentMerchant != null &&
                    !string.IsNullOrEmpty(SessionVariables.CurrentMerchant.TaxId) &&
                    !SessionVariables.CurrentMerchant.TaxId.Equals(model.Church.TaxIdNumber))
                {
                    SessionVariables.CurrentMerchant.TaxId = model.Church.TaxIdNumber;
                    SessionVariables.CurrentMerchant.ModifiedDate = DateTime.Now;
                    SessionVariables.CurrentMerchant.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.ChurchMerchantAccount.Update(SessionVariables.CurrentMerchant);
                }

                SessionVariables.CurrentChurch = model.Church;
                model.Church.ModifiedDate = DateTime.Now;
                model.Church.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.Church.Update(model.Church);
                CreateAlertMessage(Constants.SavedMessage, AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction("index", "settings");
            }
        }

        private void EnsureWebsiteHasHttpPrefix(Church church)
        {
            if (church.Website.IsNotNullOrEmpty() &&
                !church.Website.StartsWith(URLPrefixes.Http) &&
                !church.Website.StartsWith(URLPrefixes.Https))
            {
                church.Website = string.Concat(URLPrefixes.Http, church.Website);
            }
        }

        [RequirePermission(ModuleId = "1956319359bb82a242fa56496fb9d8")]
        [HttpGet]
        public ActionResult Giving()
        {
            ViewBag.Tab = "giving";
            var viewModel = work.ChurchMerchantAccount.GetChurchGivingAccountSettings(SessionVariables.CurrentChurch.Id);

            var requiredFields = new Dictionary<string, string>
            {
                { "Bank Account Type", viewModel.BankAccountType },
                { "Account Number", viewModel.AccountNumber },
                { "Routing Number", viewModel.RoutingNumber },
                { "First Name", viewModel.RespContactFirstName },
                { "Last Name", viewModel.RespContactLastName },
                { "Email", viewModel.RespContactEmail },
                { "Phone", viewModel.RespContactPhone },
                { "Date of Birth", viewModel.RespContactDOB?.ToString() },
                { "Social Security Number", viewModel.RespContactSSN },
                { "Driver's License Number", viewModel.RespContactDLN }
            };

            var missingFieldsList = requiredFields
                .Where(field => string.IsNullOrEmpty(field.Value))
                .Select(field => field.Key)
                .ToList();

            ViewBag.MissingFieldsList = missingFieldsList.CombineListToString();

            return View(viewModel);
        }

        [RequirePermission(ModuleId = "1956319359bb82a242fa56496fb9d8")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Giving(ChuchGivingAccountViewModel model)
        {
            if (TryValidateModel(model))
            {
                TempData["IsEditMode"] = false;

                var church = work.Church.Get(SessionVariables.CurrentChurch.Id);

                UpdateChurchDetails(model, church);

                UpdateChurchMerchantAccount(model);

                return RedirectToAction("giving", "settings");
            }
            else
            {
                DisplayErrors();
                TempData["IsEditMode"] = true;
                ViewBag.Tab = "giving";
                return View(model);
            }
        }

        private void UpdateChurchDetails(ChuchGivingAccountViewModel model, Church church)
        {
            church.SubscriptionFee = model.Church.SubscriptionFee;
            church.AllowDonorCoverProcessingFee = model.Church.AllowDonorCoverProcessingFee;
            church.ModifiedDate = DateTime.Now;
            church.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            work.Church.Update(church);
        }

        private void UpdateChurchMerchantAccount(ChuchGivingAccountViewModel model)
        {
            var churchMerchantAccount = work.ChurchMerchantAccount.Get(model.Id);

            if (churchMerchantAccount != null)
            {
                churchMerchantAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                churchMerchantAccount.ModifiedDate = DateTime.Now;
                churchMerchantAccount.Address = model.Address;
                churchMerchantAccount.Address2 = model.Address2;
                churchMerchantAccount.City = model.City;
                churchMerchantAccount.State = model.State;
                churchMerchantAccount.Zip = model.Zip;
                churchMerchantAccount.Country = model.Country;
                churchMerchantAccount.Phone = model.Phone;
                churchMerchantAccount.Email = model.Email;
                churchMerchantAccount.TaxId = model.TaxId;
                churchMerchantAccount.BankAccountType = model.BankAccountType;
                churchMerchantAccount.AccountNumber = model.AccountNumber.Encrypt();
                churchMerchantAccount.RoutingNumber = model.RoutingNumber.Encrypt();
                churchMerchantAccount.BusinessWebsite = model.BusinessWebsite;
                churchMerchantAccount.RespContactFirstName = model.RespContactFirstName;
                churchMerchantAccount.RespContactLastName = model.RespContactLastName;
                churchMerchantAccount.RespContactPhone = model.RespContactPhone;
                churchMerchantAccount.RespContactEmail = model.RespContactEmail;

                if (DateTime.TryParse(model.RespContactDOB, out var dob))
                {
                    churchMerchantAccount.RespContactDOB = dob;
                }
                else
                {
                    churchMerchantAccount.RespContactDOB = null; // or DateTime.MinValue or another default value
                }

                churchMerchantAccount.RespContactSSN = model.RespContactSSN.Encrypt();
                churchMerchantAccount.RespContactDLN = model.RespContactDLN;
                churchMerchantAccount.RespContactAddress1 = model.RespContactAddress1;
                churchMerchantAccount.RespContactAddress2 = model.RespContactAddress2;
                churchMerchantAccount.RespContactCity = model.RespContactCity;
                churchMerchantAccount.RespContactState = model.RespContactState;
                churchMerchantAccount.RespContactZip = model.RespContactZip;
                churchMerchantAccount.CardProcessingFee = model.CardProcessingFee;
                churchMerchantAccount.ACHProcessingFee = model.ACHProcessingFee;
                churchMerchantAccount.ApiUsername = model.ApiUsername.Encrypt();
                churchMerchantAccount.ApiPassword = model.ApiPassword.Encrypt();

                work.ChurchMerchantAccount.Update(churchMerchantAccount);
            }
        }

        [RequirePermission(ModuleId = "1956319359bb82a242fa56496fb9d8")]
        public ActionResult Account()
        {
            ViewBag.Tab = "account";
            var model = new SubscriptionViewModel
            {
                Church = SessionVariables.CurrentChurch
            };

            var subscriptionTypes = work.Subscription.GetAllSubscriptionTypes() ?? new List<SubscriptionType>();
            var churchSubscriptions = work.Subscription.GetAll(SessionVariables.CurrentChurch.Id) ?? new List<Subscription>();
            var currentSubscription = churchSubscriptions.FirstOrDefault(x => x.IsActive);

            if (currentSubscription != null)
            {
                var standardPlanId = subscriptionTypes.FirstOrDefault(x => x.Name == PlanType.Premium)?.Id;
                var freePlanId = subscriptionTypes.FirstOrDefault(x => x.Name == PlanType.Free)?.Id;

                if (currentSubscription.PlanTypeId == standardPlanId)
                {
                    model.IsTrialPeriod = currentSubscription.FreeTrial;
                    model.IsPaidPlan = true;
                    model.EndDate = currentSubscription.EndDate;
                }
                else if (currentSubscription.PlanTypeId == freePlanId)
                {
                    model.IsPaidPlan = false;
                }

                model.StartDate = currentSubscription.StartDate;
            }
            else
            {
                model.IsPaidPlan = false;
            }

            model.IsCancelable = churchSubscriptions.Any(x => x.IsActive && x.BillingPlan != BillingType.Free)
                || churchSubscriptions.Any(x => x.StartDate == null && x.EndDate == null && !x.IsActive && x.BillingPlan != BillingType.Free);

            if (currentSubscription == null || currentSubscription.BillingPlan == BillingType.Free)
            {
                model.ShowActivationButton = true;
                model.IsCancelable = false;
            }

            return View(model);
        }

        public ActionResult ActivateSubscription()
        {
            var church = work.Church.Get(SessionVariables.CurrentChurch.Id);
            var praiseChurch = work.Church.GetPraiseChurch();

            if (praiseChurch.IsNotNullOrEmpty())
            {
                var fund = work.Fund.GetByName(praiseChurch.Id, PraiseFunds.Subscriptions);

                if (fund.IsNotNullOrEmpty())
                {
                    var annual = Utilities.GetAnnualSubscriptionAmount();
                    var totalAnnual = annual * 12;

                    var model = new ActivateSubscriptionVM
                    {
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        FundId = fund.Id,
                        Accounts = work.Payment.GetPaymentMethodsDropdownList(church.DonorGUID),
                        DonorGUID = church.DonorGUID,
                        MonthlyBillingAmount = Utilities.GetMonthlySubscriptionAmount().ToCurrencyString(),
                        AnnualBillingAmount = annual.ToCurrencyString(),
                        TotalAnnualBillingAmount = totalAnnual.ToCurrencyString()
                    };

                    return PartialView("_ActivateSubscription", model);
                }
            }

            CreateAlertMessage("Uh-oh! No subscription fund was found. Please try again later", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return RedirectToAction(nameof(Account));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateSubscription(ActivateSubscriptionVM model)
        {
            if (ModelState.IsValid)
            {
                var praiseMerchantAccount = work.ChurchMerchantAccount.GetPraiseChurchAccount();

                //API credentials need to be for Praise Church since this is for the church paying Praise
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

                var church = work.Church.Get(model.ChurchId);

                if (!model.FreeTrialAvailable)
                {
                    switch (model.BillingType)
                    {
                        case BillingType.Monthly:
                            model.Amount = Utilities.GetMonthlySubscriptionAmount();
                            break;
                        case BillingType.Annually:
                            model.Amount = Utilities.GetAnnualSubscriptionAmount();
                            break;
                    }

                    var givingVM = new GivingViewModel()
                    {
                        ChurchId = model.ChurchId,
                        Payment = new Payment()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Amount = model.Amount,
                            Merchant = praiseMerchantAccount.IsNotNull() ? praiseMerchantAccount.Merchant : "Merchant Name Not Defined",
                            MerchantId = praiseMerchantAccount.IsNotNull() ? praiseMerchantAccount.MerchantAccountId : string.Empty,
                            ChurchId = SessionVariables.CurrentChurch.Id,
                            UserId = SessionVariables.CurrentUser.IsNotNull() ? SessionVariables.CurrentUser.User.Id : string.Empty,
                            FundId = model.FundId,
                            TransactionType = TransactionType.Payment,
                            Frequency = PaymentOccurrence.Recurring,
                            CreatedDate = DateTime.Now,
                            CreatedBy = SessionVariables.CurrentUser.IsNotNull() ? SessionVariables.CurrentUser.User.Id : string.Empty,
                        },
                        DonorGUID = model.DonorGUID,
                        AccountGUID = model.AccountGUID
                    };

                    var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(givingVM.AccountGUID);

                    var transactionResponse = new TransactionResponse();

                    if (paymentMethodAccount.AccountType == DigitalPaymentMethods.Card)
                    {
                        var cardTransactionRequest = new CardTransactionRequest
                        {
                            tokenized_card = new TokenizedCard
                            {
                                card_info_key = model.AccountGUID,
                                amount = model.Amount.ToString(),
                                transaction_type = TransactionTypeShortCode.Auth
                            }
                        };

                        transactionResponse = await nuveiHelper.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials);

                        givingVM.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                        givingVM.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                        givingVM.Payment.PaymentMethod = model.AccountGUID;

                        var result = work.Payment.Create(givingVM.Payment);
                        givingVM.Payment = result.Data;

                        work.Giving.SendPaymentStatusEmail(givingVM, transactionResponse);
                    }
                    else
                    {
                        var checkTransactionRequest = new CheckTransactionRequest
                        {
                            tokenized_check = new TokenizedCheck
                            {
                                check_info_key = model.AccountGUID,
                                amount = model.Amount.ToString(),
                                transaction_type = TransactionTypeShortCode.Auth
                            }
                        };

                        transactionResponse = await nuveiHelper.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials);

                        givingVM.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                        givingVM.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;

                        var result = work.Payment.Create(givingVM.Payment);
                        givingVM.Payment = result.Data;

                        work.Giving.SendPaymentStatusEmail(givingVM, transactionResponse);
                    }

                    var activeSubscription = work.Subscription.GetActiveSubscription(model.ChurchId);

                    if (activeSubscription.IsNotNullOrEmpty())
                    {
                        activeSubscription.IsActive = false;
                        activeSubscription.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        activeSubscription.ModifiedDate = DateTime.Now;

                        if (activeSubscription.EndDate.IsNullOrEmpty())
                        {
                            activeSubscription.EndDate = DateTime.Now;
                        }

                        work.Subscription.Update(activeSubscription);
                    }

                    var planType = work.Subscription.GetSubscriptionTypeByName(PlanType.Premium);
                    var subscription = new Subscription()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = model.ChurchId,
                        PlanTypeId = planType.Id,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        FreeTrial = false,
                        StartDate = DateTime.Now,
                        EndDate = model.BillingType.EqualsIgnoreCase(BillingType.Monthly) ? DateTime.Now.AddMonths(1).AddDays(-1) : DateTime.Now.AddYears(1).AddDays(-1),
                        BillingPlan = model.BillingType,
                        IsActive = true
                    };
                    work.Subscription.Create(subscription);

                    var transaction = new SubscriptionTransaction()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Amount = model.Amount,
                        BillingType = model.BillingType,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        PaymentMethod = model.AccountGUID,
                        SubscriptionId = subscription.Id,
                        TransactionDate = DateTime.Now,
                        AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty
                    };
                    work.Subscription.CreateTransaction(transaction);

                    CreateAlertMessage("Congratulations, your subscription has been activated!", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    work.Subscription.SendRenewalEmail(subscription, transaction, church);

                    model.Success = true;
                }
                else
                {
                    var activeSubscription = work.Subscription.GetActiveSubscription(model.ChurchId);

                    if (activeSubscription.IsNotNullOrEmpty())
                    {
                        activeSubscription.IsActive = false;
                        activeSubscription.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        activeSubscription.ModifiedDate = DateTime.Now;

                        if (activeSubscription.EndDate.IsNullOrEmpty())
                        {
                            activeSubscription.EndDate = DateTime.Now;
                        }

                        work.Subscription.Update(activeSubscription);
                    }

                    var planType = work.Subscription.GetSubscriptionTypeByName(PlanType.Premium);
                    var subscription = new Subscription()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = model.ChurchId,
                        PlanTypeId = planType.Id,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        FreeTrial = true,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddDays(30),
                        BillingPlan = BillingType.Monthly,
                        IsActive = true
                    };

                    work.Subscription.Create(subscription);

                    CreateAlertMessage(
                        $"Congratulations, your free trial has started! After {Utilities.GetFreeTrialDays()} days, your trial will automatically convert to our premium plan, billed monthly.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    work.Subscription.FreeTrialEmail(message: SubscriptionNotificationMessages.FreeTrialStartedAutoRenew, subject: SubscriptionNotificationSubjects.FreeTrialStarted, church);

                    model.Success = true;
                }

                if (!model.Success) return AjaxRedirectTo("/Settings/Account");

                //check if user any change in plan before buying subscription then delete it
                var nextSubscription = work.Subscription.GetNextSubscription(model.ChurchId);

                if (nextSubscription == null) return AjaxRedirectTo("/Settings/Account");

                work.Subscription.Delete(nextSubscription);
                work.Subscription.SaveChanges();

                return AjaxRedirectTo("/Settings/Account");
            }

            if (model.DonorGUID.IsNullOrEmpty())
            {
                var church = work.Church.Get(model.ChurchId);
                model.DonorGUID = church.DonorGUID;
            }

            // Repopulate view model fields that aren't posted back from the form
            var annual = Utilities.GetAnnualSubscriptionAmount();
            var totalAnnual = annual * 12;

            model.Accounts = work.Payment.GetPaymentMethodsDropdownList(model.DonorGUID);
            model.MonthlyBillingAmount = Utilities.GetMonthlySubscriptionAmount().ToCurrencyString();
            model.AnnualBillingAmount = annual.ToCurrencyString();
            model.TotalAnnualBillingAmount = totalAnnual.ToCurrencyString();

            var error = ModelState.Values.Any(q => q.Errors.Any()) ? ModelState.Values.FirstOrDefault(q => q.Errors.Any())?.Errors.FirstOrDefault()?.ErrorMessage : "Please fill out all of the required fields.";
            CreateAlertMessage(error, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            model.Accounts = work.Payment.GetPaymentMethodsDropdownList(model.DonorGUID);

            return PartialView("_ActivateSubscription", model);
        }

        #region Payment Methods
        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        public async Task<ActionResult> PaymentMethods()
        {
            ViewBag.Tab = "account";

            //If this church does not have a donor account, create one.
            if (string.IsNullOrEmpty(SessionVariables.CurrentChurch.DonorGUID))
            {
                SessionVariables.CurrentChurch = await work.Giving.CreateChurchDonorAccount(SessionVariables.CurrentChurch);
            }

            if (SessionVariables.CurrentChurch.IsNullOrEmpty()) return View(new PaymentMethodViewModel());

            var methods = work.Payment.GetPaymentMethods(SessionVariables.CurrentUser.User, SessionVariables.CurrentUser.UserMerchantAccounts, SessionVariables.CurrentChurch.DonorGUID, forChurch: true);

            return View(methods);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpGet]
        public ActionResult AddCard()
        {
            ViewBag.Tab = "account";

            var model = new PaymentMethodViewModel
            {
                User = SessionVariables.CurrentUser.User,
                PaymentMethod = DigitalPaymentMethods.Card
            };

            return View("_AddEditCard", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCard(PaymentMethodViewModel model)
        {
            if (TryValidateModel(model.PaymentCard))
            {
                var praiseMerchantAccount = work.ChurchMerchantAccount.GetPraiseChurchAccount();

                //API credentials need to be for Praise Church since this is for the church paying Praise
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

                model.ChurchId = SessionVariables.CurrentChurch.Id;
                model.DonorGUID = SessionVariables.CurrentChurch.DonorGUID;

                if (await work.Giving.CardExistsForChurch(model, apiCredentials))
                {
                    CreateAlertMessage(PaymentMethodAlertMessages.CardExists, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View("_AddEditCard", model);
                }

                //If church's donor guid is null, create new donor account.
                if (string.IsNullOrEmpty(model.DonorGUID))
                {
                    //Need to determine the billing details of the person responsible for the subscription payment.Ask the user for this in the UI.
                    //var donor = work.Giving.CreateDonorAccount(SessionVariables.ch)
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
                    return View("_AddEditCard", model);
                }

                if (!string.IsNullOrEmpty(cardData.card_key))
                {
                    var accountGUID = cardData.card_key;
                    var donorGUID = cardData.customer_key;
                    var method = model.PaymentCard.NickName.IsNotNullOrEmpty() ? model.PaymentCard.NickName : model.PaymentCard.CcType;
                    var paymentMethodAccount = new PaymentMethodAccount
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Type = PaymentMethodAccountTypes.Church,
                        TypeId = SessionVariables.CurrentChurch.Id,
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

                    var content = Shared.Shared.EmailTemplates.PaymentMethod_body.Replace("{message}", PaymentMethodAlertMessages.CardAddSuccess)
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

            return View("_AddEditCard", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpGet]
        public async Task<ActionResult> EditCard(string id)
        {
            ViewBag.Tab = "account";

            var praiseMerchantAccount = work.ChurchMerchantAccount.GetPraiseChurchAccount();
            var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(id);

            //API credentials need to be for Praise Church since this is for the church paying Praise
            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

            var model = await PaymentMethodHelpers.GetEditCardViewModel(paymentMethodAccount, apiCredentials, this, work, nuveiHelper);

            if (model == null)
            {
                return RedirectToAction(nameof(PaymentMethods));
            }

            return View("_AddEditCard", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCard(PaymentMethodViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PaymentCard.NickName))
            {
                var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(model.PaymentCard.AccountGUID);

                if (paymentMethodAccount == null) return View("_AddEditCard", model);

                paymentMethodAccount.NickName = model.PaymentCard.NickName;
                paymentMethodAccount.ModifiedDate = DateTime.Now;
                paymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.PaymentMethodAccount.Update(paymentMethodAccount);

                CreateAlertMessage($"The nickname for card ending in {model.PaymentCard.CcNumber.GetLastCharacters(4)} has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction(nameof(PaymentMethods));
            }

            CreateAlertMessage("Please enter a nickname for your card.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return View("_AddEditCard", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpGet]
        public ActionResult AddBank()
        {
            ViewBag.Tab = "account";

            var model = new PaymentMethodViewModel
            {
                User = SessionVariables.CurrentUser.User,
                PaymentMethod = DigitalPaymentMethods.ACH
            };

            return View("_AddEditBank", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddBank(PaymentMethodViewModel model)
        {
            if (ValidatePaymentModel(model.PaymentAccount))
            {
                var praiseMerchantAccount = work.ChurchMerchantAccount.GetPraiseChurchAccount();

                //API credentials need to be for Praise Church since this is for the church paying Praise
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

                model.DonorGUID = SessionVariables.CurrentChurch.DonorGUID;
                model.ChurchId = praiseMerchantAccount.ChurchId;

                var verifyRouting = await nuveiHelper.VerifyBankRoutingAsync(model.PaymentAccount.RoutingNumber, apiCredentials);

                var verifyRoutingResponse = Responses.GetApiTransactionResponse(verifyRouting?.result);

                //Problem with routing number
                if (verifyRoutingResponse != APIStatuses.Success)
                {
                    CreateAlertMessage(verifyRouting.result_message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(model);
                }

                var bankName = verifyRouting.aba.bank_name;

                if (await work.Giving.CheckAccountExistsForChurch(model, apiCredentials))
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
                        return View("_AddEditBank", model);
                    }

                    if (!string.IsNullOrEmpty(bankData.check_key))
                    {
                        var accountGUID = bankData.check_key;
                        var donorGUID = bankData.customer_key;
                        var method = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : "BANK";
                        var paymentMethodAccount = new PaymentMethodAccount
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Type = PaymentMethodAccountTypes.Church,
                            TypeId = SessionVariables.CurrentChurch.Id,
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

                        work.PaymentMethodAccount.Create(paymentMethodAccount);

                        var content = Shared.Shared.EmailTemplates.PaymentMethod_body.Replace("{message}", PaymentMethodAlertMessages.BankAccountAddSuccess)
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

                        CreateAlertMessage($"Your bank account ending in {model.PaymentAccount.AccountNumber.GetLastCharacters(4)} has been added.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                        return RedirectToAction(nameof(PaymentMethods));
                    }
                }
            }
            else
            {
                DisplayErrors();
            }

            return View("_AddEditBank", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpGet]
        public async Task<ActionResult> EditBank(string id)
        {
            ViewBag.Tab = "account";

            var praiseMerchantAccount = work.ChurchMerchantAccount.GetPraiseChurchAccount();

            //API credentials need to be for Praise Church since this is for the church paying Praise
            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

            var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(id);
            var bankAccount = new BankAccount();

            var checkResponse = await nuveiHelper.GetCheckDetailsAsync(paymentMethodAccount?.AccountGUID, apiCredentials);
            bool checksMatch = false;

            if (checkResponse?.result == "0" && checkResponse.paymentsafe_check != null)
            {
                checksMatch = paymentMethodAccount?.AccountGUID == checkResponse.paymentsafe_check.check_key;
                bankAccount.AccountGUID = paymentMethodAccount?.AccountGUID;
                bankAccount.AccountNumber = paymentMethodAccount?.PaymentMethodPreview;
                bankAccount.AccountType = paymentMethodAccount?.AccountSubType;
                bankAccount.Nickname = paymentMethodAccount?.NickName;
            }

            if (!checksMatch)
            {
                if (paymentMethodAccount != null)
                {
                    paymentMethodAccount.IsActive = false;
                    paymentMethodAccount.ModifiedDate = DateTime.Now;
                    paymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.PaymentMethodAccount.Update(paymentMethodAccount);
                }

                CreateAlertMessage($"Your bank account ending in {paymentMethodAccount?.PaymentMethodPreview} could not be found and has been removed from this page. Please try adding this payment method again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

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
                    NickName = paymentMethodAccount.NickName,
                    RoutingNumber = bankAccount.RoutingNumber,
                    AccountGUID = bankAccount.AccountGUID
                }
            };

            return View("_AddEditBank", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBank(PaymentMethodViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PaymentAccount.NickName))
            {
                var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(model.PaymentAccount.AccountGUID);

                if (paymentMethodAccount == null) return View("_AddEditBank", model);

                paymentMethodAccount.NickName = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : work.Giving.GetNickNameByType(model.PaymentAccount.AccountType);
                paymentMethodAccount.ModifiedDate = DateTime.Now;
                paymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.PaymentMethodAccount.Update(paymentMethodAccount);

                CreateAlertMessage($"The nickname for bank account ending in {model.PaymentAccount.AccountNumber.GetLastCharacters(4)} has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction(nameof(PaymentMethods));
            }

            CreateAlertMessage("Please enter a nickname for your bank account.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return View("_AddEditBank", model);
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        [HttpGet]
        public ActionResult MakePrimary(string id)
        {
            work.Giving.MakePrimary(id, SessionVariables.CurrentChurch.DonorGUID);
            CreateAlertMessage("Your primary account has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToActionPermanent(nameof(PaymentMethods));
        }

        [RequirePermission(ModuleId = "067175945827cfd6d056b14a2fabca")]
        public async Task<ActionResult> DeleteAccount(string accountID)
        {
            //Even if we can't delete from the API, we will mark as inactive in PaymentMerchantAccount so it will be unavailable.
            var praiseMerchantAccount = work.ChurchMerchantAccount.GetPraiseChurchAccount();

            //API credentials need to be for Praise Church since this is for the church paying Praise
            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

            if (string.IsNullOrEmpty(accountID)) return RedirectToActionPermanent(nameof(PaymentMethods));

            var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(accountID);

            if (paymentMethodAccount.IsNotNullOrEmpty() && paymentMethodAccount.IsPrimary)
            {
                CreateAlertMessage("Please specify another primary account before removing this one.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                return RedirectToActionPermanent(nameof(PaymentMethods));
            }

            //Delete from API
            var deleteResponse = await DeletePaymentAccountAsync(paymentMethodAccount.DonorGUID, paymentMethodAccount.AccountGUID, paymentMethodAccount.AccountType, apiCredentials);

            //We mark inactive, or soft delete, so we can still reference in giving history reports
            if (paymentMethodAccount != null)
            {
                paymentMethodAccount.IsActive = false;
                paymentMethodAccount.IsPrimary = false;
                paymentMethodAccount.ModifiedDate = DateTime.Now;
                paymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.PaymentMethodAccount.Update(paymentMethodAccount);
            }

            CreateAlertMessage(PaymentMethodAlertMessages.PaymentMethodRemoveSuccess, AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToActionPermanent(nameof(PaymentMethods));
        }
        #endregion

        [RequirePermission(ModuleId = "1091707272f1ca3ef8c55440408ef2")]
        [HttpGet]
        public ActionResult ChangePlan()
        {
            ViewBag.Tab = "account";
            var plans = work.Subscription.GetAllSubscriptionTypes();
            var allSubscription = work.Subscription.GetAll(SessionVariables.CurrentChurch.Id);
            var subscription = allSubscription.FirstOrDefault(q => q.IsActive);

            if (plans != null && subscription != null)
            {
                if (subscription.PlanTypeId.Equals(plans.FirstOrDefault(q => q.Name.Equals(PlanType.Premium))?.Id) &&
                    subscription.BillingPlan.Equals(BillingType.Annually))
                {
                    ViewBag.CurrentPlan = BillingType.Annually;
                }
                else if (subscription.PlanTypeId.Equals(plans.FirstOrDefault(q => q.Name.Equals(PlanType.Premium))
                    ?.Id) && subscription.BillingPlan.Equals(BillingType.Monthly))
                {
                    ViewBag.CurrentPlan = BillingType.Monthly;
                }
                else if (subscription.PlanTypeId.Equals(plans.FirstOrDefault(q => q.Name.Equals(PlanType.Free))?.Id))
                {
                    ViewBag.CurrentPlan = BillingType.Free;
                }
            }

            if (allSubscription.Any(q => q.StartDate.IsNullOrEmpty() && q.EndDate.IsNullOrEmpty() && !q.IsActive))
            {
                var nextPlan = allSubscription.FirstOrDefault(q => q.StartDate.IsNullOrEmpty() && q.EndDate.IsNullOrEmpty() && !q.IsActive);

                if (nextPlan != null && subscription != null)
                {
                    CreateAlertMessage(
                        $"Your plan will change to the <strong>{nextPlan.BillingPlan}</strong> subscription plan on <strong>{Convert.ToDateTime(subscription.EndDate).ToShortDateString()}</strong>.",
                        AlertMessageTypes.Primary, AlertMessageIcons.Primary);
                }
            }

            return View();
        }

        [RequirePermission(ModuleId = "1091707272f1ca3ef8c55440408ef2")]
        [HttpPost]
        public ActionResult ChangePlan(string plan, string churchId, bool isCancelled = false)
        {
            var church = work.Church.Get(churchId);
            var amount = 0m;

            if (plan.IsNotNullOrEmpty())
            {
                var newPlan = BillingType.Free;

                switch (plan.ToUpper())
                {
                    case BillingType.Monthly:
                        newPlan = BillingType.Monthly;
                        amount = Utilities.GetMonthlySubscriptionAmount();
                        break;
                    case BillingType.Annually:
                        newPlan = BillingType.Annually;
                        amount = Utilities.GetAnnualSubscriptionAmount();
                        break;
                    case BillingType.Free:
                        newPlan = BillingType.Free;
                        break;
                }

                var subscriptionType = work.Subscription.GetAllSubscriptionTypes();
                var allSubscription = work.Subscription.GetAll(churchId);
                var activePlan = allSubscription.FirstOrDefault(q => q.IsActive);

                if (allSubscription.Any(q => q.StartDate.IsNullOrEmpty() && q.EndDate.IsNullOrEmpty() && !q.IsActive))
                {
                    var nextPlan = allSubscription.FirstOrDefault(q => q.StartDate.IsNullOrEmpty() && q.EndDate.IsNullOrEmpty() && !q.IsActive);

                    if (activePlan?.BillingPlan.EqualsIgnoreCase(newPlan) == true)
                    {
                        work.Subscription.Delete(nextPlan);
                    }
                    else
                    {
                        if (nextPlan != null)
                        {
                            nextPlan.BillingPlan = newPlan;
                            nextPlan.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                            nextPlan.ModifiedDate = DateTime.Now;
                        }
                    }

                    work.Subscription.SaveChanges();
                }
                else
                {
                    var nextPlan = new Subscription()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = churchId,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        FreeTrial = false,
                        BillingPlan = newPlan,
                        IsActive = false
                    };

                    if (subscriptionType != null)
                    {
                        if (newPlan.Equals(BillingType.Free) && subscriptionType.Any(q => q.Name.EqualsIgnoreCase(PlanType.Free)))
                        {
                            nextPlan.PlanTypeId = subscriptionType.FirstOrDefault(q => q.Name.EqualsIgnoreCase(PlanType.Free))?.Id;
                        }
                        else if ((newPlan.Equals(BillingType.Monthly) || newPlan.Equals(BillingType.Annually)) && subscriptionType.Any(q => q.Name.EqualsIgnoreCase(PlanType.Premium)))
                        {
                            nextPlan.PlanTypeId = subscriptionType.FirstOrDefault(q => q.Name.EqualsIgnoreCase(PlanType.Premium))?.Id;
                        }
                    }

                    work.Subscription.Create(nextPlan);
                }

                if (isCancelled)
                {
                    work.Subscription.SubscriptionCanceledEmail(activePlan, church);
                    CreateAlertMessage($"Your Praise CMS subscription has been canceled. You will continue to have access until {Convert.ToDateTime(activePlan.EndDate):MMMM dd, yyyy}. You may renew your subscription at any time.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                }
                else
                {
                    var churchAdmin = work.User.Get(id: church.PrimaryUserId);
                    var startDate = activePlan?.EndDate.IsNotNullOrEmpty() == true ? Convert.ToDateTime(activePlan.EndDate).AddDays(1) : DateTime.Now;
                    var endDate = string.Empty;

                    switch (plan.ToUpper())
                    {
                        case BillingType.Monthly:
                            endDate = startDate.AddMonths(1).AddDays(-1).ToShortDateString();
                            break;
                        case BillingType.Annually:
                            endDate = startDate.AddYears(1).AddDays(-1).ToShortDateString();
                            break;
                        case BillingType.Free:
                            endDate = "-";
                            break;
                    }

                    var content = Shared.Shared.EmailTemplates.SubscriptionPlanChange_body
                        .Replace("{church_name}", church.Display)
                        .Replace("{plan_name}", plan.ToLower())
                        .Replace("{renew_url}", "{site-url}" + $"/settings/changeplan/{church.Id}")
                        .Replace("{subscription_type}", plan.EqualsIgnoreCase(BillingType.Free) ? "Basic" : "Premium")
                        .Replace("{monthly_price}", Utilities.GetMonthlySubscriptionAmount().ToCurrencyString())
                        .Replace("{annually_price}", Utilities.GetAnnualSubscriptionAmount().ToCurrencyString());

                    if (plan.EqualsIgnoreCase(BillingType.Free))
                    {
                        content = content.Replace("{subscription_period}", string.Empty); //Empty since it does not expire
                        content = content.Replace("{expiry_period}", string.Empty);   //Empty since it does not expire
                    }
                    else
                    {
                        var expiration = plan.EqualsIgnoreCase(BillingType.Monthly) ? "1 month" : "12 months";
                        content = content.Replace("{subscription_period}", "(" + expiration + ")");
                        content = content.Replace("{expiry_period}", $"Your plan will expire after {expiration.ToLower()}.");
                    }

                    if (churchAdmin.IsNotNullOrEmpty() && churchAdmin.Email.IsNotNullOrEmpty())
                    {
                        var email = new Email()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Message = content,
                            To = churchAdmin.Email,
                            Attachments = null,
                            Subject = "Praise CMS Subscription Plan Changed",
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now
                        };
                        Emailer.SendEmail(email);
                    }

                    CreateAlertMessage("Your plan has been updated. Your subscription will be changed based on your newly selected plan.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                }

                return RedirectToAction(nameof(Account));
            }

            CreateAlertMessage("Please select a plan.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return View();
        }

        [RequirePermission(ModuleId = "0900438452b4694f3753b24de3ad38")]
        public ActionResult BillingActivity()
        {
            ViewBag.Tab = "account";

            var result = work.Subscription.SubscriptionsWithTransactions(SessionVariables.CurrentChurch.Id).ToList();
            return View(result);
        }

        [RequirePermission(ModuleId = "0900438452b4694f3753b24de3ad38")]
        public ActionResult DownloadBillingPdf()
        {
            return new PartialViewAsPdf("_BillingActivityPartial", work.Subscription.SubscriptionsWithTransactions(churchId: SessionVariables.CurrentChurch.Id).ToList());
        }

        [RequirePermission(ModuleId = "0900438452b4694f3753b24de3ad38")]
        public ActionResult DownloadTransactionInvoice(string id)
        {
            var model = work.Subscription.TransactionInvoice(SessionVariables.CurrentChurch, id);

            return new PartialViewAsPdf("_TransactionInvoicePartial", model)
            {
                FileName = $"PraiseCMS_Invoice_{model.Transaction.TransactionDate:yyyy/MM/dd}.pdf"
            };
        }
        #region Modules

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult Modules()
        {
            var modules = work.Module.GetAll();
            return View(modules);
        }

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult AddEditModule(ModuleViewModel model)
        {
            try
            {
                if (model.Id == null)
                {
                    work.Module.Create(new Modules { Id = Utilities.GenerateUniqueId(), Name = model.Name, ParentId = model.ParentId });
                    return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);
                }

                work.Module.AddEdit(model);

                return Json(new { result = "updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { result = "exception" }, JsonRequestBehavior.AllowGet);
            }
        }

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [HttpGet]
        public ActionResult CreateModule()
        {
            var model = work.Module.GetAutoCompleteModel();
            return PartialView("_CreateEditModule", model);
        }

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateModule(ModuleAutoCompleteModel model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEditModule", model);

            var module = new Modules()
            {
                Id = model.Value,
                Name = model.Text,
                ParentId = model.ParentId
            };

            work.Module.Create(module);
            work.Module.SetModulePermissionForStandardPlan(module);
            work.Module.SetModulePermissionForSuperAdmin(module.Id, createRoleModule: module.ParentId.IsNullOrEmpty() || module.ParentId.Equals(module.Id));

            return AjaxRedirectTo("/settings/modules");
        }

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [HttpGet]
        [RequireRole(Role = Roles.SuperAdmin)]
        public PartialViewResult EditModule(string id)
        {
            var result = new ModuleAutoCompleteModel();
            var modules = work.Module.GetAll();

            result.Modules.Add(new ModuleAutoCompleteModel { Text = string.Empty, Value = null });

            foreach (var parent in modules.Where(x => x.ParentId == null).ToList())
            {
                var p = new ModuleAutoCompleteModel
                {
                    Value = parent.Id,
                    Text = parent.Name
                };
                result.Modules.Add(p);

                foreach (var module in modules.Where(x => x.ParentId == parent.Id).ToList())
                {
                    var m = new ModuleAutoCompleteModel
                    {
                        Value = module.Id,
                        Text = $"{parent.Name} > {module.Name}"
                    };
                    result.Modules.Add(m);
                }
            }

            var editModule = modules.Find(x => x.Id == id);

            if (editModule == null) return PartialView("_CreateEditModule", result);

            result.ParentId = editModule.ParentId;
            result.Text = editModule.Name;
            result.Value = editModule.Id;

            return PartialView("_CreateEditModule", result);
        }

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult EditModule(ModuleAutoCompleteModel model)
        {
            if (string.IsNullOrEmpty(model.Value))
            {
                var module = new Modules { Id = Utilities.GenerateUniqueId(), Name = model.Text, ParentId = model.ParentId };
                work.Module.Create(module);

                return AjaxRedirectTo("/settings/modules");
            }

            var currentModule = work.Module.Get(model.Value);
            var currentParentChildModules = work.Module.GetAllByIdAndParent(currentModule.Id, currentModule.ParentId);
            var hasChildModules = currentParentChildModules.Any(x => x.ParentId == currentModule.ParentId);

            currentModule.Name = model.Text;

            if ((currentModule.ParentId == null && !hasChildModules) || currentModule.ParentId != null)
            {
                currentModule.ParentId = model.ParentId;
            }

            work.Module.Update(currentModule);

            return AjaxRedirectTo("/settings/modules");
        }

        [RequirePermission(ModuleId = "1504032084908c05a6ca41443dac95")]
        [HttpGet]
        public ActionResult DeleteModule(string id)
        {
            var result = work.Module.Delete(id);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction("modules");
        }

        public PartialViewResult ViewModulePartial()
        {
            var modules = work.Module.GetAll();
            return PartialView("_ViewModulePartial", modules);
        }
        #endregion

        #region Email Templates
        [HttpGet]
        [RequirePermission(ModuleId = "25008088073d134767d33247a48c42")]
        public ActionResult EmailTemplates()
        {
            ViewBag.Tab = "email-templates";

            var model = new EmailTemplateVM
            {
                Church = work.Church.Get(SessionVariables.CurrentChurch.Id),
                EmailTemplates = work.EmailTemplate.GetEmailTemplates(SessionVariables.CurrentChurch.Id, true)
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult GetEmailTemplateByName(string value, bool includeUnverified = false)
        {
            var templateHtml = work.EmailTemplate.GetEmailTemplateByName(value, includeUnverified);
            return Json(templateHtml, JsonRequestBehavior.AllowGet);
        }

        [RequirePermission(ModuleId = "25008088073d134767d33247a48c42")]
        [HttpGet]
        public ActionResult CreateEmailTemplate()
        {
            var model = new EmailTemplate
            {
                Id = Utilities.GenerateUniqueId(),
                Type = EmailTemplateTypes.Church,
                TypeId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEditEmailTemplate", model);
        }

        [RequirePermission(ModuleId = "25008088073d134767d33247a48c42")]
        [HttpPost]
        public ActionResult CreateEmailTemplate(EmailTemplate model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEditEmailTemplate", model);

            work.EmailTemplate.Create(model);

            return AjaxRedirectTo("/settings/emailtemplates");
        }

        [RequirePermission(ModuleId = "25008088073d134767d33247a48c42")]
        [HttpGet]
        public ActionResult EditEmailTemplate(string id)
        {
            var model = work.EmailTemplate.Get(id);
            return PartialView("_CreateEditEmailTemplate", model);
        }

        [RequirePermission(ModuleId = "25008088073d134767d33247a48c42")]
        [HttpPost]
        public ActionResult EditEmailTemplate(EmailTemplate model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEditEmailTemplate", model);

            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = SessionVariables.CurrentUser?.User?.Id;
            work.EmailTemplate.Update(model);

            return AjaxRedirectTo("/settings/emailtemplates");
        }

        [RequirePermission(ModuleId = "25008088073d134767d33247a48c42")]
        [HttpGet]
        public ActionResult DeleteEmailTemplate(string id)
        {
            work.EmailTemplate.Delete(id);
            return RedirectToAction("emailtemplates");
        }
        #endregion

        [HttpGet]
        public ActionResult InvitationEmail()
        {
            var model = new InvitationEmailModel
            {
                InvitedBy = SessionVariables.CurrentUser.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Display : SessionVariables.CurrentChurch.Display
            };

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult InvitationEmail(InvitationEmailModel model)
        {
            if (ModelState.IsValid)
            {
                var content = Shared.Shared.EmailTemplates.InvitationEmail_body.Replace("{message}", model.Message).Replace("{btn_url}", ApplicationCache.Instance.SiteConfiguration.Url + "account/register");
                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = content,
                    To = model.Email,
                    Attachments = null,
                    Subject = $"{model.InvitedBy} invited you to join Praise CMS.",
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                };
                var result = Emailer.SendEmail(email);

                if (result)
                {
                    CreateAlertMessage("The invitation email has been sent.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return RedirectToAction(nameof(Index));
                }
            }

            CreateAlertMessage("Uh-oh! Something went wrong while sending the invitation mail. Please try again later.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return RedirectToAction(nameof(Index));
        }

        [RequirePermission(ModuleId = "9457561050842efe2f1a784e16a9ff")]
        [HttpGet]
        public ActionResult Implementations()
        {
            ViewBag.Tab = "implementations";

            var settings = work.Church.GetSettings(SessionVariables.CurrentChurch.Id);
            settings.GiveNowButton = GenerateGiveNowButtonCode(SessionVariables.CurrentChurch.Id);
            settings.GiveNotButtonStyling = Constants.GiveNowButtonStyling;
            settings.ExternalPrayerRequestCode = GeneratePrayerRequestCode(SessionVariables.CurrentChurch.Id);
            settings.ExternalMessageRequestCode = GenerateMessageRequestCode(SessionVariables.CurrentChurch.Id);

            return View(settings);
        }

        public string GenerateGiveNowButtonCode(string churchId)
        {
            var domain = ApplicationCache.Instance.EnvironmentConfiguration.Url;
            return $"<a href=\"{domain}/givingworkflow/startgiving?id={churchId}\" target=\"_blank\" class=\"giving_button_praise\">Give Online Now</a>";
        }

        public string GeneratePrayerRequestCode(string churchId)
        {
            var domain = ApplicationCache.Instance.EnvironmentConfiguration.Url;
            return $"<embed type=\"text/html\" src=\"{domain}/PrayerRequests/CreatePrayerRequestExternal?id={churchId}\" frameborder=\"0\" style=\"width:100%;height:1000px\" allowfullscreen=\"true\" mozallowfullscreen=\"true\" webkitallowfullscreen=\"true\"></embed>";
        }

        public string GenerateMessageRequestCode(string churchId)
        {
            var domain = ApplicationCache.Instance.EnvironmentConfiguration.Url;
            return $"<embed type=\"text/html\" src=\"{domain}/MessageRequests/CreateMessageRequestExternal?id={churchId}\" frameborder=\"0\" style=\"width:100%;height:1000px\" allowfullscreen=\"true\" mozallowfullscreen=\"true\" webkitallowfullscreen=\"true\"></embed>";
        }
    }
}