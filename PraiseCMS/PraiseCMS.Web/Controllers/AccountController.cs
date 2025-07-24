using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.Web.Controllers
{
    public class AccountController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string userId = null, string code = null)
        {
            if (userId.IsNotNullOrEmpty() && code.IsNotNullOrEmpty())
            {
                work.Account.ConfirmEmailByIdandCode(userId, code);
            }

            ViewBag.ReturnUrl = returnUrl;

            var model = new LoginViewModel
            {
                ResponseStatus = LoginResponseStatus.LoadLoginForm,
                LoginVia = LoginVia.Email
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            string logObj;
            const string alertMessage = "entered does not match a record in our system. Please double check your entry and try again.";
            // Set the flag to false after the first submission
            bool isFirstSubmission = GetAndClearIsFirstSubmissionFlag();

            SessionVariables.Clear();

            ApplicationUser user = null;

            //look up email or phone for existing accounts. If not found, show create account, else show payment info
            if (model.LoginVia == LoginVia.Email)
            {
                user = work.User.GetByEmail(model.Email);

                if (user.IsNullOrEmpty() && model.LoginVia == LoginVia.Email)
                {
                    model.ResponseStatus = LoginResponseStatus.LoadLoginForm;
                    model.LoginVia = LoginVia.Email;
                    CreateAlertMessage("The email " + alertMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    ModelState.AddModelError("Email", "Please enter a valid email.");
                    return View(model);
                }

                if (model.ResponseStatus == LoginResponseStatus.LoadLoginForm && string.IsNullOrEmpty(user.PasswordHash))
                {
                    model.ResponseStatus = LoginResponseStatus.SetupPassword;
                    return View(model);
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        model.LoginVia = LoginVia.Email;
                        model.ResponseStatus = LoginResponseStatus.LoginWithPassword;
                    }
                }
            }
            else if (model.LoginVia == LoginVia.Phone)
            {
                user = work.User.GetByPhone(model.Phone);

                if (user.IsNullOrEmpty())
                {
                    model.ResponseStatus = LoginResponseStatus.LoadLoginForm;
                    model.LoginVia = LoginVia.Phone;
                    CreateAlertMessage("The phone number " + alertMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(model);
                }

                if (model.ResponseStatus == LoginResponseStatus.LoadLoginForm && string.IsNullOrEmpty(user.PasswordHash))
                {
                    model.ResponseStatus = LoginResponseStatus.SetupPassword;
                    return View(model);
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        model.LoginVia = LoginVia.Phone;
                        model.ResponseStatus = LoginResponseStatus.LoginWithPassword;
                    }
                }
            }

            if (model.ResponseStatus == LoginResponseStatus.LoginWithPassword && string.IsNullOrEmpty(model.Password))
            {
                if (!isFirstSubmission)
                {
                    ModelState.AddModelError("Password", "The password field is required.");
                    TempData["IsFirstSubmission"] = false;
                    CreateAlertMessage("Please enter your password and try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }
                return View(model);
            }

            if (model.ResponseStatus == LoginResponseStatus.SetupPassword && !string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ConfirmPassword))
            {
                user.Email = model.Email;
                user.PhoneNumber = model.Phone;
                user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                user.LastLogin = DateTime.Now;
                user.SecurityStamp = Utilities.GenerateUniqueId();
                work.User.Update(user);
            }

            if (user.IsNotNullOrEmpty() && !user.IsActive)
            {
                CreateAlertMessage("Uh-oh! Your Account has been deactivated. Please contact your administrator if you believe this was done in error.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return View(model);
            }
            var userName = model.LoginVia == LoginVia.Phone ? model.Phone : model.Email;
            var result = SignInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, shouldLockout: false);

            if (result.Result == SignInStatus.Success)
            {
                if (user.TwoFactorEnabled)
                {
                    string provider = null;

                    if ((user.EmailConfirmed && user.PhoneNumberConfirmed) || (user.PhoneNumberConfirmed && !user.EmailConfirmed))
                    {
                        provider = "Phone";
                        user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                        work.User.Update(user);

                        //Send welcome sms text
                        var smsMessage = new SmsMessage
                        {
                            Id = Utilities.GenerateUniqueId(),
                            To = user.PhoneNumber,
                            Message = "Your one-time Praise verification code is: " + user.PhoneVerificationCode +
                                "\n\nRequested: " + DateTime.Now,
                            CreatedDate = DateTime.Now,
                            CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                            Type = SmsMessageType.SignIn
                        };
                        Utilities.SendMessage(smsMessage);
                    }
                    else
                    {
                        provider = "Email";
                        user.EmailVerificationCode = Utilities.GenerateVerificationCode();
                        work.User.Update(user);
                        var content = EmailTemplates.VerificationCode_body.Replace("{verification-code}", user.EmailVerificationCode);
                        var email = new Email()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Message = content,
                            To = user.Email,
                            Attachments = null,
                            Subject = "Your Praise Verification Code",
                            CreatedBy = user.Id,
                            CreatedDate = DateTime.Now
                        };
                        Emailer.SendEmail(email);
                    }

                    return RedirectToAction(nameof(VerifyCode), new { t = user.SecurityStamp, provider, rememberMe = model.RememberMe, returnUrl, email = user.Email });
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, Constants.InactiveAccountMessage);

                    return View(model);
                }

                user.LastLogin = DateTime.Now;

                //Get list of all churches the user belongs to and
                //if multiple let the user specify the church to connect to.
                var userChurches = work.Church.GetAllChurchUsersByUserId(user.Id);

                if (userChurches.Count > 1)
                {
                    return RedirectToAction(nameof(SelectChurch));
                }

                Utilities.SetSessionVariables(user, userChurches.FirstOrDefault()?.ChurchId);
                work.User.Update(user);

                if (!user.AssignedToChurch)
                {
                    return RedirectToAction("ChurchWelcome", "Onboarding");
                }

                if (SessionVariables.CurrentUser.IsSuperAdmin)
                {
                    return Redirect("/");
                }

                if (SessionVariables.CurrentUser.MemberOf(Roles.Donor))
                {
                    var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

                    if (givingAmount.IsNotNull())
                    {
                        return RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });
                    }
                }
            }

            switch (result.Result)
            {
                case SignInStatus.Success:
                    logObj = logRepository.JsonConverter("Email", model.Email);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Login", SessionVariables.CurrentUser.User.Id, LogStatuses.Done, logObj);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    logObj = logRepository.JsonConverter("Email", model.Email);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Login", model.Email, "LockedOut", logObj);
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    logObj = logRepository.JsonConverter("Email", model.Email);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Login", model.Email, "RequiresVerification", logObj);
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                default:
                    CreateAlertMessage("Invalid email or password. Please double-check your credentials and try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(model);
            }
        }

        private bool GetAndClearIsFirstSubmissionFlag()
        {
            bool isFirstSubmission;

            // Check if the TempData contains the key "IsFirstSubmission"
            if (TempData.ContainsKey("IsFirstSubmission"))
            {
                // Retrieve and cast the value from TempData
                isFirstSubmission = (bool)TempData["IsFirstSubmission"];

                // Remove the key from TempData to ensure it's cleared
                TempData.Remove("IsFirstSubmission");
            }
            else
            {
                // If the key is not found, default to true
                isFirstSubmission = true;

                // Set the flag to false after the first submission
                TempData["IsFirstSubmission"] = false;
            }

            return isFirstSubmission;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UserLogin(string Email, string Password)
        {
            SessionVariables.Clear();
            var result = SignInManager.PasswordSignInAsync(Email, Password, isPersistent: false, shouldLockout: false);
            var paymentMethods = new List<SelectListItems>();
            var donorId = string.Empty;

            if (result.Result != SignInStatus.Success)
            {
                return Json(new { Success = false, Message = "Invalid Email or Password!" });
            }

            var user = work.User.GetByEmailPlain(Email);

            if (!user.IsActive)
            {
                return Json(new { Success = false, Message = Constants.InactiveAccountMessage });
            }

            Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);

            var userMerchantAccount = work.UserMerchantAccount.GetByUserId(user.Id);

            if (userMerchantAccount?.DonorGUID.IsNotNullOrEmpty() != true)
            {
                return Json(new
                {
                    Success = true,
                    DonorId = donorId,
                    NewUser = false,
                    PaymentMethods = paymentMethods,
                    Message = "Login Success!"
                });
            }

            donorId = userMerchantAccount.DonorGUID;
            paymentMethods = work.Payment.GetPaymentMethodsDropdownList(userMerchantAccount.DonorGUID);

            return Json(new { Success = true, DonorId = donorId, NewUser = false, PaymentMethods = paymentMethods, Message = "Login Success!" });
        }

        [AllowAnonymous]
        public ActionResult SignIn(string churchId, string amount, string fund)
        {
            var model = new GivingSignUpViewModel
            {
                Church = work.Church.Get(churchId),
                Payment = new Payment
                {
                    Amount = Convert.ToDecimal(amount, CultureInfo.InvariantCulture),
                    FundId = fund
                },
                RegisterVia = RegisterVia.PhoneNumber,
                ResponseStatus = SignInResponseStatus.LoadLoginForm
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult SignIn(GivingSignUpViewModel model)
        {
            SessionVariables.Clear();
            ApplicationUser user;

            //look up email or phone for existing accounts. If not found, show create account, else show payment info
            if (model.RegisterVia == RegisterVia.PhoneNumber)
            {
                //look for user by phone
                user = work.User.GetByPhonePlain(model.Phone);

                if (user.IsNotNull())
                {
                    user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                    work.User.Update(user);

                    var smsMessage = new SmsMessage
                    {
                        Id = Utilities.GenerateUniqueId(),
                        To = model.Phone,
                        Message = "Your " + (model.Church != null && !string.IsNullOrEmpty(model.Church.Name) ? model.Church.Name : "Praise") +
                            " verification code is: " + user.PhoneVerificationCode +
                                  "\n\nRequested: " + DateTime.Now,
                        CreatedDate = DateTime.Now,
                        CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                        Type = SmsMessageType.SignIn
                    };
                    Utilities.SendMessage(smsMessage);

                    model.ResponseStatus = SignInResponseStatus.LoadConfirmationCode;
                    return View(model);
                }

                model.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;
                model.RegisterVia = RegisterVia.PhoneNumber;

                return View("Register", model);
            }

            if (model.RegisterVia == RegisterVia.Email)
            {
                //look for user by email
                var userExist = work.User.AnyByEmail(model.Email);

                if (!userExist)
                {
                    model.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;
                    model.RegisterVia = RegisterVia.Email;

                    return View(model);
                }

                if (!model.IsValid)
                {
                    model.ResponseStatus = SignInResponseStatus.LoadPassword;
                    model.IsValid = true;

                    return View(model);
                }

                var results = SignInManager.PasswordSignInAsync(model.Email, model.Password, true, shouldLockout: false);

                if (results.Result == SignInStatus.Success)
                {
                    user = work.User.GetByEmailPlain(model.Email);

                    if (user.IsNotNull())
                    {
                        Utilities.SetSessionVariables(user, model.Church?.Id);
                    }

                    if (SessionVariables.CurrentUser.IsDonorOnly)
                    {
                        var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

                        if (givingAmount.IsNotNull())
                        {
                            return RedirectToAction("Index", "MyGiving");
                        }

                        return RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });
                    }
                }
                else
                {
                    model.ResponseStatus = SignInResponseStatus.LoadPassword;
                }

                switch (results.Result)
                {
                    case SignInStatus.Success:
                        return RedirectToAction("Index", "Home", new { amount = model.Payment.Amount, fund = model.Payment.FundId });
                    case SignInStatus.LockedOut:
                    case SignInStatus.RequiresVerification:
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                }
            }

            return RedirectToAction("SignIn", "Account", new { churchId = model.Church.Id, amount = model.Payment.Amount, fund = model.Payment.FundId });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhoneLogin(PhoneLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return RedirectToLocal(returnUrl);

            var user = work.User.GetByPhone(model.Phone);

            if (user == null) return View(model);

            user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
            var result = await UserManager.UpdateAsync(user);

            if (!result.Succeeded) return View(model);

            var smsMessage = new SmsMessage
            {
                Id = Utilities.GenerateUniqueId(),
                To = model.Phone,
                Message = "Your " + (model.Church != null && !string.IsNullOrEmpty(model.Church.Name) ? model.Church.Name : "Praise") +
                          " verification code is: " + user.PhoneVerificationCode +
                          "\n\nRequested: " + DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                Type = SmsMessageType.SignIn
            };
            Utilities.SendMessage(smsMessage);

            var logObj = logRepository.JsonConverter("Phone#", model.Phone, "Verification Code", model.VerificationCode);
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Login by phone", user.Id, LogStatuses.Done, logObj);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult VerifyCode(string t, string provider, string returnUrl, bool rememberMe, string email)
        {
            // Require that the user has already logged in via username/password or external login
            //if (!await SignInManager.HasBeenVerifiedAsync())
            //{
            //    return View("Error");
            //}
            return View(new VerifyCodeViewModel { Email = email, Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe, Token = t });
        }

        [HttpGet]
        public ActionResult ResendCodeTwoFactor(string email)
        {
            var user = work.User.GetByEmailPlain(email);
            var result = false;

            if (!user.IsNotNullOrEmpty() || !user.TwoFactorEnabled)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            if ((user.EmailConfirmed && user.PhoneNumberConfirmed) || (user.PhoneNumberConfirmed && !user.EmailConfirmed))
            {
                user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                work.User.Update(user);

                //Send welcome sms text
                var smsMessage = new SmsMessage
                {
                    Id = Utilities.GenerateUniqueId(),
                    To = user.PhoneNumber,
                    Message = "Your one-time Praise verification code is: " + user.PhoneVerificationCode +
                              "\n\nRequested: " + DateTime.Now,
                    CreatedDate = DateTime.Now,
                    CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                    Type = SmsMessageType.SignIn
                };
                result = Utilities.SendMessage(smsMessage);
            }
            else
            {
                user.EmailVerificationCode = Utilities.GenerateVerificationCode();
                work.User.Update(user);

                var content = EmailTemplates.VerificationCode_body.Replace("{verification-code}", user.EmailVerificationCode);
                var emailObj = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = content,
                    To = user.Email,
                    Attachments = null,
                    Subject = "Your Praise Verification Code",
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now
                };
                Emailer.SendEmail(emailObj);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.Any(q => q.Errors.Any()) ? ModelState.Values.FirstOrDefault(q => q.Errors.Any())?.Errors.FirstOrDefault()?.ErrorMessage : "Please enter the verification code.";
                CreateAlertMessage(error, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var errorObj = logRepository.JsonConverter("Description", error);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Code Verify", string.Empty, LogStatuses.Error, errorObj);

                return View(model);
            }

            var user = work.User.GetBySecurityStampAndEmailCode(model);

            if (user.IsNull())
            {
                CreateAlertMessage("Invalid code. Please verify your code and try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return View(model);
            }

            user.SecurityStamp = Guid.NewGuid().ToString();
            work.User.Update(user);

            Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);

            if (!user.AssignedToChurch)
            {
                return RedirectToAction("churchwelcome", "onboarding");
            }

            return Redirect("/");
        }

        #region Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new SignUpViewModel
            {
                Church = new Church(),
                PlanType = PlanType.Premium,
                Payment = new Payment(),
                RegisterVia = RegisterVia.Email,
                ResponseStatus = SignInResponseStatus.LoadRegistrationForm,
                ValidateCaptcha = TempData["enableRecapcha"] == null
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateGoogleCaptcha]
        [ValidateAntiForgeryToken]
        public ActionResult Register(SignUpViewModel model)
        {
            SessionVariables.Clear();
            Utilities.AddToCookies("recapcha", DateTime.Now.AddMinutes(15), true);

            //look up email or phone for existing accounts. If not found, show create account, else show payment info
            if (!model.IsValid)
            {
                if (model.RegisterVia == RegisterVia.Email)
                {
                    model.Phone = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Phone))
                    {
                        CreateAlertMessage("Please enter a phone number.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                        //ModelState.AddModelError("Phone", "Please enter a phone number.");
                        model.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;
                        model.IsValid = true; // Set IsValid to true to prevent model binder from re-validating the entire model
                        return View(model);
                    }

                    if (!model.Phone.IsValidNumber())
                    {
                        //ModelState.AddModelError("Phone", "Please enter a valid phone number.");
                        CreateAlertMessage("Please enter a valid phone number.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                        model.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;
                        model.IsValid = true;
                        return View(model);
                    }

                    model.Email = null;
                }
            }

            var user = work.User.GetByEmailAndPhone(model.Email, model.Phone);

            if (user.IsNotNull())
            {
                var response = user.PhoneNumber.IsNotNullOrEmpty() && user.PhoneNumber == model.Phone
                    ? Constants.PhoneAlreadyRegistered
                    : Constants.EmailAlreadyRegistered;

                ModelState.AddModelError(string.Empty, response);
                model.ResponseStatus = model.IsValid ? SignInResponseStatus.LoadRegistrationDetailsForm : SignInResponseStatus.LoadRegistrationForm;
                model.ValidateCaptcha = !model.IsValid;

                return View(model);
            }

            if (!model.IsValid)
            {
                model.ResponseStatus = SignInResponseStatus.LoadRegistrationDetailsForm;
                model.IsValid = true;
                model.ValidateCaptcha = false;

                return View(model);
            }

            model.Password = model.RegisterVia == RegisterVia.Email ? model.Password : Constants.GeneratePassword(true, true, true, true, false, 24);
            var person = work.Person.GetByEmailAndPhone(model.Email, model.Phone);

            if (person.IsNullOrEmpty())
            {
                person = new Person()
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedDate = DateTime.Now,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email?.ToLower(),
                    PhoneNumber = model.Phone,
                    IsActive = true
                };
                var personResult = work.Person.Create(person);

                if (personResult.ResultType != ResultType.Success)
                {
                    ModelState.AddModelError(string.Empty, personResult.Message);
                    model.ResponseStatus = SignInResponseStatus.LoadRegistrationDetailsForm;

                    return View(model);
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
                UserName = model.RegisterVia == RegisterVia.Email ? model.Email : model.Phone,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = Constants.System,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email?.ToLower(),
                PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password),
                SecurityStamp = Utilities.GenerateUniqueId(),
                PhoneNumber = model.Phone.IsNullOrEmpty() ? null : model.Phone,
                PhoneVerificationCode = Utilities.GenerateVerificationCode(),
                ExternalProvider = null,
                ExternalProviderId = null,
                AssignedToChurch = false,
                ShowWelcomeMessage = true,
                PersonId = person.Id
            };

            var result = UserManager.Create(user, model.Password);

            if (!result.Succeeded)
            {
                var errorList = result.Errors.FirstOrDefault();
                ModelState.AddModelError(string.Empty, errorList);
                model.ResponseStatus = SignInResponseStatus.LoadRegistrationDetailsForm;

                return View(model);
            }

            adoData.InsertUserRoleByName(user.Id, Roles.Administrator);

            var userSettings = new UserSetting
            {
                Id = Utilities.GenerateUniqueId(),
                UserId = user.Id,
                CreatedBy = user.Id,
                CreatedDate = DateTime.Now
            };
            var settingsResult = work.UserSetting.Create(userSettings);

            if (settingsResult.ResultType != ResultType.Success)
            {
                ModelState.AddModelError(string.Empty, settingsResult.Message);
                model.ResponseStatus = SignInResponseStatus.LoadRegistrationDetailsForm;

                return View(model);
            }

            switch (model.RegisterVia)
            {
                case RegisterVia.Email:
                    {
                        var newUser = work.User.Get(user.Id);
                        Utilities.SetSessionVariables(newUser);

                        return RedirectToAction(nameof(OnboardingController.ChurchWelcome), "Onboarding", new { plan = model.PlanType });
                    }
                case RegisterVia.PhoneNumber:
                    //Send welcome SMS text message
                    var smsMessage = new SmsMessage
                    {
                        Id = Utilities.GenerateUniqueId(),
                        To = model.Phone,
                        Message = "Your " + (model.Church != null && !string.IsNullOrEmpty(model.Church.Name) ? model.Church.Name : "Praise") +
                                  " verification code is: " + user.PhoneVerificationCode +
                                  "\n\nRequested: " + DateTime.Now,
                        CreatedDate = DateTime.Now,
                        CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                        Type = SmsMessageType.SignIn
                    };
                    Utilities.SendMessage(smsMessage);
                    model.ResponseStatus = SignInResponseStatus.LoadConfirmationCode;

                    return View(model);

                default:
                    return View(model);
            }
        }
        #endregion

        public ActionResult Success()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult PhoneNumber(GivingSignUpViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _PhoneSignIn(GivingSignUpViewModel model)
        {
            //send phone confirmation code here and show confirmation boxes
            var user = work.User.GetByPhone(model.Phone);

            //If the user is not found by phone number, let's treat the user as new and show the email sign up form
            if (user == null) return PartialView("_EmailSignup", model);

            model.AccountFound = true;
            user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
            var result = UserManager.Update(user);

            if (!result.Succeeded) return PartialView("_PhoneConfirm");

            var smsMessage = new SmsMessage
            {
                Id = Utilities.GenerateUniqueId(),
                To = model.Phone,
                Message = "Your " + (model.Church != null && !string.IsNullOrEmpty(model.Church.Name) ? model.Church.Name : "Praise") +
                          " verification code is: " + user.PhoneVerificationCode +
                          "\n\nRequested: " + DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                Type = SmsMessageType.SignIn
            };
            Utilities.SendMessage(smsMessage);

            return PartialView("_PhoneConfirm", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult _PhoneSignUp(PhoneSignUpViewModel model)
        {
            //send phone confirmation code here and show confirmation boxes
            var user = work.User.GetByPhone(model.Phone);

            if (user == null) return PartialView("_PhoneConfirm");

            user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
            var result = UserManager.Update(user);

            if (!result.Succeeded) return PartialView("_PhoneConfirm");

            var smsMessage = new SmsMessage
            {
                Id = Utilities.GenerateUniqueId(),
                To = model.Phone,
                Message = "Your Praise verification code is: " + user.PhoneVerificationCode +
                          "\n\nRequested: " + DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                Type = SmsMessageType.SignIn
            };
            Utilities.SendMessage(smsMessage);

            return PartialView("_PhoneConfirm", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult _PhoneConfirm()
        {
            //verify phone code here, then return email sign up form or error
            return PartialView("_EmailSignup");
        }

        public ActionResult _ResendCode(string churchName, string phone)
        {
            var user = work.User.GetByPhone(phone);

            if (user == null) return null;

            user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
            var result = UserManager.Update(user);

            if (!result.Succeeded) return null;

            var smsMessage = new SmsMessage
            {
                Id = Utilities.GenerateUniqueId(),
                To = phone,
                Message = "Your " + (!string.IsNullOrEmpty(churchName) ? churchName : "Praise") +
                          " verification code is: " + user.PhoneVerificationCode +
                          "\n\nRequested: " + DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                Type = SmsMessageType.SignIn
            };
            Utilities.SendMessage(smsMessage);

            return null;
        }

        [AllowAnonymous]
        public ActionResult PhoneVerification(SignUpViewModel model)
        {
            var user = work.User.GetByVerificationCode(model.Phone, model.VerificationCode);

            if (user != null && user.PhoneVerificationCode == model.VerificationCode)
            {
                user.PhoneNumberConfirmed = true;
                work.User.Update(user);

                Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);

                if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

                var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

                return givingAmount.IsNotNull() ? RedirectToAction("Index", "MyGiving") : RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });
            }

            ModelState.AddModelError(string.Empty, "Invalid verification code");
            model.ResponseStatus = SignInResponseStatus.LoadConfirmationCode;

            return View("Register", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult _EmailSignup(string status, string phoneNumber)
        {
            var model = new GivingSignUpViewModel();

            //If verified, then user already confirmed phone number
            if (!string.IsNullOrEmpty(status))
            {
                model.Title = "Finish signing up";
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                model.Phone = phoneNumber;
            }

            return PartialView("_EmailSignup", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _EmailSignup(GivingSignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { Id = Utilities.GenerateUniqueId(), UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, PhoneVerificationCode = Utilities.GenerateVerificationCode(), IsActive = true, CreatedDate = DateTime.Now };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //TODO
                    //Auto Assign Permissions

                    //Check that sign in works..doesn't appear so
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    var logObj = logRepository.JsonConverter("First Name", model.FirstName, "Last Name", model.LastName, "Email", model.Email, "Password", model.Password, "Confirm Password", model.ConfirmPassword);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "User Registration", user.Id, LogStatuses.Done, logObj);

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }

            var errorObj = logRepository.JsonConverter();
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Register", string.Empty, LogStatuses.Error, errorObj);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                var errorObj = logRepository.JsonConverter("UserID", userId, "Code", code);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "ConfirmEmail", string.Empty, LogStatuses.Error, errorObj);

                return View("Error");
            }

            var user = work.Account.ConfirmEmailByIdandCode(userId, code);

            if (user == null) return View("The token has expired or is invalid.");

            var logObj = logRepository.JsonConverter("UserID", userId, "Code", code);
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "ConfirmEmail", userId, LogStatuses.Done, logObj);

            Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);

            if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

            var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

            return givingAmount.IsNotNull() ? RedirectToAction("Index", "MyGiving") : RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });
        }

        #region Forgot Password
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var logObj = logRepository.JsonConverter("Email", model.Email);
                var user = work.User.GetByEmailPlain(model.Email);

                if (user == null || (await UserManager.IsEmailConfirmedAsync(user.Id)).IsNull())
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Forgot Password", string.Empty, LogStatuses.Error, logObj);
                    CreateAlertMessage("If the provided email address corresponds to an existing record in our system, you will receive an email with instructions on how to reset your password.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);

                    return View(model);
                }

                // Send an email with this link
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = user.SecurityStamp }, protocol: Request.Url.Scheme);
                var content = EmailTemplates.ForgotPassword_body.Replace("{btn_url}", callbackUrl);
                var email = new Email
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = content,
                    To = user.Email,
                    Subject = "Reset Your Password - Praise",
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now
                };
                Emailer.SendEmail(email);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Forgot Password", user.Id, LogStatuses.Done, logObj);
                CreateAlertMessage("Please check your email for a link to reset your password.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return View();
            }

            DisplayErrors();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region Reset Password
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            const string alertMessage = "Click the Forgot Password button below to request a new link to reset your password.";

            if (string.IsNullOrEmpty(userId))
            {
                CreateAlertMessage("Your userId is invalid. " + alertMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return View("ResetPasswordConfirmation");
            }

            if (string.IsNullOrEmpty(code))
            {
                CreateAlertMessage("Your token is invalid. " + alertMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return View("ResetPasswordConfirmation");
            }

            var user = work.User.GetBySecurityStamp(userId: userId, securityStamp: code);

            if (user.IsNull())
            {
                CreateAlertMessage("Your account could not be found. " + alertMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                return View("ResetPasswordConfirmation");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errorObj = logRepository.JsonConverter();
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "ResetPassword", string.Empty, LogStatuses.Error, errorObj);

                DisplayErrors();

                return View(model);
            }

            var user = work.User.GetByEmailAndSecurityStamp(model.Code, model.Email);

            if (user.IsNull())
            {
                var errorObj = logRepository.JsonConverter();
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "ResetPassword", string.Empty, LogStatuses.Error, errorObj);
                ModelState.AddModelError(string.Empty, "The email address could not be found.");

                return View(model);
            }

            model.Code = model.Code.Replace(" ", "+");

            if (user.SecurityStamp != model.Code)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or username");

                return View(model);
            }

            user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
            user.LastLogin = DateTime.Now;
            user.SecurityStamp = Utilities.GenerateUniqueId();
            work.User.Update(user);

            var logObj = logRepository.JsonConverter("Email", model.Email, "Password", model.Password, "Confirm Password", model.ConfirmPassword, "Code", model.Code);
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, string.Empty, user.Id, LogStatuses.Done, logObj);

            Utilities.SetSessionVariables(user, null);

            if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

            //var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

            return RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });

            //if (givingAmount.IsNotNull())
            //{
            //    return RedirectToAction("GivingSummary", "Giving");
            //}
            //else
            //{
            //    return RedirectToAction("StartGiving", "GivingWorkflow", new { Id = SessionVariables.CurrentChurch.Id });
            //}
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SetupPassword(SetupPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errorObj = logRepository.JsonConverter();
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "ResetPassword", string.Empty, LogStatuses.Error, errorObj);

                return View(model);
            }

            var user = work.User.GetByEmailPlain(model.Email);

            if (!user.IsNotNullOrEmpty())
            {
                ModelState.AddModelError(string.Empty, "Invalid email or username");
                return View(model);
            }

            user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
            user.LastLogin = DateTime.Now;
            user.SecurityStamp = Utilities.GenerateUniqueId();
            work.User.Update(user);

            var logObj = logRepository.JsonConverter("Email", model.Email, "Password", model.Password, "Confirm Password", model.ConfirmPassword);
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, string.Empty, user.Id, LogStatuses.Done, logObj);

            Utilities.SetSessionVariables(user, null);

            if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

            //var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

            return RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });

            //if (givingAmount.IsNotNull())
            //{
            //    return RedirectToAction("GivingSummary", "Giving");
            //}
            //else
            //{
            //    return RedirectToAction("StartGiving", "GivingWorkflow", new { Id = SessionVariables.CurrentChurch.Id });
            //}
        }

        [AllowAnonymous]
        public ActionResult PinLogin()
        {
            return View();
        }

        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string t, string returnUrl, bool rememberMe)
        //{
        //    var userId = await SignInManager.GetVerifiedUserIdAsync();

        //    if (userId == null)
        //    {
        //        return View("Error");
        //    }

        //    var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();

        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errorObj = logRepository.JsonConverter();
        //        logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "", "", LogStatuses.Error, errorObj);

        //        return View();
        //    }

        //    var logObj = logRepository.JsonConverter();

        //    // Generate the token and send it
        //    if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //    {
        //        logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "", "", LogStatuses.Error, logObj);
        //        return View("Error");
        //    }

        //    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "", "", LogStatuses.Done, logObj);

        //    return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        #region LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var logObj = logRepository.JsonConverter("UserId", User.Identity.GetUserId());
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "LogOff", User.Identity.GetUserId(), LogStatuses.Done, logObj);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            SessionVariables.Clear();

            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Giving User
        [HttpPost]
        public ActionResult GivingRegister(GivingRegisterModel model)
        {
            var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");
            var viewModel = new GivingSignUpViewModel
            {
                Church = work.Church.Get(givingAmount.ChurchId),
                Payment = new Payment
                {
                    Amount = Convert.ToDecimal(givingAmount.Amount, CultureInfo.InvariantCulture),
                    FundId = givingAmount.Fund
                }
            };

            if (ModelState.IsValid)
            {
                var dbUserEmailExist = work.User.GetByEmailPlain(model.Email);

                if (dbUserEmailExist.IsNotNull())
                {
                    viewModel.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;
                    ModelState.AddModelError(string.Empty, "It looks like there is an account already associated with the email provided. Please sign in.");

                    return View("SignIn", viewModel);
                }

                var dbUserPhoneExist = work.User.GetByPhonePlain(model.Phone);

                if (dbUserPhoneExist.IsNotNull())
                {
                    viewModel.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;
                    ModelState.AddModelError(string.Empty, "It looks like there is an account already associated with the phone number provided. Please sign in.");

                    return View("SignIn", viewModel);
                }

                model.Id = Utilities.GenerateUniqueId();
                model.ChurchId = viewModel.Church.Id;
                model.LoginProvider = null;

                var result = work.Account.CombineExternalRegistration(model);

                if (result.ResultType != ResultType.Success) return RedirectToAction("SignIn", "Account");

                if (model.RegisterVia == RegisterVia.PhoneNumber)
                {
                    SessionVariables.Clear();
                    var user = work.User.GetByPhone(model.Phone);
                    var church = work.Church.Get(viewModel.Church.Id);

                    if (user != null)
                    {
                        user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                        work.User.Update(user);

                        var smsMessage = new SmsMessage
                        {
                            Id = Utilities.GenerateUniqueId(),
                            To = model.Phone,
                            Message = "Your " + (church != null && !string.IsNullOrEmpty(church.Name) ? church.Name : "Praise") +
                                      " verification code is: " + user.PhoneVerificationCode +
                                      "\n\nRequested: " + DateTime.Now,
                            CreatedDate = DateTime.Now,
                            CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                            Type = SmsMessageType.SignIn
                        };
                        Utilities.SendMessage(smsMessage);
                    }

                    viewModel.ResponseStatus = SignInResponseStatus.LoadConfirmationCode;

                    return View("SignIn", viewModel);
                }
                else
                {
                    var user = work.User.Get(model.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = user.SecurityStamp }, protocol: Request.Url.Scheme);
                    var email = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = "Please activate your account by clicking <a href=\"" + callbackUrl + "\">here</a>",
                        To = user.Email,
                        Attachments = null,
                        Subject = "Account Activation - Praise",
                        CreatedBy = user.Id,
                        CreatedDate = DateTime.Now
                    };
                    Emailer.SendEmail(email);

                    return RedirectToAction("StartGiving", "GivingWorkflow", new { id = model.ChurchId });
                }
            }

            viewModel.ResponseStatus = SignInResponseStatus.LoadRegistrationForm;

            return View("SignIn", viewModel);
        }

        [AllowAnonymous]
        public ActionResult VerificationCode(VerificationCodeModel model)
        {
            var cookies = HttpContext.Request.Cookies["giving_amount"];
            var jss = new JavaScriptSerializer();
            var ds = jss.Deserialize<GivingAmountModel>(cookies?.Value);

            var obj = new GivingSignUpViewModel
            {
                Church = work.Church.Get(ds.ChurchId),
                Payment = new Payment
                {
                    Amount = Convert.ToDecimal(ds.Amount, CultureInfo.InvariantCulture),
                    FundId = ds.Fund
                },
                Phone = model.Phone
            };

            if (!ModelState.IsValid)
            {
                obj.ResponseStatus = SignInResponseStatus.LoadConfirmationCode;

                return View("SignIn", obj);
            }

            var user = work.User.GetByPhone(model.Phone);

            if (user != null && user.PhoneVerificationCode == model.VerificationCode)
            {
                Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);

                if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

                //var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

                return RedirectToAction("StartGiving", "GivingWorkflow", new { SessionVariables.CurrentChurch.Id });
                //if (givingAmount.IsNotNull())
                //{
                //    return RedirectToAction("GivingSummary", "Giving");
                //}
                //else
                //{
                //    return RedirectToAction("StartGiving", "GivingWorkflow", new { Id = SessionVariables.CurrentChurch.Id });
                //}
            }

            obj.ResponseStatus = SignInResponseStatus.LoadConfirmationCode;
            ModelState.AddModelError("VerificationCodeModel", "The provided verification code is invalid.");

            return View("SignIn", obj);
        }
        #endregion

        #region Third-Party Logins        
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult ExternalLogin(string provider, string returnUrl)
        //{
        //    // Request a redirect to the external login provider
        //    return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        //}

        #region Facebook
        /// <summary>
        /// Facebook doesn't provide phone number https://developers.facebook.com/docs/facebook-login/permissionss/
        /// </summary>
        /// <returns></returns>
        //public ActionResult Facebook(string from = null)
        //{
        //    if (from == "register")
        //    {
        //        Utilities.RemoveCookies("giving_amount");
        //    }

        //    var fb = new FacebookClient();
        //    var loginUrl = fb.GetLoginUrl(new
        //    {
        //        client_id = "Facebook.Id".AppSetting<string>(null),
        //        client_secret = "Facebook.Secret".AppSetting<string>(null),
        //        redirect_uri = RedirectUri.AbsoluteUri,
        //        response_type = "code",
        //        scope = "email"
        //    });

        //    return Redirect(loginUrl.AbsoluteUri);
        //}

        //public ActionResult FacebookCallBack(string code)
        //{
        //    try
        //    {
        //        var cookies = HttpContext.Request.Cookies["giving_amount"];
        //        var jss = new JavaScriptSerializer();
        //        var obj = jss.Deserialize<GivingAmountModel>(cookies.Value);
        //        var fb = new FacebookClient();
        //        dynamic fbPost = fb.Post("oauth/access_token", new
        //        {
        //            client_id = "Facebook.Id".AppSetting<string>(null),
        //            client_secret = "Facebook.Secret".AppSetting<string>(null),
        //            redirect_uri = RedirectUri.AbsoluteUri,
        //            response_type = "code",
        //            code = code
        //        });

        //        var accessToken = fbPost.access_token;
        //        fb.AccessToken = accessToken;
        //        dynamic profile = fb.Get("me?fields=link,id,name,first_name,currency,last_name,email,gender,locale,timezone,verified,picture,age_range");

        //        var result = work.Account.CombineExternalRegistration(new GivingRegisterModel
        //        {
        //            ChurchId = obj.ChurchId,
        //            Email = profile.email,
        //            FirstName = profile.first_name,
        //            LastName = profile.last_name,
        //            Id = Utilities.GenerateUniqueId(),
        //            Password = LoginProvider.Facebook,
        //            Phone = null,
        //            ProfileImage = profile.picture.data.url,
        //            Username = profile.email,
        //            LoginProvider = LoginProvider.Facebook,
        //            ExternalProviderId = profile.id
        //        });

        //        if (result.ResultType != ResultType.Success) return RedirectToAction("SignIn", "Account");

        //        if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

        //        var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

        //        return givingAmount.IsNotNull() ? RedirectToAction("Index", "MyGiving") : RedirectToAction("StartGiving", "GivingWorkflow", new { Id = SessionVariables.CurrentChurch.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionLogger.LogException(ex);
        //        return RedirectToAction("SignIn", "Account");
        //    }
        //}

        //private Uri RedirectUri
        //{
        //    get
        //    {
        //        var uriBuilder = new UriBuilder(Request.Url)
        //        {
        //            Query = null,
        //            Fragment = null,
        //            Path = Url.Action("FacebookCallBack")
        //        };

        //        return uriBuilder.Uri;
        //    }
        //}
        #endregion

        #region Google
        /// <summary>
        /// Stackoverflow -> https://stackoverflow.com/questions/37876125/how-can-we-get-the-phone-number-with-google-oauth-api-and-facebook-api-used-for
        /// Google playground - https://developers.google.com/oauthplayground/
        /// Google Enable Person -> https://console.developers.google.com/apis/library/people.googleapis.com?project=quickstart-1589237999141
        /// Package Manager -> Install-Package RestSharp -Version 106.11.3
        /// github "How to call REST Client api with headers" -> https://gist.github.com/teocomi/9b65f59de827435000a3
        /// </summary>
        /// <returns></returns>
        public ActionResult Google(string from)
        {
            SessionVariables.Clear();

            if (from == "register")
            {
                Utilities.RemoveCookies("giving_amount");
            }

            var request = string.Format("https://accounts.google.com/o/oauth2/v2/auth?access_type=offline&response_type=code&client_id={0}&redirect_uri={1}&scope={2}",
                "Google.Id".AppSetting<string>(null),
                "Google.CallBackUri".AppSetting<string>(null),
                "Google.Scope".AppSetting<string>(null), from);

            return Redirect(request);
        }

        public ActionResult GoogleCallBack(string code)
        {
            SessionVariables.Clear();

            if (code.IsNullOrEmpty())
            {
                return RedirectToAction("SignIn", "Account");
            }

            var url =
                $"code={code}&client_id={"Google.Id".AppSetting<string>(null)}&client_secret={"Google.Secret".AppSetting<string>(null)}&redirect_uri={"Google.CallBackUri".AppSetting<string>(null)}&grant_type=authorization_code";

            // Request for access token
            var encoder = new ASCIIEncoding();
            var data = encoder.GetBytes(url);

            var request = WebRequest.Create("https://oauth2.googleapis.com/token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);

            var sd = new StreamReader(request.GetResponse().GetResponseStream());
            var reqSR = sd.ReadToEnd();
            var tokenResponse = new JavaScriptSerializer().Deserialize<GoogleTokenModel>(reqSR);

            // Request for user profile
            var profileRequest = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + tokenResponse.access_token;
            var profileResponse = WebRequest.Create(profileRequest);
            var reader = new StreamReader(profileResponse.GetResponse().GetResponseStream());
            var readerRead = reader.ReadToEnd();

            var profile = new JavaScriptSerializer().Deserialize<GoogleProfileModel>(readerRead);

            // Google people api web request
            var _url = $"https://people.googleapis.com/v1/people/{profile.id}?personFields=phoneNumbers,emailAddresses";
            var restClient = new RestClient(_url);
            var _request = new RestRequest() { Method = Method.Get };
            _request.AddParameter("Authorization", $"Bearer {tokenResponse.access_token}", ParameterType.HttpHeader);
            var _response = restClient.Execute(_request, Method.Get);
            var people_profile = new JavaScriptSerializer().Deserialize<GooglePeopleApiModel>(_response.Content);

            string phone = null;

            if (_response.StatusCode == HttpStatusCode.OK && people_profile.phoneNumbers.IsNotNull())
            {
                var _phone = people_profile.phoneNumbers.FirstOrDefault()?.value;

                if (_phone.IsValidNumber())
                {
                    phone = _phone;
                }
            }

            var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");
            var result = work.Account.CombineExternalRegistration(new GivingRegisterModel
            {
                ChurchId = givingAmount.IsNotNull() ? givingAmount.ChurchId : null,
                Email = profile.email,
                FirstName = profile.given_name,
                LastName = profile.family_name,
                Id = Utilities.GenerateUniqueId(),
                LoginProvider = LoginProvider.Google,
                Password = LoginProvider.Google,
                Phone = phone,
                ProfileImage = profile.picture,
                Username = profile.email,
                ExternalProviderId = profile.id
            });

            if (result.ResultType != ResultType.Success) return RedirectToAction("SignIn", "Account");

            if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

            if (givingAmount.IsNotNull())
            {
                return RedirectToAction("Index", "MyGiving");
            }

            var churchId = work.UserSetting.GetByUserId(SessionVariables.CurrentUser.User.Id).PrimaryChurchId;

            if (churchId == "unregister")
            {
                return RedirectToAction("ChurchWelcome", "Onboarding");
            }

            return Redirect("/");
        }
        #endregion

        #endregion

        #region Users With Multiple Churches
        [HttpGet]
        public ActionResult SelectChurch()
        {
            var userChurches = work.Church.GetAllChurchUsersByUserId(User.Identity.GetUserId());
            var churches = work.Church.Get(userChurches.Select(x => x.ChurchId).ToList());
            return View(churches);
        }

        [HttpGet]
        public ActionResult SelectedChurch(string id)
        {
            var user = work.User.Get(User.Identity.GetUserId());
            Utilities.SetSessionVariables(user, id);

            if (!user.AssignedToChurch)
            {
                return RedirectToAction("churchwelcome", "onboarding");
            }

            if (!SessionVariables.CurrentUser.MemberOf(Roles.Donor)) return Redirect("/");

            var givingAmount = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

            if (givingAmount.IsNotNull())
            {
                return RedirectToAction("StartGiving", "Giving", new { SessionVariables.CurrentChurch.Id });
            }

            return Redirect("/");
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };

                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}