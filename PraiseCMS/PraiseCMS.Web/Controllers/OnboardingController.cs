using Microsoft.AspNet.Identity;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
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
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    public class OnboardingController : BaseController
    {
        public ActionResult ChurchWelcome(string plan)
        {
            var model = new ChurchOnboardingView
            {
                Denominations = work.Denomination.GetAll(),
                Church = new Church()
                {
                    Id = Utilities.GenerateUniqueId(),
                    PrimaryUserId = SessionVariables.CurrentUser.User.Id
                }
            };

            plan ??= PlanType.Premium;

            model.Plan = plan;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChurchWelcome(ChurchOnboardingView model)
        {
            try
            {
                // Check if the church already exists
                if (work.Church.ChurchExists(model.Church))
                {
                    return Json(new ResponseModel() { Success = false, Message = "An account for this church has been created previously. Please sign in or contact us for further information." });
                }

                // Set default values for the church
                InitializeDefaultChurchValues(model.Church);

                // Set location information for the church
                await SetChurchLocationInfoAsync(model.Church);

                // Create the church and set it as the current church in the session
                var churchResult = work.Church.Create(model.Church);
                SessionVariables.CurrentChurch = model.Church;

                // Check if church creation was successful
                if (churchResult.ResultType == ResultType.Success)
                {
                    // Create a campus for the church
                    var campus = CreateChurchCampus(model.Church);
                    // Add the new campus to the campuses session variable
                    SessionVariables.Campuses = new List<Campus> { campus };

                    // Update user settings with primary church information
                    UpdateUserSettingsForChurch(model.Church, campus);

                    // Create a record in the ChurchPeople table
                    var createdPerson = CreateChurchPersonForUser(SessionVariables.CurrentUser.User, model.Church);
                    // Add the current admin to the ChurchUser table
                    AddUserToChurch(SessionVariables.CurrentUser.User, model.Church);

                    // Subscription info for the church
                    await CreateChurchDonorAccount(model.Church);

                    CreateDefaultFunds(model.Church);

                    // Add entry in Subscription table as per the user plan selection
                    string freeTrialMessage = CreateSubscriptionForChurch(model.Church, model.Plan);

                    var domain = SessionVariables.CurrentDomain;
                    var baseUrl = domain != null ? URLPrefixes.Https + domain.BaseUrl : ApplicationCache.Instance.SiteConfiguration.Url;

                    #region Second Admin
                    /************************************************
                     BEGIN: Second Admin
                     ************************************************/
                    var secondAdmin = new Person();

                    //Create a new Admin user from Step 3 (Admin Details) in addition to user that registered.
                    if (!string.IsNullOrEmpty(model.AdminUserEmail) || !string.IsNullOrEmpty(model.AdminUserPhone))
                    {
                        secondAdmin.FirstName = !string.IsNullOrEmpty(model.AdminUserFirstname) ? model.AdminUserFirstname : string.Empty;
                        secondAdmin.LastName = !string.IsNullOrEmpty(model.AdminUserLastname) ? model.AdminUserLastname : string.Empty;
                        secondAdmin.Email = !string.IsNullOrEmpty(model.AdminUserEmail) ? model.AdminUserEmail : string.Empty;
                        secondAdmin.PhoneNumber = !string.IsNullOrEmpty(model.AdminUserPhone) ? model.AdminUserPhone : string.Empty;

                        //Check if the second admin PERSON exists
                        var secondAdminPerson = work.Person.GetByEmailAndPhone(secondAdmin.Email, secondAdmin.PhoneNumber);

                        if (secondAdminPerson.IsNullOrEmpty())
                        {
                            secondAdminPerson = new Person()
                            {
                                Id = Utilities.GenerateUniqueId(),
                                CreatedDate = DateTime.Now,
                                FirstName = secondAdmin.FirstName,
                                LastName = secondAdmin.LastName,
                                Email = secondAdmin.Email,
                                PhoneNumber = secondAdmin.PhoneNumber,
                                IsActive = true
                            };
                            work.Person.Create(secondAdminPerson);
                            work.Person.CreateChurchPerson(new ChurchPerson
                            {
                                Id = Utilities.GenerateUniqueId(),
                                PersonId = secondAdminPerson.Id,
                                ChurchId = model.Church.Id,
                                CreatedBy = SessionVariables.CurrentUser.User.Id,
                                CreatedDate = DateTime.Now
                            });
                        }

                        //Check if the second admin USER already exists
                        var secondAdminUser = work.User.GetByEmailAndPhone(secondAdmin.Email, secondAdmin.PhoneNumber);

                        if (secondAdminUser == null)
                        {
                            secondAdminUser = new ApplicationUser();
                        }
                        else
                        {
                            var response = secondAdminUser.PhoneNumber.IsNotNullOrEmpty() && secondAdminUser.PhoneNumber == secondAdmin.PhoneNumber
                                ? Constants.PhoneAlreadyRegistered
                                : Constants.EmailAlreadyRegistered;
                            return Json(new ResponseModel() { Success = false, Message = response });
                        }

                        //Create the second admin USER
                        secondAdminUser.Id = Utilities.GenerateUniqueId();
                        secondAdminUser.FirstName = secondAdmin.FirstName;
                        secondAdminUser.LastName = secondAdmin.LastName;
                        secondAdminUser.Email = !string.IsNullOrEmpty(secondAdmin.Email) ? secondAdmin.Email : string.Empty;
                        secondAdminUser.PhoneNumber = !string.IsNullOrEmpty(secondAdmin.PhoneNumber) ? secondAdmin.PhoneNumber : string.Empty;
                        secondAdminUser.UserName = secondAdmin.Email;
                        secondAdminUser.EmailConfirmed = false;
                        secondAdminUser.PhoneNumberConfirmed = false;
                        secondAdminUser.TwoFactorEnabled = false;
                        secondAdminUser.TwoFactorEnabled = false;
                        secondAdminUser.LockoutEnabled = false;
                        secondAdminUser.AccessFailedCount = 0;
                        secondAdminUser.IsActive = true;
                        secondAdminUser.CreatedDate = DateTime.Now;
                        secondAdminUser.CreatedBy = SessionVariables.CurrentUser.User.Id;
                        secondAdminUser.ShowWelcomeMessage = true;
                        secondAdminUser.PersonId = createdPerson.Id;
                        secondAdminUser.ShowWelcomeMessage = true;
                        secondAdminUser.PhoneVerificationCode = Utilities.GenerateVerificationCode();

                        var result = UserManager.Create(secondAdminUser);  //Generating id for initial password

                        if (result.Succeeded)
                        {
                            adoData.InsertUserRoleByName(secondAdminUser.Id, Roles.Administrator);
                            work.UserSetting.Create(new UserSetting
                            {
                                Id = Utilities.GenerateUniqueId(),
                                UserId = secondAdminUser.Id,
                                PrimaryChurchId = model.Church.Id,
                                PrimaryChurchCampusId = model.Church.Id,
                                CreatedDate = DateTime.Now,
                                CreatedBy = SessionVariables.CurrentUser.User.Id
                            });
                            work.Church.CreateUser(new ChurchUser
                            {
                                Id = Utilities.GenerateUniqueId(),
                                UserId = secondAdminUser.Id,
                                ChurchId = model.Church.Id,
                                CreatedDate = DateTime.Now,
                                CreatedBy = SessionVariables.CurrentUser.User.Id
                            });

                            secondAdminUser.AssignedToChurch = true;
                            work.User.Update(secondAdminUser);

                            //set new admin to the church's primary user
                            churchResult.Data.PrimaryUserId = secondAdminUser.Id;
                            churchResult.Data.ModifiedDate = DateTime.Now;
                            churchResult.Data.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                            work.Church.Update(churchResult.Data);

                            //Send the second admin the new user account email
                            if (!string.IsNullOrEmpty(secondAdmin.Email))
                            {
                                var newUserContent = EmailTemplates.NewUserAccount_body
                                 .Replace("{createdBy}", SessionVariables.CurrentUser.User.FullName)
                                 .Replace("{username}", secondAdminUser.UserName)
                                 .Replace("{btn_url}", $"{baseUrl}/users/setpassword?userid={secondAdminUser.Id}&token={UserManager.GenerateUserToken("SET_PASSWORD", secondAdminUser.Id)}");
                                var newUserEmail = new Email()
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    Message = newUserContent,
                                    To = secondAdminUser.Email,
                                    Subject = "New User Account",
                                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                                    CreatedDate = DateTime.Now
                                };
                                Emailer.SendEmail(newUserEmail);
                            }
                        }
                        else
                        {
                            var error = result.Errors.FirstOrDefault();

                            return Json(new ResponseModel() { Success = false, Message = error });
                        }
                    }
                    /************************************************
                         END: Second Admin
                         ************************************************/
                    #endregion

                    // Send registration email to the user
                    SendRegistrationEmail(SessionVariables.CurrentUser.User.Email, freeTrialMessage, baseUrl);

                    // Send notification email to Praise CMS
                    SendPraiseCmsNotification(model.Church, SessionVariables.CurrentUser.User, secondAdmin);

                    // Update user information and set the user as assigned to the church
                    UpdateUserForChurch(SessionVariables.CurrentUser.User);

                    return Json(new ResponseModel { Success = true });
                }

                // Log error if church creation failed
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", churchResult.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error - Church Onboarding", model.Church.Id, LogStatuses.Error, logObj);

                return Json(new ResponseModel() { Success = false, Message = churchResult.Message });
            }
            catch (Exception ex)
            {
                //ExceptionLogger.LogException(ex);
                // Log any unexpected exceptions
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", ex.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error - Church Onboarding", model.Church.Id, LogStatuses.Error, logObj);

                return Json(new ResponseModel() { Success = false, Message = "An unexpected error occurred during church onboarding." });
            }
        }

        private void InitializeDefaultChurchValues(Church church)
        {
            church.IsActive = true;
            church.ShowWelcomeMessage = true;
            church.PaperlessGiving = true;
            church.CreatedDate = DateTime.Now;
            church.CreatedBy = SessionVariables.CurrentUser.User.Id;

            // Default values
            church.GivingThankYouText = Constants.DefaultGivingThankYouText;
            church.AnnualStatementEmailBody = Constants.DefaultAnnualStatementEmailBody;
            church.AnnualStatementDisclaimer = Constants.DefaultAnnualStatementDisclaimer;
            church.PrayerRequestReceivedText = Constants.DefaultPrayerRequestReceivedText;
            church.PrayerRequestReceivedFollowUpText = Constants.DefaultPrayerRequestReceivedFollowUpText;
        }

        //private void SetChurchLocationInfo(Church church)
        //{
        //    var loc = GetLocations();
        //    church.Latitude = loc.Latitude;
        //    church.Longitude = loc.Longitude;
        //    church.IPAddress = Utilities.GetIP();
        //}

        private void UpdateUserForChurch(ApplicationUser user)
        {
            user.AssignedToChurch = true;
            work.User.Update(user);
        }

        public void CreateDefaultFunds(Church church)
        {
            work.Fund.CreateDefaultFunds(church.Id);
        }

        private Campus CreateChurchCampus(Church church)
        {
            var campusResult = work.Campus.Create(new Campus
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = church.Id,
                Name = church.Name,
                Phone = church.Phone,
                Email = church.Email,
                TimeZone = church.TimeZone,
                Address1 = church.PhysicalAddress1,
                Address2 = church.PhysicalAddress2,
                City = church.PhysicalCity,
                State = church.PhysicalState,
                Zip = church.PhysicalZip,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            });

            if (campusResult.ResultType == ResultType.Success)
            {
                return campusResult.Data;
            }
            else
            {
                // Handle the error or return null
                return null;
            }
        }

        private void UpdateUserSettingsForChurch(Church church, Campus campus)
        {
            SessionVariables.CurrentUser.Settings.PrimaryChurchId = church.Id;
            SessionVariables.CurrentUser.Settings.PrimaryChurchCampusId = campus.Id;
            SessionVariables.CurrentUser.Settings.ModifiedDate = DateTime.Now;
            SessionVariables.CurrentUser.Settings.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            work.UserSetting.Update(SessionVariables.CurrentUser.Settings);
        }

        private Person CreateChurchPersonForUser(ApplicationUser user, Church church)
        {
            var person = work.Person.GetByUserId(user.Id);

            if (person.IsNotNullOrEmpty())
            {
                work.Person.CreateChurchPerson(new ChurchPerson
                {
                    Id = Utilities.GenerateUniqueId(),
                    PersonId = person.Id,
                    ChurchId = church.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                });

                return person;
            }

            return null; // or throw an exception or handle accordingly
        }

        private void AddUserToChurch(ApplicationUser user, Church church)
        {
            work.Church.CreateUser(new ChurchUser
            {
                Id = Utilities.GenerateUniqueId(),
                UserId = user.Id,
                ChurchId = church.Id,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            });
        }

        private async Task CreateChurchDonorAccount(Church church)
        {
            try
            {
                SessionVariables.CurrentChurch = await work.Giving.CreateChurchDonorAccount(church);
            }
            catch (Exception ex)
            {
                //ExceptionLogger.LogException(ex);
                var obj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", ex.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error - Create Church Donor Account", church.Id, LogStatuses.Error, obj);
            }
        }

        private string CreateSubscriptionForChurch(Church church, string plan)
        {
            string freeTrialMessage;

            var planTypes = work.Subscription.GetAllSubscriptionTypes();
            var subscription = new Subscription()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = church.Id,
                PlanTypeId = planTypes.FirstOrDefault(q => q.Name.ContainsIgnoreCase(plan.ToLower()))?.Id,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now,
                FreeTrial = false,
                StartDate = DateTime.Now,
                BillingPlan = BillingType.Free,
                IsActive = true
            };

            if (plan.EqualsIgnoreCase(PlanType.Premium.ToLower()))
            {
                subscription.BillingPlan = BillingType.Monthly;
                subscription.PlanTypeId = planTypes.FirstOrDefault(q => q.Name.ContainsIgnoreCase(plan.ToLower()))?.Id;
                subscription.FreeTrial = true;
                var freeTrialDays = Utilities.GetFreeTrialDays().ToInt32();
                subscription.EndDate = DateTime.Now.AddDays(freeTrialDays);

                freeTrialMessage = $"With your {freeTrialDays}-day free trial, you can receive digital gifts, create events, manage prayer requests, and much more. Be sure to add a payment method before your free trial ends so you don't lose out on these great services.";
            }
            else
            {
                freeTrialMessage = $"Your free plan started on {DateTime.Now:MMMM dd, yyyy}. Upgrade your plan at any time to access all of Praise's features.";
            }

            work.Subscription.Create(subscription);

            return freeTrialMessage;
        }

        private Person HandleSecondAdminCreation(Church church, string personId, string adminEmail, string adminPhone, string adminFirstName, string adminLastName, string baseUrl)
        {
            var secondAdmin = new Person
            {
                FirstName = !string.IsNullOrEmpty(adminFirstName) ? adminFirstName : string.Empty,
                LastName = !string.IsNullOrEmpty(adminLastName) ? adminLastName : string.Empty,
                Email = !string.IsNullOrEmpty(adminEmail) ? adminEmail : string.Empty,
                PhoneNumber = !string.IsNullOrEmpty(adminPhone) ? adminPhone : string.Empty
            };

            // Check if the second admin PERSON exists
            var secondAdminPerson = work.Person.GetByEmailAndPhone(secondAdmin.Email, secondAdmin.PhoneNumber);

            if (secondAdminPerson.IsNullOrEmpty())
            {
                secondAdminPerson = new Person()
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedDate = DateTime.Now,
                    FirstName = secondAdmin.FirstName,
                    LastName = secondAdmin.LastName,
                    Email = secondAdmin.Email,
                    PhoneNumber = secondAdmin.PhoneNumber,
                    IsActive = true
                };
                work.Person.Create(secondAdminPerson);
                work.Person.CreateChurchPerson(new ChurchPerson
                {
                    Id = Utilities.GenerateUniqueId(),
                    PersonId = secondAdminPerson.Id,
                    ChurchId = church.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                });
            }

            // Check if the second admin USER already exists
            var secondAdminUser = work.User.GetByEmailAndPhone(secondAdmin.Email, secondAdmin.PhoneNumber);

            if (secondAdminUser == null)
            {
                secondAdminUser = new ApplicationUser();
            }
            else
            {
                var response = secondAdminUser.PhoneNumber.IsNotNullOrEmpty() && secondAdminUser.PhoneNumber == secondAdmin.PhoneNumber
                    ? Constants.PhoneAlreadyRegistered
                    : Constants.EmailAlreadyRegistered;
                // Handle the error scenario if needed
                return null;
            }

            // Create the second admin USER
            secondAdminUser.Id = Utilities.GenerateUniqueId();
            secondAdminUser.FirstName = secondAdmin.FirstName;
            secondAdminUser.LastName = secondAdmin.LastName;
            secondAdminUser.Email = !string.IsNullOrEmpty(secondAdmin.Email) ? secondAdmin.Email : string.Empty;
            secondAdminUser.PhoneNumber = !string.IsNullOrEmpty(secondAdmin.PhoneNumber) ? secondAdmin.PhoneNumber : string.Empty;
            secondAdminUser.UserName = secondAdmin.Email;
            secondAdminUser.EmailConfirmed = false;
            secondAdminUser.PhoneNumberConfirmed = false;
            secondAdminUser.TwoFactorEnabled = false;
            secondAdminUser.TwoFactorEnabled = false;
            secondAdminUser.LockoutEnabled = false;
            secondAdminUser.AccessFailedCount = 0;
            secondAdminUser.IsActive = true;
            secondAdminUser.CreatedDate = DateTime.Now;
            secondAdminUser.CreatedBy = SessionVariables.CurrentUser.User.Id;
            secondAdminUser.ShowWelcomeMessage = true;
            secondAdminUser.PersonId = personId;
            secondAdminUser.ShowWelcomeMessage = true;
            secondAdminUser.PhoneVerificationCode = Utilities.GenerateVerificationCode();

            var result = UserManager.Create(secondAdminUser);

            if (result.Succeeded)
            {
                adoData.InsertUserRoleByName(secondAdminUser.Id, Roles.Administrator);
                work.UserSetting.Create(new UserSetting
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = secondAdminUser.Id,
                    PrimaryChurchId = church.Id,
                    PrimaryChurchCampusId = church.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                });
                work.Church.CreateUser(new ChurchUser
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = secondAdminUser.Id,
                    ChurchId = church.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                });

                secondAdminUser.AssignedToChurch = true;
                work.User.Update(secondAdminUser);

                // Set new admin to the church's primary user
                church.PrimaryUserId = secondAdminUser.Id;
                church.ModifiedDate = DateTime.Now;
                church.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.Church.Update(church);

                // Send the second admin the new user account email
                if (!string.IsNullOrEmpty(secondAdmin.Email))
                {
                    var newUserContent = EmailTemplates.NewUserAccount_body
                     .Replace("{createdBy}", SessionVariables.CurrentUser.User.FullName)
                     .Replace("{username}", secondAdminUser.UserName)
                     .Replace("{btn_url}", $"{baseUrl}/users/setpassword?userid={secondAdminUser.Id}&token={UserManager.GenerateUserToken("SET_PASSWORD", secondAdminUser.Id)}");
                    var newUserEmail = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = newUserContent,
                        To = secondAdminUser.Email,
                        Subject = "New User Account",
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    };
                    Emailer.SendEmail(newUserEmail);
                }

                // Return the created Person object
                return secondAdminPerson;
            }
            else
            {
                var error = result.Errors.FirstOrDefault();

                // Handle the error scenario if needed
                return null;
            }
        }

        private void SendRegistrationEmail(string userEmail, string freeTrialMessage, string baseUrl)
        {
            var btnUrl = baseUrl + "/Account/Login";

            var content = EmailTemplates.ChurchRegistration_body
                .Replace("{message}", freeTrialMessage)
                .Replace("{btn_url}", btnUrl);
            var email = new Email()
            {
                Id = Utilities.GenerateUniqueId(),
                Message = content,
                To = userEmail,
                Subject = "Church Registration",
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };
            Emailer.SendEmail(email);
        }

        private void SendPraiseCmsNotification(Church church, ApplicationUser currentUser, Person secondAdmin)
        {
            var churchAdmin = $"<a href='mailto:{currentUser.Email}'>{currentUser.Display}</a>";

            // Get the second admin details and include in email to Praise CMS
            if (!string.IsNullOrEmpty(secondAdmin.Email))
            {
                churchAdmin += $", <a href='mailto:{secondAdmin.Email}'>{secondAdmin.Display}</a>";
            }

            // Email Praise CMS about the new church signing up
            var praiseEmailContent = EmailTemplates.ChurchRegistrationSuperAdmin_body;
            praiseEmailContent = praiseEmailContent.Replace("{churchName}", church.Display)
                .Replace("{phone}", church.Phone)
                .Replace("{email}", church.Email)
                .Replace("{church-address}", church.PhysicalAddress)
                .Replace("{created_datetime}", church.CreatedDate.ToShortDateString())
                .Replace("{church-admin}", churchAdmin);

            var praiseAdminEmail = new Email()
            {
                Id = Utilities.GenerateUniqueId(),
                Message = praiseEmailContent,
                To = ConfigurationManager.AppSettings["SuperAdminEmail"],
                Subject = "New Church Registration",
                CreatedBy = currentUser.Id,
                CreatedDate = DateTime.Now
            };
            Emailer.SendEmail(praiseAdminEmail);
        }

        public ActionResult CreateMerchantAccount(string id)
        {
            var church = work.Church.Get(id);
            var account = work.ChurchMerchantAccount.GetByChurchId(id) ?? new ChurchMerchantAccount();

            var model = new ChurchMerchantAccountVM
            {
                Church = church,
                Account = account
            };

            if (AreAllFieldsFilled(church, account))
            {
                const string button = "<button id='enable-giving-btn' type='button' class='btn btn-primary font-weight-bolder ml-2'>Enable Giving</button>";
                var message = string.Format(Constants.EnableGivingMessage, button);
                CreateAlertMessage(message, AlertMessageTypes.Primary, AlertMessageIcons.Primary);
            }
            else if (IsAnyFieldFilled(church, account))
            {
                var message = string.Format(Constants.EnableGivingMessage, string.Empty);
                CreateAlertMessage(message, AlertMessageTypes.Primary, AlertMessageIcons.Primary);
            }

            return View(model);
        }

        private bool AreAllFieldsFilled(Church church, ChurchMerchantAccount account)
        {
            return church != null && account != null
                && !string.IsNullOrEmpty(church.LegalName)
                && !string.IsNullOrEmpty(church.TaxIdNumber)
                && !string.IsNullOrEmpty(church.PhysicalAddress1)
                && !string.IsNullOrEmpty(church.PhysicalAddress2)
                && !string.IsNullOrEmpty(church.PhysicalCity)
                && !string.IsNullOrEmpty(church.PhysicalState)
                && !string.IsNullOrEmpty(church.PhysicalZip)
                && !string.IsNullOrEmpty(church.Email)
                && !string.IsNullOrEmpty(church.Website)
                && !string.IsNullOrEmpty(account.AccountNumber)
                && !string.IsNullOrEmpty(account.RoutingNumber)
                && !string.IsNullOrEmpty(account.BankAccountType);
        }

        private bool IsAnyFieldFilled(Church church, ChurchMerchantAccount account)
        {
            return (church != null && (
                        !string.IsNullOrEmpty(church.LegalName) || !string.IsNullOrEmpty(church.TaxIdNumber)
                        || !string.IsNullOrEmpty(church.PhysicalAddress1) || !string.IsNullOrEmpty(church.PhysicalAddress2)
                        || !string.IsNullOrEmpty(church.PhysicalCity) || !string.IsNullOrEmpty(church.PhysicalState)
                        || !string.IsNullOrEmpty(church.PhysicalZip) || !string.IsNullOrEmpty(church.Email)
                        || !string.IsNullOrEmpty(church.Website)
                    ))
                || (account != null && (
                        !string.IsNullOrEmpty(account.AccountNumber) || !string.IsNullOrEmpty(account.RoutingNumber)
                        || !string.IsNullOrEmpty(account.BankAccountType)
                    ));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateMerchantAccount(ChurchMerchantAccountVM model)
        {
            try
            {
                var termsAndConditionsResponseData = await nuveiHelper.GetTermsAndConditionsAsync();

                model.CorrelationId = termsAndConditionsResponseData.correlationId;
                model.TermsAndConditionsId = termsAndConditionsResponseData.termsAndConditionsId;
                model.TermsAndConditionsUrl = termsAndConditionsResponseData.termsAndConditionsURL;

                bool success = await nuveiHelper.AcceptTermsAndConditionsAsync(model.CorrelationId, model.TermsAndConditionsId);

                if (!success) return Json(new { Success = false, Data = model });

                var leadModel = work.Giving.MapToLeadRequestModel(model);

                var leadApiResponse = await nuveiHelper.CreateLeadAsync(model.CorrelationId, leadModel);

                if (leadApiResponse.result == APIStatuses.Success)
                {
                    model.Account.MerchantAccountId = leadApiResponse.merchant_key;
                    model.Account.Merchant = MerchantProviders.Nuvei;
                    model.Account.ChurchId = SessionVariables.CurrentChurch.Id;
                    model.Account.TaxId = model.Church.TaxIdNumber;
                    model.Account.BusinessWebsite = model.Church.Website;
                    model.Account.IsActive = true;
                    model.Account.CreatedBy = SessionVariables.CurrentUser.User.Id;
                    model.Account.CreatedDate = DateTime.Now;
                    model.Account.AccountNumber = model.Account.AccountNumber.Encrypt();
                    model.Account.RoutingNumber = model.Account.RoutingNumber.Encrypt();
                    model.Account.IsActive = true;
                    model.Account.CardProcessingFee = "DefaultCardProcessingFee".AppSetting(2.99m);
                    model.Account.ACHProcessingFee = "DefaultACHProcessingFee".AppSetting(0.75m);
                    model.Account.ApiUsername = leadApiResponse.api_username.Encrypt();
                    model.Account.ApiPassword = leadApiResponse.api_password.Encrypt();
                    model.Account.Username = leadApiResponse.username.Encrypt();
                    model.Account.Password = leadApiResponse.password.Encrypt();
                    model.Account.CorrelationId = termsAndConditionsResponseData.correlationId;
                    model.Account.TermsAndConditionsId = termsAndConditionsResponseData.termsAndConditionsId;
                    model.Account.TermsAndConditionsUrl = termsAndConditionsResponseData.termsAndConditionsURL;
                    model.Account.RespContactSSN = model.Account.RespContactSSN.Encrypt();

                    SessionVariables.CurrentChurch.HasMerchantAccount = true;
                    SessionVariables.CurrentChurch.TaxIdNumber = model.Church.TaxIdNumber;
                    SessionVariables.CurrentChurch.Website = model.Church.Website;
                    SessionVariables.CurrentChurch.Email = model.Church.Email;
                    SessionVariables.CurrentChurch.LegalName = model.Church.LegalName;
                    SessionVariables.CurrentChurch.PhysicalAddress1 = model.Church.PhysicalAddress1;
                    SessionVariables.CurrentChurch.PhysicalAddress2 = model.Church.PhysicalAddress2;
                    SessionVariables.CurrentChurch.PhysicalCity = model.Church.PhysicalCity;
                    SessionVariables.CurrentChurch.PhysicalState = model.Church.PhysicalState;
                    SessionVariables.CurrentChurch.PhysicalZip = model.Church.PhysicalZip;
                    SessionVariables.CurrentChurch.ModifiedDate = DateTime.Now;
                    SessionVariables.CurrentChurch.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.Church.Update(SessionVariables.CurrentChurch);

                    if (model.Account.Id.IsNotNullOrEmpty())
                    {
                        model.Account.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        model.Account.ModifiedDate = DateTime.Now;
                        work.ChurchMerchantAccount.Update(model.Account);
                    }
                    else
                    {
                        model.Account.Id = Utilities.GenerateUniqueId();
                        work.ChurchMerchantAccount.Create(model.Account);
                    }

                    SessionVariables.SetCurrentMerchant(model.Church.Id);
                    CreateAlertMessage("<strong>Great News!</strong> Digital giving has been enabled for your account. <a href='/Settings'>Add a personalized thank-you message that will display after each donation is made.</a>", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return Json(new { Success = true, Data = model });
                }

                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", leadApiResponse.message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error Occurred", SessionVariables.CurrentUser.User.Id, LogStatuses.Error, logObj);

                var churchName = !string.IsNullOrEmpty(model.Church.Name) ? model.Church.Name : model.Church.LegalName;
                var content = "<b>Church:</b> " + churchName + "<br><b>Phone:</b> " + model.Church.Phone + "<br><b>Email:</b> " + model.Church.Email + "<br><b>Responsible Contact Person Name:</b> " + model.Account.RespContactDisplay + "<br><b>Error:</b> " + leadApiResponse.message;
                var message = EmailTemplates.Base.Replace("{body_content}", content);
                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = message,
                    To = ConfigurationManager.AppSettings["SupportEmail"],
                    Attachments = null,
                    Subject = "Error - Church Merchant Onboarding",
                    CreatedBy = SessionVariables.CurrentUser?.User.Id != null ? SessionVariables.CurrentUser.User.Id : string.Empty,
                    CreatedDate = DateTime.Now
                };
                Emailer.SendEmail(email);

                return Json(new ResponseModel() { Success = false, Message = leadApiResponse.message });
            }
            catch (Exception ex)
            {
                //ExceptionLogger.LogException(ex);
                CreateAlertMessage($"Something went wrong while creating the church merchant account, Error: {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", ex.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error Occurred", SessionVariables.CurrentUser.User.Id, LogStatuses.Error, logObj);

                var churchName = !string.IsNullOrEmpty(model.Church.Name) ? model.Church.Name : model.Church.LegalName;
                var content = "<b>Church:</b> " + churchName + "<br><b>Phone:</b> " + model.Church.Phone + "<br><b>Email:</b> " + model.Church.Email + "<br><b>Responsible Contact Person Name:</b> " + model.Account.RespContactDisplay + "<br><b>Error:</b> " + ex.Message;
                var message = EmailTemplates.Base.Replace("{body_content}", content);
                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = message,
                    To = ConfigurationManager.AppSettings["SupportEmail"],
                    Attachments = null,
                    Subject = "Error - Church Merchant Onboarding",
                    CreatedBy = SessionVariables.CurrentUser?.User.Id != null ? SessionVariables.CurrentUser.User.Id : string.Empty,
                    CreatedDate = DateTime.Now
                };
                Emailer.SendEmail(email);

                return Json(new ResponseModel() { Success = false, Message = ex.Message });
            }
        }
    }
}