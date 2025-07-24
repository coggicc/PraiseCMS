using Microsoft.AspNet.Identity;
using PraiseCMS.API.Models;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PraiseCMS.Shared.Methods.ExtensionMethods;
using Constants = PraiseCMS.Shared.Shared.Constants;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
using static DotNetOpenAuth.OpenId.Extensions.AttributeExchange.WellKnownAttributes.Contact;
using System.ComponentModel;
using PraiseCMS.API.Helpers;

namespace PraiseCMS.Web.Controllers
{
    public class TextGivingController : BaseController
    {
        [HttpPost]
        public async Task<ActionResult> ReceiveSms()
        {
            // 1️⃣ Extract donor's phone number and the church's Twilio number
            string donorPhoneNumber = Request["From"];
            string churchTwilioNumber = Request["To"];
            string messageBody = Request["Body"]?.Trim();

            // 2️⃣ Validate message format (Must contain at least an amount)
            if (string.IsNullOrEmpty(messageBody))
            {
                return TwilioResponse("Invalid format. To donate, text: [Amount] [Optional Fund Name] (e.g., '50' or '50 Missions').");
            }

            // 3️⃣ Split message into amount and optional fund name
            string[] messageParts = messageBody.Split(new[] { ' ' }, 2);

            if (!decimal.TryParse(messageParts[0], out decimal donationAmount) || donationAmount <= 0)
            {
                return TwilioResponse("Invalid amount. To donate, text: [Amount] [Optional Fund Name] (e.g., '50' or '50 Missions').");
            }

            string fundName = messageParts.Length > 1 ? messageParts[1].Trim() : null;

            // 4️⃣ Lookup the church by Twilio number
            var church = await work.Church.GetChurchByTwilioNumberAsync(churchTwilioNumber);

            if (church == null)
            {
                return TwilioResponse("This number is not linked to a church. Please check with your church.");
            }

            string fundId = "";

            // 5️⃣ If a fund name was provided, try to find the fund
            if (!string.IsNullOrEmpty(fundName))
            {
                var fund = await work.Fund.GetByNameAsync(church.Id, fundName);

                if (fund == null)
                {
                    return TwilioResponse($"The fund '{fundName}' was not found for {church.Display}. Please check the fund name or try again without specifying one.");
                }

                fundId = fund.Id;
            }

            // 6️⃣ Generate the giving link
            string givingLink = GenerateGivingFormLink(church.Id, fundId, donationAmount);

            // 7️⃣ Send the link via SMS
            string responseMessage = $"Tap the link to complete your ${donationAmount} donation to {church.Display}";

            if (!string.IsNullOrEmpty(fundId))
            {
                responseMessage += $" ({fundName} Fund)";
            }

            responseMessage += $": {givingLink}";

            Utilities.SendMessage(new SmsMessage { To = donorPhoneNumber, Message = responseMessage });

            return TwilioResponse(responseMessage);
        }

        /// <summary>
        /// Generates a pre-filled giving form link.
        /// </summary>
        private string GenerateGivingFormLink(string churchId, string fundId, decimal amount)
        {
            return Url.Action("Give", "Donation",
                new { churchId, fundId = !string.IsNullOrEmpty(fundId) ? fundId : null, amount },
                Request.Url.Scheme);
        }

        /// <summary>
        /// Generates a Twilio-compatible XML response.
        /// </summary>
        private ActionResult TwilioResponse(string message)
        {
            var messagingResponse = new Twilio.TwiML.MessagingResponse();
            messagingResponse.Message(message);
            return Content(messagingResponse.ToString(), "text/xml");
        }

