using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static PraiseCMS.Shared.Methods.ExtensionMethods;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.Web.Controllers
{
    public class GivingWorkflowController : BaseController
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult StartGiving(string id, bool guest = true, bool setUpRecurring = false, string selectedFundId = null)
        {
            //Prevent this if no merchant account has been set up for the church           
            var result = work.Giving.StartGiving(id, setUpRecurring, selectedFundId);

            if (result.ResultType == ResultType.Failure)
            {
                ViewBag.Message = result.Message;
            }
            else if (!guest)
            {
                result.Data.IsGuest = false;
            }

            return View(result.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> StartGiving(GivingSignUpViewModel model)
        {
            var message = string.Empty;

            if (decimal.TryParse(model.Amount, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out var amount))
            {
                model.Payment.Amount = amount;
            }

            var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(model.Payment.PaymentMethod);
            var processingFees = work.Giving.CalculateProcessingFee(model.Church.Id, model.Payment.Amount.ToString(), paymentMethodAccount.AccountType);

            //get campus name if selected any
            var campusName = model.Payment.CampusId.IsNotNullOrEmpty() ? work.Campus.Get(model.Payment.CampusId).Name : string.Empty;

            if (model.Payment?.Frequency == PaymentOccurrence.OneTime)
            {
                var paymentResult = await work.Giving.CreatePayment(model, paymentMethodAccount.AccountType, processingFees, campusName);

                return Json(paymentResult);
            }

            // ScheduledPayment table used for recurring payment
            else if (model.Payment?.Frequency == PaymentOccurrence.Recurring)
            {
                var paymentResult = work.Giving.CreateRecurringPayment(model, processingFees, campusName);

                return Json(paymentResult);
            }

            ViewBag.Guest = false;

            return Json(new CompleteViewModel() { Success = false, Message = message });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PhoneVerification(SignUpViewModel model)
        {
            if (SessionVariables.CurrentChurch.IsNull() && !string.IsNullOrEmpty(model.Church.Id))
            {
                SessionVariables.CurrentChurch = work.Church.Get(model.Church.Id);
            }

            var user = work.User.GetByPhone(model.Phone);

            if (user != null)
            {
                try
                {
                    //Check if user belongs to this church, If not then Add user to churchusers table
                    var userChurches = work.Church.GetAllChurchUsersByUserId(user.Id);

                    if (!userChurches.Select(x => x.ChurchId).Contains(model.Church.Id))
                    {
                        work.Church.CreateUser(new ChurchUser
                        {
                            Id = Utilities.GenerateUniqueId(),
                            UserId = user.Id,
                            ChurchId = model.Church.Id,
                            CreatedBy = user.Id,
                            CreatedDate = DateTime.Now
                        });
                    }

                    var person = work.Person.GetByEmailAndPhoneAndName(user.Email, user.PhoneNumber, user.FirstName, user.LastName);

                    if (person.IsNullOrEmpty())
                    {
                        person = new Person()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            FirstName = user.FirstName.IsNotNullOrEmpty() ? user.FirstName : Constants.User,
                            LastName = user.LastName.IsNotNullOrEmpty() ? user.LastName : "Name",
                            Email = user.Email,
                            CreatedDate = DateTime.Now,
                            PhoneNumber = model.Phone,
                            IsActive = true,
                            Address1 = user.Address1,
                            Address2 = user.Address2,
                            City = user.City,
                            State = user.State,
                            Zip = user.Zip
                        };
                        var result = work.Person.Create(person);

                        if (result.ResultType == ResultType.Success)
                        {
                            work.Person.CreateChurchPerson(new ChurchPerson
                            {
                                Id = Utilities.GenerateUniqueId(),
                                PersonId = person.Id,
                                ChurchId = model.Church.Id,
                                CreatedBy = user.Id,
                                CreatedDate = DateTime.Now
                            });
                            user.PersonId = result.Data.Id;
                        }
                    }
                    else
                    {
                        //Check if person belongs to this church, If not, add person to churchPeople table
                        var personChurches = work.Person.GetAllByPersonId(person.Id);

                        if (!personChurches.Select(x => x.ChurchId).Contains(model.Church.Id))
                        {
                            work.Person.CreateChurchPerson(new ChurchPerson
                            {
                                Id = Utilities.GenerateUniqueId(),
                                PersonId = person.Id,
                                ChurchId = model.Church.Id,
                                CreatedBy = user.Id,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                    work.User.Update(user);
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} " + ex });
                }
            }
            else
            {
                var person = work.Person.GetByEmailAndPhone(model.Email, model.Phone);

                if (person.IsNullOrEmpty())
                {
                    person = new Person()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        FirstName = "User",
                        LastName = "Name",
                        CreatedDate = DateTime.Now,
                        PhoneNumber = model.Phone,
                        IsActive = true
                    };
                    work.Person.Create(person);
                    work.Person.CreateChurchPerson(new ChurchPerson
                    {
                        Id = Utilities.GenerateUniqueId(),
                        PersonId = person.Id,
                        ChurchId = model.Church.Id,
                        CreatedBy = SessionVariables.CurrentUser?.User.IsNotNullOrEmpty() == true ? SessionVariables.CurrentUser.User.Id : Constants.System,
                        CreatedDate = DateTime.Now
                    });
                }

                user = new ApplicationUser
                {
                    Id = Utilities.GenerateUniqueId(),
                    //Email = model.Email = person.Email.IsNotNullOrEmpty() ? person.Email : "guest" + Utilities.GenerateVerificationCode() + "@email.com", Why were we setting this to guest before ? Changed on 3 / 8 / 2022
                    Email = model.Email = person.Email.IsNotNullOrEmpty() ? person.Email : null,
                    ExternalProvider = null,
                    ExternalProviderId = null,
                    EmailConfirmed = false,
                    SecurityStamp = Utilities.GenerateUniqueId(),
                    PhoneNumber = model.Phone.IsNullOrEmpty() ? null : model.Phone,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    UserName = person.Email.IsNotNullOrEmpty() ? person.Email : model.Phone,
                    PhoneVerificationCode = Utilities.GenerateVerificationCode(),
                    IsActive = true,
                    Address1 = person.Address1.IsNotNullOrEmpty() ? person.Address1 : string.Empty,
                    Address2 = person.Address2.IsNotNullOrEmpty() ? person.Address2 : string.Empty,
                    City = person.City.IsNotNullOrEmpty() ? person.City : string.Empty,
                    State = person.State.IsNotNullOrEmpty() ? person.State : string.Empty,
                    Zip = person.Zip.IsNotNullOrEmpty() ? person.Zip : string.Empty,
                    CreatedDate = DateTime.Now,
                    CreatedBy = Constants.System,
                    AssignedToChurch = true,
                    ShowWelcomeMessage = false,
                    PersonId = person.Id
                };

                work.User.Create(user);
                adoData.InsertUserRoleByName(user.Id, Roles.Donor);

                //Add user to churchusers table
                work.Church.CreateUser(new ChurchUser
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = user.Id,
                    ChurchId = model.Church.Id,
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now
                });

                work.UserSetting.Create(new UserSetting
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = user.Id,
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now,
                    PrimaryChurchId = model.Church.Id
                    //,PrimaryChurchCampusId = model.Campus.Id
                });
            }
            try
            {
                var smsMessage = new SmsMessage
                {
                    Id = Utilities.GenerateUniqueId(),
                    To = model.Phone,
                    Message = "Your " + (SessionVariables.CurrentChurch != null && !string.IsNullOrEmpty(SessionVariables.CurrentChurch.Display) ? SessionVariables.CurrentChurch.Display : "Praise") +
                            " verification code is: " + user.PhoneVerificationCode +
                                  "\n\nRequested: " + DateTime.Now,
                    CreatedDate = DateTime.Now,
                    CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                    Type = SmsMessageType.SignIn
                };
                Utilities.SendMessage(smsMessage);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} " + ex.Message });
            }

            return Json(new { Success = true, Message = "A verification code has been sent to your phone.", Key = user.PhoneVerificationCode });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult VerifyCode(string Phone, string Code)
        {
            var user = work.User.GetByPhone(Phone);
            var paymentMethods = new List<SelectListItems>();
            var donorId = string.Empty;

            if (user.IsNotNull())
            {
                if (user.PhoneVerificationCode == Code)
                {
                    var newUser = user.FirstName.IsNullOrEmpty() || user.LastName.IsNullOrEmpty();

                    user.PhoneNumberConfirmed = true;
                    work.User.Update(user);

                    //Log in current user by default
                    var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
                    signInManager.SignIn(user, isPersistent: true, rememberBrowser: false);

                    if (user.IsActive)
                    {
                        Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);
                    }

                    var userMerchantAccount = work.UserMerchantAccount.GetByUserId(user.Id);

                    if (userMerchantAccount?.DonorGUID.IsNotNullOrEmpty() == true)
                    {
                        donorId = userMerchantAccount.DonorGUID;
                        paymentMethods = work.Payment.GetPaymentMethodsDropdownList(userMerchantAccount.DonorGUID);
                    }

                    return Json(new { Success = true, DonorId = donorId, NewUser = newUser, PaymentMethods = paymentMethods, Message = "This phone number has been verified." });
                }

                return Json(new { Success = false, Message = "Invalid Code. It looks like the code you used is incorrect or expired. Please click the Resend Code link to try again." });
            }

            return Json(new { Success = false, Message = "Something went wrong. Please try again." });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> AddNewPaymentMethods(PaymentMethodViewModel model, string churchId)
        {
            if (SessionVariables.CurrentChurch.IsNull() && !string.IsNullOrEmpty(churchId))
            {
                SessionVariables.CurrentChurch = work.Church.Get(churchId);
            }

            if (SessionVariables.CurrentMerchant.IsNullOrEmpty())
            {
                SessionVariables.CurrentMerchant = work.ChurchMerchantAccount.GetByChurchId(SessionVariables.CurrentChurch.Id);
            }

            if (string.IsNullOrEmpty(model.ChurchId))
            {
                model.ChurchId = SessionVariables.CurrentChurch.Id;
            }

            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

            var user = work.User.GetByPhone(model.User.PhoneNumber);

            if (user.IsNotNullOrEmpty() && string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
            {
                var person = work.Person.GetByUserId(user.Id);

                if (person.IsNotNullOrEmpty())
                {
                    person.Email = model.User.Email;
                    person.FirstName = model.User.FirstName;
                    person.LastName = model.User.LastName;
                    work.Person.Update(person);
                }

                // Update user properties
                user.FirstName = model.User.FirstName;
                user.LastName = model.User.LastName;
                user.Zip = model.User.Zip;
                user.Address1 = model.User.Address1;
                user.Address2 = model.User.Address2;
                user.City = model.User.City;
                user.State = model.User.State;
                user.Email = model.User.Email;
                user.UserName = model.User.Email;

                // Update session variable and save changes
                SessionVariables.CurrentUser.User = user;
                work.User.Update(user);
            }

            if (model.DonorGUID.IsNullOrEmpty())
            {
                var result = await work.Giving.CreateDonorAccount(user, SessionVariables.CurrentMerchant);

                if (result.ResultType == ResultType.Failure || result.Data.IsNullOrEmpty() || result.Data.DonorGUID.IsNullOrEmpty())
                {
                    return Json(new { Success = false, DonorId = model.DonorGUID, result.Message });
                }

                model.DonorGUID = result.Data.DonorGUID;
                var domain = SessionVariables.CurrentDomain;
                var baseUrl = domain.IsNotNullOrEmpty() && domain.BaseUrl.IsNotNullOrEmpty() ? URLPrefixes.Http + domain.BaseUrl : ApplicationCache.Instance.SiteConfiguration.Url;
                var content = EmailTemplates.NewUserAccount_body.Replace("{createdBy}", "Praise")
                .Replace("{username}", user?.UserName)
                //.Replace("{password}", password)
                .Replace("{btn_url}", $"{baseUrl}/users/setpassword?userid={user.Id}&token={UserManager.GenerateUserToken("SET_PASSWORD", user.Id)}");
                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = content,
                    To = user.Email,
                    Subject = "Account Setup",
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                };

                Emailer.SendEmail(email);
            }

            //Add new card
            if (model.PaymentMethod == DigitalPaymentMethods.Card)
            {
                var expiry = model.PaymentCard.CcExpiry.Split('/');
                model.PaymentCard.CcExpMonth = expiry[0];
                model.PaymentCard.CcExpYear = expiry[1];

                if (await work.Giving.CardExistsForDonor(model, apiCredentials))
                {
                    return Json(new { Success = false, DonorId = model.DonorGUID, Message = PaymentMethodAlertMessages.CardExists });
                }

                //add card details to payment provider
                var mappedCard = work.Giving.MapToCardRequestModel(model);

                var cardData = await nuveiHelper.CreateCardAsync(mappedCard, apiCredentials);

                var createCardResponse = Responses.GetApiTransactionResponse(cardData?.result);

                if (createCardResponse != APIStatuses.Success)
                {
                    string apiErrorMessage = Responses.HandleApiTransactionFailure(cardData);
                    var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", apiErrorMessage, "DonorGUID", model.DonorGUID);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Card", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : string.Empty, LogStatuses.Error, logObj);

                    return Json(new { Success = false, DonorId = model.DonorGUID, Message = apiErrorMessage });
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
                        TypeId = user?.Id,
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
                        ExpYear = model.PaymentCard.CcExpYear.GetLastCharacters(2),
                        CreatedDate = DateTime.Now,
                        CreatedBy = user?.Id
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

                    work.Giving.MakePrimary(paymentMethodAccount.AccountGUID, model.DonorGUID, true);
                }
            }
            //add new bank account
            else if (model.PaymentMethod == DigitalPaymentMethods.ACH)
            {
                var verifyRouting = await nuveiHelper.VerifyBankRoutingAsync(model.PaymentAccount.RoutingNumber, apiCredentials);

                var verifyRoutingResponse = Responses.GetApiTransactionResponse(verifyRouting?.result);

                //Problem with routing number
                if (verifyRoutingResponse != APIStatuses.Success)
                {
                    return Json(new { Success = false, DonorId = model.DonorGUID, Message = verifyRouting.result_message });
                }

                var bankName = verifyRouting.aba.bank_name;

                if (await work.Giving.CheckAccountExists(model, apiCredentials))
                {
                    return Json(new { Success = false, DonorId = model.DonorGUID, Message = PaymentMethodAlertMessages.BankAccountExists });
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
                        var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", apiErrorMessage, "DonorGUID", model.DonorGUID);
                        logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Bank Account", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : string.Empty, LogStatuses.Error, logObj);

                        return Json(new { Success = false, DonorId = model.DonorGUID, Message = apiErrorMessage });
                    }

                    if (!string.IsNullOrEmpty(bankData.check_key))
                    {
                        var accountGUID = bankData.check_key;
                        var donorGUID = bankData.customer_key;
                        var method = model.PaymentAccount.NickName.IsNotNullOrEmpty() ? model.PaymentAccount.NickName : "BANK";
                        var paymentMethodAccount = new PaymentMethodAccount
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
                            IsActive = true,
                            IsPrimary = false,
                            CreatedDate = DateTime.Now,
                            CreatedBy = SessionVariables.CurrentUser.User.Id
                        };

                        work.PaymentMethodAccount.Create(paymentMethodAccount);

                        var content = EmailTemplates.PaymentMethod_body.Replace("{message}", PaymentMethodAlertMessages.BankAccountAddSuccess)
                         .Replace("{user_display}", SessionVariables.CurrentUser.User.Display)
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

                        work.Giving.MakePrimary(paymentMethodAccount.AccountGUID, model.DonorGUID, true);
                    }
                }
            }

            // Get all payment methods
            var paymentMethods = work.Payment.GetPaymentMethodsDropdownList(model.DonorGUID);

            return Json(new { Success = true, DonorId = model.DonorGUID, PaymentMethods = paymentMethods, Message = "Your payment method has been added." });
        }

        public ActionResult Completed(string churchId, string fundId, bool guest, string campusName, string amount, decimal? processingFee = null, string paymentOccurrence = null)
        {
            if (string.IsNullOrEmpty(churchId)) return View();

            var church = work.Church.Get(churchId);
            var fund = work.Fund.Get(fundId);
            processingFee = processingFee > 0 ? processingFee : null;
            var model = new CompleteViewModel()
            {
                Church = church,
                CampusName = campusName,
                Guest = guest,
                PaymentAmount = amount,
                ProcessingFee = processingFee,
                PaymentOccurrence = paymentOccurrence,
                Success = true,
                Message = fund.GivingThankYouText.IsNotNullOrEmpty() ? fund.GivingThankYouText : church.GivingThankYouText
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult GivingOptions(bool guest)
        {
            ViewBag.Guest = guest;
            return PartialView("_GivingOptions");
        }

        [HttpGet]
        public ActionResult GiveAsGuest(string id)
        {
            //Prevent guest giving if no merchant account has been set up for the church
            var model = new GuestPaymentModel();

            if (work.Church.Get(id).IsNullOrEmpty())
            {
                ViewBag.Message = "No account exists for this church.";
                return View(model);
            }

            var result = work.Giving.GuestGiving(id);

            if (!result.IsNullOrEmpty()) return View(result);

            ViewBag.Message = "No merchant account has been set up for the church.";
            result = model;

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult GiveAsGuest(GuestPaymentModel model)
        {
            if (model.CreateAccount && (!string.IsNullOrEmpty(model.Email) || !string.IsNullOrEmpty(model.Phone)))
            {
                //Check if the guest exists
                var user = work.User.GetByEmailAndPhone(model.Email, model.Phone);
                if (user.IsNullOrEmpty())
                {
                    var person = work.Person.GetByEmailAndPhone(model.Email, model.Phone);

                    if (person.IsNullOrEmpty())
                    {
                        person = new Person()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            CreatedDate = DateTime.Now,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.Phone,
                            Email = model.Email,
                            IsActive = true
                        };
                        work.Person.Create(person);
                        work.Person.CreateChurchPerson(new ChurchPerson
                        {
                            Id = Utilities.GenerateUniqueId(),
                            PersonId = person.Id,
                            ChurchId = model.Church.Id,
                            CreatedBy = Constants.System,
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        //Check if the person belongs to this church. If not, add the person to ChurchPeople table
                        var peopleChurches = work.Person.GetAllByPersonId(person.Id);

                        if (!peopleChurches.Select(x => x.ChurchId).Contains(model.Church.Id))
                        {
                            work.Person.CreateChurchPerson(new ChurchPerson
                            {
                                Id = Utilities.GenerateUniqueId(),
                                PersonId = person.Id,
                                ChurchId = model.Church.Id,
                                CreatedBy = Constants.System,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }

                    user = new ApplicationUser
                    {
                        Id = Utilities.GenerateUniqueId(),
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                        UserName = !string.IsNullOrEmpty(model.Email) ? model.Email : model.Phone,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Constants.System,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = !string.IsNullOrEmpty(model.Email) ? model.Email : null,
                        SecurityStamp = Utilities.GenerateUniqueId(),
                        PhoneNumber = !string.IsNullOrEmpty(model.Phone) ? model.Phone : null,
                        ExternalProvider = null,
                        ExternalProviderId = null,
                        AssignedToChurch = true,
                        PersonId = person.Id
                    };

                    //Create new user, roles, and settings
                    work.User.Create(user);
                    adoData.InsertUserRoleByName(user.Id, Roles.Donor);
                    work.UserSetting.Create(new UserSetting
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = user.Id,
                        CreatedBy = user.Id,
                        CreatedDate = DateTime.Now,
                        PrimaryChurchId = model.Church.Id,
                        PrimaryChurchCampusId = model.CampusId
                    });

                    //Add user to churchusers table
                    work.Church.CreateUser(new ChurchUser
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = user.Id,
                        ChurchId = model.Church.Id,
                        CreatedBy = user.Id,
                        CreatedDate = DateTime.Now
                    });
                }
                else
                {
                    try
                    {
                        //Check if user belongs to this church. If not, add user to the ChurchUsers table
                        var userChurches = work.Church.GetAllChurchUsersByUserId(user.Id);

                        if (!userChurches.Select(x => x.ChurchId).Contains(model.Church.Id))
                        {
                            work.Church.CreateUser(new ChurchUser
                            {
                                Id = Utilities.GenerateUniqueId(),
                                UserId = user.Id,
                                ChurchId = model.Church.Id,
                                CreatedBy = user.Id,
                                CreatedDate = DateTime.Now
                            });
                        }

                        var person = work.Person.GetByEmailAndPhoneAndName(user.Email, user.PhoneNumber, user.FirstName, user.LastName);

                        if (person.IsNullOrEmpty())
                        {
                            person = new Person()
                            {
                                Id = Utilities.GenerateUniqueId(),
                                FirstName = user.FirstName.IsNotNullOrEmpty() ? user.FirstName : Constants.User,
                                LastName = user.LastName.IsNotNullOrEmpty() ? user.LastName : "Name",
                                Email = user.Email,
                                CreatedDate = DateTime.Now,
                                PhoneNumber = model.Phone,
                                IsActive = true,
                                Address1 = user.Address1,
                                Address2 = user.Address2,
                                City = user.City,
                                State = user.State,
                                Zip = user.Zip
                            };
                            var result = work.Person.Create(person);

                            if (result.ResultType == ResultType.Success)
                            {
                                work.Person.CreateChurchPerson(new ChurchPerson
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    PersonId = person.Id,
                                    ChurchId = model.Church.Id,
                                    CreatedBy = user.Id,
                                    CreatedDate = DateTime.Now
                                });
                                user.PersonId = result.Data.Id;
                            }
                        }
                        else
                        {
                            //Check if person belongs to this church, If not then add person to churchPeople table
                            var personChurches = work.Person.GetAllByPersonId(person.Id);

                            if (!personChurches.Select(x => x.ChurchId).Contains(model.Church.Id))
                            {
                                work.Person.CreateChurchPerson(new ChurchPerson
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    PersonId = person.Id,
                                    ChurchId = model.Church.Id,
                                    CreatedBy = user.Id,
                                    CreatedDate = DateTime.Now
                                });
                            }
                        }

                        user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogger.LogException(ex);
                    }
                }
            }

            SessionVariables.CurrentChurch ??= work.Church.Get(model.Church.Id);

            var processingFees = work.Giving.CalculateProcessingFee(model.Church.Id, model.Amount, DigitalPaymentMethods.Card);
            decimal.TryParse(model.Amount, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out var amountDecimal);
            var message = SessionVariables.CurrentChurch.GivingThankYouText;
            var payment = new Payment
            {
                Id = Utilities.GenerateUniqueId(),
                Amount = amountDecimal
            };

            if (!string.IsNullOrEmpty(model.FundId))
            {
                var fund = work.Fund.Get(model.FundId);

                if (fund.IsNullOrEmpty() || fund.Closed)
                {
                    fund = work.Fund.GetByName(model.Church.Id, GivingFunds.General);
                    model.FundId = fund.Id;
                }
            }

            if (!string.IsNullOrEmpty(model.FundId))
            {
                var fund = work.Fund.Get(model.FundId);

                if (fund.GivingThankYouText.IsNotNullOrEmpty())
                {
                    message = fund.GivingThankYouText;
                }
            }

            //TODO Create Donor and payment method account

            //Donor was successfully created, now create the payment transaction
            //if (returnData.Donor != null)
            //{
            //    var givingModel = new GivingViewModel()
            //    {
            //        Payment = payment,
            //        ChurchId = model.Church.Id,
            //        DonorGUID = returnData.Donor.DonorGUID,
            //        AccountGUID = returnData.Donor.AccountGUID,
            //        AccountType = DigitalPaymentMethods.Card,
            //        IncludeProcessingFee = model.IncludeProcessingFee
            //    };

            //    //process the payment transaction
            //    var paymentStatus = stewardshipHelper.CreateTransaction(givingModel);

            //    if (paymentStatus.Status != null && paymentStatus.Status.ErrorCode != "0")
            //    {
            //        work.Giving.SendPaymentStatusEmail(givingModel, paymentStatus, model.Email);

            //        message = paymentStatus.Status.Description;
            //        var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", returnData.Status.Description,
            //                "Exception Code", returnData.Status.ErrorCode, "Fund Id", model.FundId, "Amount", givingModel.Payment.Amount.ToString(), "AccountGUID", givingModel.AccountGUID, "DonorGUID", givingModel.DonorGUID);
            //        logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Create Transaction", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : "", LogStatuses.Error, logObj);
            //    }

            //    //Payment successfully processed, now add to the payments table
            //    if (paymentStatus.Transaction != null)
            //    {
            //        //get the MerchantId
            //        var churchMerchantAccount = work.ChurchMerchantAccount.GetByChurchId(model.Church.Id);

            //        if (churchMerchantAccount.IsNotNull())
            //        {
            //            payment.Merchant = MerchantProviders.Nuvei;
            //            payment.MerchantId = churchMerchantAccount.Id;
            //        }

            //        payment.ProcessingFee = processingFees;
            //        payment.DonorPaidMerchantFee = model.IncludeProcessingFee;
            //        payment.Amount = amountDecimal;
            //        payment.ChurchId = model.Church.Id;
            //        payment.UserId = stewardshipDonorVm.Donor.DonorID;
            //        payment.FundId = model.FundId;
            //        payment.Frequency = PaymentOccurrence.OneTime;
            //        payment.PaymentMethod = "GUESTNotSaved";
            //        payment.TransactionType = TransactionType.Payment;
            //        payment.CreatedDate = DateTime.Now;
            //        payment.CreatedBy = stewardshipDonorVm.Donor.DonorID;
            //        payment.CampusId = model.CampusId;
            //        payment.DigitalPaymentMethod = DigitalPaymentMethods.Card;
            //        payment.DigitalPaymentType = DigitalPaymentTypes.Online;
            //        payment.AccountScheduleGUID = !string.IsNullOrEmpty(paymentStatus.Transaction.AccountScheduleGUID) ? paymentStatus.Transaction.AccountScheduleGUID : "";

            //        var verifyTransaction = stewardshipHelper.VerifyTransaction(returnData.Donor.DonorGUID, paymentStatus.Transaction.AccountScheduleGUID, SessionVariables.CurrentChurch.Id);

            //        if (verifyTransaction.Status != null && returnData.Status.ErrorCode != "0")
            //        {
            //            message = verifyTransaction.Status.Description;
            //            payment.PaymentStatus = PaymentStatus.Error;
            //            var result = work.Payment.Create(payment);
            //            givingModel.Payment = result.Data;
            //            work.Giving.SendPaymentStatusEmail(givingModel, verifyTransaction, model.Email);
            //        }
            //        else
            //        {
            //            payment.PaymentStatus = PaymentStatus.Success;
            //            var result = work.Payment.Create(payment);
            //            givingModel.Payment = result.Data;
            //            work.Giving.SendPaymentStatusEmail(givingModel, returnData, model.Email);

            //            if (result.ResultType == ResultType.Success)
            //            {
            //                return RedirectToAction(nameof(Completed), new
            //                {
            //                    churchId = model.Church.Id,
            //                    fundId = model.FundId,
            //                    guest = true,
            //                    campusName = model.CampusId.IsNotNullOrEmpty() ? work.Campus.Get(model.CampusId).Name : "",
            //                    amount = (payment.Amount + processingFees).ToString(),
            //                    processingFee = processingFees
            //                });
            //            }

            //            message = returnData.Status.Description;
            //        }
            //    }
            //}
            //else
            //{
            //    message = returnData.Status.Description;
            //}

            ViewBag.Guest = true;

            return View("Completed", new CompleteViewModel() { Church = SessionVariables.CurrentChurch, Success = false, Message = "<h5>Uh-oh! There was a problem processing your gift. Please try again.</h5><p><b>Error Description:</b> " + message + "</p>" });
        }
    }
}