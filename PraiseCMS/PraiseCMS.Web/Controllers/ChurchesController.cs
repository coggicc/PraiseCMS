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
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    //[RequirePermission(ModuleId = "3937285510a9601de03c6440228bc1")]
    public class ChurchesController : BaseController
    {
        public ActionResult RegisterChurch()
        {
            var model = new ChurchOnboardingView
            {
                Denominations = work.Denomination.GetAll(),
                Church = new Church()
                {
                    Id = Utilities.GenerateUniqueId(),
                    PrimaryUserId = SessionVariables.CurrentUser.User.Id
                },
                Plan = PlanType.Premium
            };

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterChurch(ChurchOnboardingView model)
        {
            if (ModelState.IsValid)
            {
                if (work.Church.GetAll().Any(q => q.Name.Equals(model.Church.Name) && q.PhysicalCity.Equals(model.Church.PhysicalCity)
                                                                          && q.PhysicalState.Equals(model.Church.PhysicalState) && q.PhysicalAddress1.Equals(model.Church.PhysicalAddress1)
                                                                          && q.PhysicalZip.Equals(model.Church.PhysicalZip)))
                {
                    CreateAlertMessage("This church already has an account.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                    return View(model);
                }

                model.Church.IsActive = true;
                model.Church.ShowWelcomeMessage = true;
                model.Church.PaperlessGiving = true;
                model.Church.CreatedDate = DateTime.Now;
                model.Church.CreatedBy = SessionVariables.CurrentUser.User.Id;

                await SetChurchLocationInfoAsync(model.Church);

                var churchResult = work.Church.Create(model.Church);

                if (churchResult.ResultType == ResultType.Success)
                {
                    var domain = SessionVariables.CurrentDomain;
                    var baseUrl = domain != null ? URLPrefixes.Https + domain.BaseUrl : ApplicationCache.Instance.SiteConfiguration.Url;

                    //create Donor account of church for subscription payments
                    var donorResult = await work.Giving.CreateChurchDonorAccount(model.Church);

                    var campus = work.Campus.Create(new Campus
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = model.Church.Id,
                        Name = model.Church.Name,
                        Phone = model.Church.Phone,
                        Email = model.Church.Email,
                        TimeZone = model.Church.TimeZone,
                        Address1 = model.Church.PhysicalAddress1,
                        Address2 = model.Church.PhysicalAddress2,
                        City = model.Church.PhysicalCity,
                        State = model.Church.PhysicalState,
                        Zip = model.Church.PhysicalZip,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = SessionVariables.CurrentUser.User.Id
                    });

                    var adminUser = new ApplicationUser
                    {
                        FirstName = !string.IsNullOrEmpty(model.AdminUserEmail) ? model.AdminUserFirstname : string.Empty,
                        LastName = !string.IsNullOrEmpty(model.AdminUserEmail) ? model.AdminUserLastname : string.Empty,
                        Email = !string.IsNullOrEmpty(model.AdminUserEmail) ? model.AdminUserEmail : string.Empty,
                        PhoneNumber = !string.IsNullOrEmpty(model.AdminUserPhone) ? model.AdminUserPhone : string.Empty
                    };

                    if (!string.IsNullOrEmpty(adminUser.Email))
                    {
                        //Verify the new admin user is not already in the system via email & phone
                        var usr = work.User.GetByEmailPlain(adminUser.Email);

                        if (usr.IsNotNull())
                        {
                            var userSetting = work.UserSetting.GetByUserId(usr.Id);

                            if (userSetting.IsNotNullOrEmpty())
                            {
                                userSetting.PrimaryChurchId = model.Church.Id;
                                userSetting.PrimaryChurchCampusId = campus.Data.Id;
                                userSetting.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                                userSetting.ModifiedDate = DateTime.Now;
                            }
                            else
                            {
                                work.UserSetting.Create(new UserSetting
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    UserId = adminUser.Id,
                                    PrimaryChurchId = model.Church.Id,
                                    PrimaryChurchCampusId = campus.Data.Id,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = SessionVariables.CurrentUser.User.Id
                                });
                            }
                            work.Church.CreateUser(new ChurchUser
                            {
                                Id = Utilities.GenerateUniqueId(),
                                UserId = adminUser.Id,
                                ChurchId = model.Church.Id,
                                CreatedDate = DateTime.Now,
                                CreatedBy = SessionVariables.CurrentUser.User.Id
                            });

                            //set new admin to the church's primary user
                            churchResult.Data.PrimaryUserId = usr.Id;
                            churchResult.Data.ModifiedDate = DateTime.Now;
                            churchResult.Data.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                            work.Church.Update(churchResult.Data);
                        }
                        else
                        {
                            adminUser.Id = Utilities.GenerateUniqueId();
                            adminUser.UserName = adminUser.Email;
                            adminUser.EmailConfirmed = false;
                            adminUser.PhoneNumberConfirmed = false;
                            adminUser.TwoFactorEnabled = false;
                            adminUser.TwoFactorEnabled = false;
                            adminUser.LockoutEnabled = false;
                            adminUser.AccessFailedCount = 0;
                            adminUser.IsActive = true;
                            adminUser.CreatedDate = DateTime.Now;
                            adminUser.CreatedBy = SessionVariables.CurrentUser.User.Id;
                            adminUser.ShowWelcomeMessage = true;
                            adminUser.PhoneVerificationCode = Utilities.GenerateVerificationCode();

                            var result = UserManager.Create(adminUser);  //Generating id for initial password

                            if (result.Succeeded)
                            {
                                adoData.InsertUserRoleByName(adminUser.Id, Roles.Administrator);
                                work.UserSetting.Create(new UserSetting
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    UserId = adminUser.Id,
                                    PrimaryChurchId = model.Church.Id,
                                    PrimaryChurchCampusId = campus.Data.Id,  //Should we create a campus for when a church only has 1 location and address is the same as church?
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = SessionVariables.CurrentUser.User.Id
                                });
                                work.Church.CreateUser(new ChurchUser
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    UserId = adminUser.Id,
                                    ChurchId = model.Church.Id,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = SessionVariables.CurrentUser.User.Id
                                });

                                adminUser.AssignedToChurch = true;
                                work.User.Update(adminUser);

                                //set new admin to the church's primary user
                                churchResult.Data.PrimaryUserId = adminUser.Id;
                                churchResult.Data.ModifiedDate = DateTime.Now;
                                churchResult.Data.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                                work.Church.Update(churchResult.Data);

                                var logObj = logRepository.JsonConverter("Email", adminUser.Email, "First Name", adminUser.FirstName, "Last Name", adminUser.LastName, "Phone", adminUser.PhoneNumber);
                                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "New Administrator", adminUser.Id, LogStatuses.Done, logObj);

                                var content = EmailTemplates.NewUserAccount_body.Replace("{createdBy}", SessionVariables.CurrentUser.User.FullName)
                                .Replace("{username}", adminUser.UserName)
                                .Replace("{btn_url}", $"{baseUrl}/users/setpassword?userid={adminUser.Id}&token={UserManager.GenerateUserToken("SET_PASSWORD", adminUser.Id)}");
                                var emailObj = new Email()
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    Message = content,
                                    To = adminUser.Email,
                                    Subject = "Account Setup",
                                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                                    CreatedDate = DateTime.Now
                                };
                                Emailer.SendEmail(emailObj);
                            }
                        }
                    }

                    //Add entry in Subscription table as per the user plan selection
                    var planTypes = work.Subscription.GetAllSubscriptionTypes();
                    var freeTrialMessage = string.Empty;

                    if (planTypes != null)
                    {
                        var subscription = new Subscription()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            ChurchId = model.Church.Id,
                            PlanTypeId = planTypes.FirstOrDefault(q => q.Name.ContainsIgnoreCase(model.Plan.ToLower()))?.Id,
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            FreeTrial = false,
                            StartDate = DateTime.Now,
                            BillingPlan = BillingType.Free,
                            IsActive = true
                        };

                        //If the user selects a premium plan, then start a free trial for one month from today
                        if (planTypes.FirstOrDefault(q => q.Name.ContainsIgnoreCase(model.Plan.ToLower())).Display.Equals(PlanType.Premium, StringComparison.OrdinalIgnoreCase))
                        {
                            subscription.BillingPlan = BillingType.Monthly;
                            subscription.PlanTypeId = planTypes.FirstOrDefault(q => q.Name.ContainsIgnoreCase(model.Plan.ToLower()))?.Id;
                            subscription.FreeTrial = true;

                            //Find the free trial days from AppSettings, if not found default will be 30 days
                            int freeTrialDays = Utilities.GetFreeTrialDays().ToInt32();
                            subscription.EndDate = DateTime.Now.AddDays(freeTrialDays);

                            freeTrialMessage = string.Format(SubscriptionNotificationMessages.FreeTrialStartedAndExpireOn, freeTrialDays, subscription.EndDate.ToShortDateString());
                        }
                        else
                        {
                            freeTrialMessage = string.Format(SubscriptionNotificationMessages.FreeTrialStarted, DateTime.Now.ToShortDateString());
                        }
                        work.Subscription.Create(subscription);
                    }

                    //Email the new user 
                    var btnUrl = baseUrl + "/Account/Login";
                    var body = EmailTemplates.ChurchRegistration_body
                   .Replace("{message}", freeTrialMessage)
                   .Replace("{btn_url}", btnUrl);
                    var email = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = body,
                        To = adminUser.Email,
                        Subject = "Church Registration",
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    };
                    Emailer.SendEmail(email);

                    CreateAlertMessage("The church has been registered.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    CreateAlertMessage(churchResult.Exception.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", churchResult.Message);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error - Church Onboarding", model.Church.Id, LogStatuses.Error, logObj);

                    return View(model);
                }
            }

            return View(model);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult Index(string id = null)
        {
            var dashboard = work.Church.GetDashboard(id);
            return View(dashboard);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult Details(string id)
        {
            var church = work.Church.Get(id);
            return View(church);
        }
    }
}