        [HttpPost]
        public async Task Message()
        {
            var phone = Request.Form["From"];
            var body = Request.Form["Body"];

            if (body.IsNotNullOrEmpty() || phone.IsNotNullOrEmpty())
            {
                phone = phone.Replace("+1", string.Empty);
                const string phoneFormat = "(###) ###-####";
                // First, remove everything except of numbers
                var regexObj = new Regex(@"[^\d]");
                phone = regexObj.Replace(phone, string.Empty);

                // Second, format numbers to phone string
                if (phone.Length > 0)
                {
                    phone = Convert.ToInt64(phone).ToString(phoneFormat);
                }

                var number = new string(phone.Where(char.IsDigit).ToArray());
                var user = work.User.GetByPhone(phone);

                if (user.IsNotNullOrEmpty())
                {
                    var userSetting = work.UserSetting.GetByUserId(user.Id);
                    var church = work.Church.Get(userSetting.PrimaryChurchId);

                    if (body.Contains("$"))
                    {
                        var hasAmount = decimal.TryParse(body, NumberStyles.Currency,
                          CultureInfo.CurrentCulture.NumberFormat, out var amount);

                        if (hasAmount)
                        {
                            Session["Amount"] = amount;
                            var smsMessage = new SmsMessage
                            {
                                Id = Utilities.GenerateUniqueId(),
                                To = user.PhoneNumber,
                                Message = $"Please confirm your {amount:C} gift to {church.Display}. Reply Y to confirm or N to cancel.",
                                CreatedDate = DateTime.Now,
                                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                                Type = SmsMessageType.Register
                            };
                            Utilities.SendMessage(smsMessage);
                        }
                    }
                    else if (body.ToUpper() == TextResponses.YesAbbrv || body.ToUpper() == TextResponses.YES || body.ToUpper() == TextResponses.NoAbbrv || body.ToUpper() == TextResponses.NO)
                    {
                        if ((body.ToUpper() == TextResponses.YesAbbrv || body.ToUpper() == TextResponses.YES) && Session["Amount"].IsNotNullOrEmpty())
                        {
                            var amount = Session["Amount"].ToString();
                            Session.Clear();

                            if (church.IsNotNullOrEmpty())
                            {
                                SessionVariables.CurrentChurch = church;
                                SessionVariables.CurrentMerchant = work.ChurchMerchantAccount.GetByChurchId(church.Id);

                                if (SessionVariables.CurrentMerchant.IsNotNull())
                                {
                                    var userMerchAccount = work.UserMerchantAccount.GetByUserId(user.Id, MerchantProviders.Nuvei);
                                    var paymentMethods = work.Payment.GetPaymentMethodsDropdownList(userMerchAccount.DonorGUID);
                                    paymentMethods = paymentMethods.Where(q => !q.Disabled).ToList();
                                    string message;

                                    var fund = new Fund();

                                    if (paymentMethods.Any())
                                    {
                                        var payment = new Payment
                                        {
                                            Id = Utilities.GenerateUniqueId(),
                                            Amount = Convert.ToDecimal(amount.Replace("$", string.Empty))
                                        };

                                        //Change to use designation selected in form
                                        if (!string.IsNullOrEmpty(payment.FundId))
                                        {
                                            fund = work.Fund.Get(payment.FundId);

                                            if (fund.IsNullOrEmpty() || fund.Closed)
                                            {
                                                fund = work.Fund.GetByName(church.Id, GivingFunds.General);
                                                payment.FundId = fund.Id;
                                            }
                                        }

                                        var accountGUID = paymentMethods.Any(q => q.IsPrimary) ? paymentMethods.Find(q => q.IsPrimary).Value : paymentMethods[0].Value;
                                        var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(accountGUID);
                                        payment.ProcessingFee = work.Giving.CalculateProcessingFee(church.Id, payment.Amount.ToString(), paymentMethodAccount.AccountType);
                                        payment.TransactionType = TransactionType.Payment;
                                        payment.Frequency = PaymentOccurrence.OneTime;
                                        payment.UserId = user.Id;
                                        payment.CreatedBy = user.Id;
                                        payment.DigitalPaymentMethod = paymentMethodAccount.AccountType;

                                        payment.DigitalPaymentType = DigitalPaymentTypes.Online;

                                        var givingModel = new GivingViewModel()
                                        {
                                            Payment = payment,
                                            ChurchId = userSetting.PrimaryChurchId,
                                            DonorGUID = userMerchAccount.DonorGUID,
                                            AccountGUID = accountGUID,
                                            IncludeProcessingFee = false
                                        };

                                        if (!string.IsNullOrEmpty(givingModel.DonorGUID) && !string.IsNullOrEmpty(givingModel.AccountGUID))
                                        {
                                            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

                                            var transactionResponse = new TransactionResponse();

                                            if (paymentMethodAccount.AccountType == DigitalPaymentMethods.Card)
                                            {
                                                var cardTransactionRequest = new CardTransactionRequest
                                                {
                                                    tokenized_card = new TokenizedCard
                                                    {
                                                        card_info_key = givingModel.AccountGUID,
                                                        amount = givingModel.Payment.Amount.ToString(),
                                                        transaction_type = TransactionTypeShortCode.Auth
                                                    }
                                                };

                                                transactionResponse = await nuveiHelper.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials);

                                                givingModel.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                                                givingModel.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                                                var result = work.Payment.Create(givingModel.Payment);
                                                givingModel.Payment = result.Data;

                                                work.Giving.SendPaymentStatusEmail(givingModel, transactionResponse);
                                            }
                                            else
                                            {
                                                var checkTransactionRequest = new CheckTransactionRequest
                                                {
                                                    tokenized_check = new TokenizedCheck
                                                    {
                                                        check_info_key = givingModel.AccountGUID,
                                                        amount = givingModel.Payment.Amount.ToString(),
                                                        transaction_type = TransactionTypeShortCode.Auth
                                                    }
                                                };

                                                transactionResponse = await nuveiHelper.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials);

                                                givingModel.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                                                givingModel.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                                                var result = work.Payment.Create(givingModel.Payment);
                                                givingModel.Payment = result.Data;

                                                work.Giving.SendPaymentStatusEmail(givingModel, transactionResponse);
                                            }

                                            payment.TransactionId = !string.IsNullOrEmpty(transactionResponse?.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                                            payment.PaymentMethod = givingModel.Payment.PaymentStatus;
                                            work.Payment.Update(payment);
                                            if (givingModel.Payment.PaymentStatus == APIStatuses.Success)
                                            {
                                                message = $"We appreciate your generosity. Your gift of {payment.Amount:C} to {church.Display} was successful.";
                                            }
                                            else
                                            {
                                                message = $"Uh-oh! There was a problem processing your gift. Error: {transactionResponse?.result_description}";
                                            }

                                            var smsMessage = new SmsMessage
                                            {
                                                Id = Utilities.GenerateUniqueId(),
                                                To = user.PhoneNumber,
                                                Message = message,
                                                CreatedDate = DateTime.Now,
                                                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                                                Type = SmsMessageType.Register
                                            };
                                            Utilities.SendMessage(smsMessage);
                                        }
                                    }
                                    else
                                    {
                                        var smsMessage = new SmsMessage
                                        {
                                            Id = Utilities.GenerateUniqueId(),
                                            To = user.PhoneNumber,
                                            Message = "Uh-oh! It looks like your card has expired. Please update your card here: " + ApplicationCache.Instance.SiteConfiguration.Url + "TextGiving/Registration/" + user.Id,
                                            CreatedDate = DateTime.Now,
                                            CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                                            Type = SmsMessageType.Register
                                        };
                                        Utilities.SendMessage(smsMessage);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Session.Clear();
                            var smsMessage = new SmsMessage
                            {
                                Id = Utilities.GenerateUniqueId(),
                                To = user.PhoneNumber,
                                Message = "Your gift has been canceled.",
                                CreatedDate = DateTime.Now,
                                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                                Type = SmsMessageType.Register
                            };
                            Utilities.SendMessage(smsMessage);
                        }
                    }
                    else if (church.Display == body)
                    {
                        var smsMessage = new SmsMessage
                        {
                            Id = Utilities.GenerateUniqueId(),
                            To = user.PhoneNumber,
                            Message = "You already have an account.",
                            CreatedDate = DateTime.Now,
                            CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                            Type = SmsMessageType.Register
                        };
                        Utilities.SendMessage(smsMessage);
                    }
                }
                else
                {
                    var church = work.Church.GetPraiseChurch();

                    if (church.IsNotNullOrEmpty())
                    {
                        user = new ApplicationUser
                        {
                            Id = Utilities.GenerateUniqueId(),
                            EmailConfirmed = false,
                            PhoneNumberConfirmed = true,
                            TwoFactorEnabled = false,
                            LockoutEnabled = false,
                            AccessFailedCount = 0,
                            UserName = phone,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            Email = null,
                            PasswordHash = UserManager.PasswordHasher.HashPassword(Utilities.GeneratePasswordByNumber(phone)),
                            SecurityStamp = Utilities.GenerateUniqueId(),
                            PhoneNumber = phone,
                            PhoneVerificationCode = Utilities.GenerateVerificationCode(),
                            ExternalProvider = null,
                            ExternalProviderId = null,
                            AssignedToChurch = false
                        };

                        var result = UserManager.Create(user);

                        if (result.Succeeded)
                        {
                            adoData.InsertUserRoleByName(user.Id, Roles.Donor);
                            work.UserSetting.Create(new UserSetting
                            {
                                Id = Utilities.GenerateUniqueId(),
                                UserId = user.Id,
                                CreatedBy = user.Id,
                                CreatedDate = DateTime.Now,
                                PrimaryChurchId = church.Id
                            });

                            var smsMessage = new SmsMessage
                            {
                                Id = Utilities.GenerateUniqueId(),
                                To = user.PhoneNumber,
                                Message = "Great! To complete your one time registration, visit " + ApplicationCache.Instance.SiteConfiguration.Url + "TextGiving/Registration/" + user.Id,
                                CreatedDate = DateTime.Now,
                                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                                Type = SmsMessageType.Register
                            };
                            Utilities.SendMessage(smsMessage);
                        }
                    }
                }
            }
        }

        public ActionResult Registration(string id)
        {
            var model = new PaymentMethodViewModel
            {
                User = work.User.Get(id)
            };
            model.User.Email = string.Empty;
            model.PaymentMethod = DigitalPaymentMethods.Card;
            var userSetting = work.UserSetting.GetByUserId(id);

            if (userSetting.IsNotNullOrEmpty() && userSetting.PrimaryChurchId.IsNotNullOrEmpty())
            {
                var church = work.Church.Get(userSetting.PrimaryChurchId);

                if (church.IsNotNullOrEmpty() && church.Logo.IsNotNullOrEmpty())
                {
                    ViewBag.ChurchLogo = church.Logo;
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Registration(PaymentMethodViewModel model)
        {
            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

            //model.Status = "Result";
            var user = work.User.Get(model.User.Id);

            if (user.IsNotNullOrEmpty())
            {
                user.FirstName = model.User.FirstName;
                user.LastName = model.User.LastName;
                user.Email = model.User.Email;
                user.Zip = model.User.Zip;
                var result = work.User.Update(user);

                if (result.ResultType == ResultType.Success)
                {
                    if (model.DonorGUID.IsNullOrEmpty())
                    {
                        SessionVariables.CurrentMerchant = work.ChurchMerchantAccount.GetByChurchId(SessionVariables.CurrentChurch.Id);
                        var donorResult = await work.Giving.CreateDonorAccount(user, SessionVariables.CurrentMerchant);

                        if (donorResult.ResultType == ResultType.Failure || donorResult.Data.IsNullOrEmpty() || donorResult.Data.DonorGUID.IsNullOrEmpty())
                        {
                            //model.Success = false;
                            //model.Description = donorResult.Message;

                            return View(model);
                        }
                        model.DonorGUID = donorResult.Data.DonorGUID;
                    }

                    //Add new card
                    if (model.PaymentMethod == DigitalPaymentMethods.Card)
                    {
                        var mappedCard = work.Giving.MapToCardRequestModel(model);

                        var cardData = await nuveiHelper.CreateCardAsync(mappedCard, apiCredentials);

                        var createCardResponse = Responses.GetApiTransactionResponse(cardData?.result);

                        if (createCardResponse != APIStatuses.Success)
                        {
                            //model.Success = false;
                            //model.Description = cardData.result_message;

                            var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", cardData.result_message,
                                "Exception Code", cardData.result_description, "DonorGUID", model.DonorGUID);
                            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Card", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : string.Empty, LogStatuses.Error, logObj);

                            return View(model);
                        }

                        var expiration = model.PaymentCard.CcExpiry.Split('/');
                        model.PaymentCard.CcExpMonth = expiration[0];
                        model.PaymentCard.CcExpYear = expiration[1];
                        model.PaymentCard.CcName = $"{model.User.FirstName} {model.User.LastName}";
                        model.PaymentCard.NickName = model.User.FirstName;

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
                                ExpYear = model.PaymentCard.CcExpYear.GetLastCharacters(2),
                                CreatedDate = DateTime.Now,
                                CreatedBy = SessionVariables.CurrentUser.User.Id
                            };

                            work.PaymentMethodAccount.Create(paymentMethodAccount);
                            work.Giving.MakePrimary(paymentMethodAccount.AccountGUID, model.DonorGUID, true);

                            //model.Success = true;
                            //model.Description = "Text Giving is now available for future gifts.";
                            var smsMessage = new SmsMessage
                            {
                                Id = Utilities.GenerateUniqueId(),
                                To = user.PhoneNumber,
                                //Message = model.Description,
                                CreatedDate = DateTime.Now,
                                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                                Type = SmsMessageType.Register
                            };
                            Utilities.SendMessage(smsMessage);

                            return View(model);
                        }
                    }
                }
            }
            //model.Description = "Uh-oh! No user was found.";

            return View(model);
        }
    }
}