using ExcelDataReader;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    public class UsersController : BaseController
    {
        #region Boilerplate

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
        #endregion        

        [RequirePermission(ModuleId = "6190518429afaf0ccebdfe4ccabaf4")]
        public ActionResult Index(string type, string filterKeyword)
        {
            var model = new UsersViewModel();

            if (!string.IsNullOrEmpty(type))
            {
                var users = work.User.GetAllByChurchRole(SessionVariables.CurrentChurch.Id, type);
                model.UsersWithRoles = work.User.GetUsersWithRoles(users.Select(x => x.Id));
                ViewBag.Title = type + "s";
                ViewBag.Type = type;
            }
            else
            {
                model.UsersWithRoles = work.User.GetChurchUsersWithRoles(SessionVariables.CurrentChurch.Id);
                ViewBag.Title = "All Users";
            }

            if (!string.IsNullOrEmpty(filterKeyword))
            {
                ViewBag.userFilterKeyword = filterKeyword;
                filterKeyword = filterKeyword.ToLower().Trim();

                var filterList = new List<UsersWithRoles>();
                filterList.AddRange(model.UsersWithRoles.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.FullName) && x.FullName.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.UsersWithRoles.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.FirstName) && x.FirstName.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.UsersWithRoles.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.LastName) && x.LastName.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.UsersWithRoles.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.Email) && x.Email.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.UsersWithRoles.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.UserRoles) && x.UserRoles.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.UsersWithRoles.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.ContainsIgnoreCase(filterKeyword)));
                model.UsersWithRoles = filterList;
            }

            return View(model);
        }

        [RequirePermission(ModuleId = "6190518429afaf0ccebdfe4ccabaf4")]
        public ActionResult Create(string type, string typeId)
        {
            if (!string.IsNullOrEmpty(type))
            {
                var role = work.Role.GetByName(type);
                typeId = role.Id;
                type = role.Name;
            }

            var model = new CreateAccountViewModel
            {
                Type = type,
                TypeId = typeId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(ModuleId = "6190518429afaf0ccebdfe4ccabaf4")]
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = work.User.GetByEmailAndPhone(model.Email, model.Phone);

                if (user.IsNotNullOrEmpty())
                {
                    CreateAlertMessage(Constants.DefaultErrorMessage + " A user with the same email or phone number already exists. You cannot create a duplicate user.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                    return View(model);
                }

                var person = work.Person.GetByEmailAndPhone(model.Email, model.Phone);

                if (person.IsNullOrEmpty())
                {
                    person = new Person
                    {
                        Id = Utilities.GenerateUniqueId(),
                        CreatedDate = DateTime.Now,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        PhoneNumber = model.Phone,
                        IsActive = true
                    };
                    work.Person.Create(person);
                    work.Person.CreateChurchPerson(new ChurchPerson
                    {
                        Id = Utilities.GenerateUniqueId(),
                        PersonId = person.Id,
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    });
                }

                user = new ApplicationUser
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.Phone,
                    Zip = model.Zip,
                    State = model.State,
                    City = model.City,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    CreatedDate = DateTime.Now,
                    AssignedToChurch = true,
                    ShowWelcomeMessage = true,
                    IsActive = true,
                    PersonId = person.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                };
                var result = await UserManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    work.User.CreateSettings(user, model);

                    var logObj = logRepository.JsonConverter("Email", model.Email, "First Name", model.FirstName, "Last Name", model.LastName, "Password", model.Password, "Phone", model.Phone);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Create User", "-", LogStatuses.Done, logObj);

                    var domain = SessionVariables.CurrentDomain;
                    var baseUrl = domain.IsNotNullOrEmpty() && domain.BaseUrl.IsNotNullOrEmpty() ? URLPrefixes.Http + domain.BaseUrl : ApplicationCache.Instance.SiteConfiguration.Url;
                    var content = EmailTemplates.NewUserAccount_body.Replace("{createdBy}", SessionVariables.CurrentUser.User.FullName)
                    .Replace("{username}", user.UserName)
                    .Replace("{btn_url}", $"{baseUrl}/users/setpassword?userid={user.Id}&token={UserManager.GenerateUserToken("SET_PASSWORD", user.Id)}");
                    var email = new Email
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = content,
                        To = user.Email,
                        Subject = "Account Setup",
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    };
                    Emailer.SendEmail(email);
                    CreateAlertMessage("The user has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return RedirectToAction("Index", "Users");
                }

                var errorList = result.Errors.Aggregate(string.Empty, (current, item) => current + item + "<br>");

                CreateAlertMessage("Uh-oh! There was a problem creating the user.<br>" + errorList, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return View(model);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage(Constants.DefaultErrorMessage + ": " + ex, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                //throw ex;
                return View(model);
            }
        }

        public ActionResult CreateUser(string id)
        {
            ViewBag.Id = id;
            return PartialView("_UserRoles");
        }

        [HttpPost]
        public ActionResult CreateUser(string id, List<string> Roles)
        {
            if (Roles.IsNullOrEmpty() || !Roles.Any())
            {
                CreateAlertMessage("Please select at least one role.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                ViewBag.Id = id;

                return PartialView("_UserRoles");
            }

            var person = work.Person.Get(id);
            var personChurches = work.Person.GetAllByPersonId(id);

            if (person.Email.IsNullOrEmpty())
            {
                CreateAlertMessage("Please add an email address to the user's profile and try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                ViewBag.Id = id;

                return PartialView("_UserRoles");
            }

            try
            {
                var user = new ApplicationUser
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedDate = DateTime.Now,
                    AssignedToChurch = true,
                    ShowWelcomeMessage = true,
                    PersonId = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = person.Email.ToLower(),
                    UserName = person.Email.ToLower(),
                    PhoneNumber = person.PhoneNumber,
                    IsActive = person.IsActive,
                    Address1 = person.Address1,
                    Address2 = person.Address2,
                    City = person.City,
                    State = person.State,
                    Zip = person.Zip,
                    ConvertedToUserById = SessionVariables.CurrentUser.User.Id,
                    ConvertedToUserDate = DateTime.Now
                };
                var result = UserManager.Create(user);

                if (result.Succeeded)
                {
                    foreach (var role in Roles)
                    {
                        adoData.InsertUserRoleByName(user.Id, role);
                    }

                    //Add user to churchusers table
                    var churchUsers = personChurches.Select(x => new ChurchUser
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = user.Id,
                        ChurchId = x.ChurchId,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    });

                    work.Church.CreateUser(churchUsers);

                    work.UserSetting.Create(new UserSetting
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = user.Id,
                        CreatedBy = user.Id,
                        CreatedDate = DateTime.Now,
                        PrimaryChurchId = personChurches.Any() ? personChurches[0].ChurchId : null,
                        ProfileImage = person.ProfileImage,
                        PrimaryChurchCampusId = SessionVariables.CurrentCampus.IsNotNullOrEmpty() ? SessionVariables.CurrentCampus.Id : string.Empty
                    });

                    var logObj = logRepository.JsonConverter("Email", person.Email, "First Name", person.FirstName, "Last Name", person.LastName, "Phone", person.PhoneNumber);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Create User From Person", person.Id, LogStatuses.Done, logObj);

                    var domain = SessionVariables.CurrentDomain;
                    var baseUrl = domain.IsNotNullOrEmpty() && domain.BaseUrl.IsNotNullOrEmpty() ? URLPrefixes.Http + domain.BaseUrl : ApplicationCache.Instance.SiteConfiguration.Url;
                    var content = EmailTemplates.NewUserAccount_body.Replace("{createdBy}", SessionVariables.CurrentUser.User.FullName)
                    .Replace("{username}", user.UserName)
                    .Replace("{btn_url}", $"{baseUrl}/users/setpassword?userid={user.Id}&token={UserManager.GenerateUserToken("SET_PASSWORD", user.Id)}");
                    var email = new Email
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = content,
                        To = user.Email,
                        Subject = "Account Setup",
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    };
                    Emailer.SendEmail(email);
                    CreateAlertMessage("A user account has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return AjaxRedirectTo($"/users/userprofile?id={user.Id}");
                }

                var errors = string.Join(" <br> ", result.Errors.Distinct());
                CreateAlertMessage($"{Constants.DefaultErrorMessage} {errors}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return AjaxRedirectTo($"/users/userprofile?Id={id}&type=person");
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"{Constants.DefaultErrorMessage} {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return AjaxRedirectTo($"/users/userprofile?Id={id}&type=person");
            }
        }

        public ActionResult CommunicationGroup(string id)
        {
            ViewBag.Tab = "communication";
            var model = new UserView
            {
                Type = Constants.User,
                Groups = work.CommunicationGroup.GetByPerson(id),
                User = work.User.GetByPersonId(id),
                DonorStatus = work.Giving.GetDonorStatus(id)
            };
            model.Settings = work.UserSetting.GetByUserId(model.User.Id);

            return View("UserCommunicationGroups", model);
        }

        public ActionResult UnsubscribeGroup(string groupId, string personId)
        {
            var result = work.CommunicationGroup.InactivePersonInGroup(groupId, personId);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage("You have been removed from the communication group.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage($"{result.Message}. Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(CommunicationGroup), new { id = personId });
        }

        public ActionResult ChangeUserStatus(string id)
        {
            if (id.IsNotNullOrEmpty())
            {
                try
                {
                    string msg = string.Empty;
                    string content = string.Empty;
                    string subject = string.Empty;
                    var user = work.User.Get(id: id);

                    if (user.IsNotNullOrEmpty())
                    {
                        msg = user.IsActive ? $"{user.Display}'s account has been canceled." : $"{user.Display}'s account has been activated.";

                        if (user.IsActive)
                        {
                            content = EmailTemplates.General_With_Heading.Replace("Hi {user_display},", "Your account has been canceled.")
                                              .Replace("{message}", $"Your Praise account was canceled on {DateTime.Now:MMMM dd, yyyy hh:mm tt}.");
                            subject = "Praise Account Canceled";
                        }
                        else
                        {
                            content = EmailTemplates.General_With_Heading.Replace("Hi {user_display},", "Your account has been activated.")
                                             .Replace("{message}", $"A Praise account was recently created for you. You will now be able to give and view contribution history for {SessionVariables.CurrentChurch.Display}.");
                            subject = "Praise Account Created";
                        }

                        user.IsActive = !user.IsActive;
                        work.User.Update(user);
                        var email = new Email
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Message = content,
                            To = user.Email,
                            Attachments = null,
                            Subject = subject,
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now
                        };

                        if (user.Email.IsNotNullOrEmpty())
                        {
                            Emailer.SendEmail(email);
                        }

                        return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    return Json(new { Success = false, ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Success = false, Message = "The requested user was not found." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [RequirePermission(ModuleId = "6190518429afaf0ccebdfe4ccabaf4")]
        public ActionResult Details(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Users");
            }

            var model = new UserView
            {
                User = UserManager.FindById(userId) ?? new ApplicationUser()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult AddRelationship(string personId)
        {
            var relationships = work.Person.GetAllRelatives(personId);
            ViewBag.PersonId = personId;
            TempData["people"] = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id).Where(x => !x.Id.Equals(personId)).ToList();

            return PartialView("_Relationships", relationships);
        }
        [HttpPost]
        public ActionResult AddRelationship(List<Relationship> model)
        {
            var relationships = model.Select(x => new Relationship
            {
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                Id = Utilities.GenerateUniqueId(),
                PersonId = x.PersonId,
                Relation = x.Relation,
                RelativePersonId = x.RelativePersonId
            }).ToList();

            var result = work.Person.AddUpdateRelationship(relationships);

            return Json(result);
        }

        [HttpGet]
        public ActionResult DeleteRelationships(string personId)
        {
            var result = work.Person.DeleteRelationships(personId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> UserProfile(string id, string type = "")
        {
            //If no userId is passed in, then show the current user's profile
            ViewBag.Tab = "personal-info";
            var model = new UserView();
            var currentUserRoles = new List<ApplicationRoles>();

            if (string.IsNullOrEmpty(id))
            {
                id = SessionVariables.CurrentUser.User.Id;
            }

            if (type.IsNotNullOrEmpty() && type.EqualsIgnoreCase(Constants.Person))
            {
                model.Person = work.Person.Get(id);
                model.Type = Constants.Person;
                ViewBag.hasUserAccount = false;
                var user = work.User.GetByPersonId(model.Person.Id);

                if (user.IsNotNullOrEmpty())
                {
                    ViewBag.hasUserAccount = true;
                }
            }
            else
            {
                model.Type = Constants.User;
                model.Person = work.Person.GetByUserId(id);
                model.HasPassword = HasPassword();
                model.PhoneNumber = await UserManager.GetPhoneNumberAsync(id);
                model.TwoFactor = await UserManager.GetTwoFactorEnabledAsync(id);
                model.Logins = await UserManager.GetLoginsAsync(id);
                model.BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(id);
                model.User = work.User.Get(id);
                model.User.Person = model.Person;
                model.Roles = GetAllRoles();
                currentUserRoles = adoData.ReadRolesByUserId(id).ToList();
                model.CurrentUserRoles = currentUserRoles.Select(x => x.Id).ToList();

                model.Settings = work.UserSetting.GetByUserId(id);

                if (model.Settings == null)
                {
                    model.Settings = new UserSetting
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = id,
                        PrimaryChurchId = SessionVariables.CurrentChurch.Id,
                        PrimaryChurchCampusId = SessionVariables.CurrentCampus != null ? SessionVariables.CurrentCampus.Id : string.Empty,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        DarkModeEnabled = false
                    };
                    work.UserSetting.Create(model.Settings);
                }

                model.Notes = work.Note.GetAllByUserId(model.User.Id);
                model.Users = (from u in db.Users
                               join n in db.Notes on u.Id equals n.TypeId
                               where u.Id == model.User.Id
                               select u).ToList();
                model.Attachments = work.Attachment.GetAll(model.User.Id, Constants.User);
            }

            model.DonorStatus = work.Giving.GetDonorStatus(model.Person.Id);
            model.Relationships = work.Person.GetAllRelatives(model.Person.Id, true);
            var households = work.Household.GetAllHouseholdsByPerson(model.Person.Id);

            if (households.Any())
            {
                foreach (var item in households)
                {
                    var familyRole = !string.IsNullOrEmpty(item.HouseholdMember.FamilyRole) ? " (" + item.HouseholdMember.FamilyRole + ")" : string.Empty;

                    if (Utilities.IsDonorOnly(currentUserRoles))
                    {
                        model.Households += model.Households.IsNullOrEmpty() ? $"{item.Household.Display}" + familyRole : $", {item.Household.Display}" + familyRole;
                    }
                    else
                    {
                        var returnUrl = Url.Encode($"/Users/UserProfile?id={id}&type={model.Type}");
                        model.Households += model.Households.IsNullOrEmpty() ? $"<a href='/people/householdMembers/?id={item.Household.Id}&returnUrl={returnUrl}'>{item.Household.Display}</a>" + familyRole : $", <a href=' /people/householdMembers/{item.Household.Id}&returnUrl={returnUrl}'>{item.Household.Display}</a>" + familyRole;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserProfile(UserView model, HttpPostedFileBase ImageFile)
        {
            if (model.Type == Constants.User)
            {
                if (ModelState.IsValid)
                {
                    var user = work.User.Get(model.User.Id);
                    var person = work.Person.Get(user.PersonId);

                    if (ImageFile?.ContentLength > 0)
                    {
                        if (Utilities.IsImage(ImageFile.FileName))
                        {
                            var newFileName = Utilities.GenerateUniqueFileName(ImageFile.FileName);
                            var success = AwsHelpers.UploadImage(newFileName, ImageFile, "Uploads/ProfileImages");

                            if (success)
                            {
                                if (!string.IsNullOrEmpty(model.Settings.ProfileImage))
                                {
                                    AwsHelpers.DeleteImage(model.Settings.ProfileImage, "Uploads/ProfileImages");
                                }

                                if (!string.IsNullOrEmpty(newFileName))
                                {
                                    model.Settings.ProfileImage = newFileName;
                                    model.Settings.ModifiedDate = DateTime.Now;
                                    model.Settings.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                                    work.UserSetting.Update(model.Settings);

                                    //update image in person table also
                                    person.ProfileImage = newFileName;

                                    //update user setting in session after uploading new profile picture (If current user and logged-in user are the same).
                                    if (user.Id == SessionVariables.CurrentUser.User.Id)
                                    {
                                        SessionVariables.CurrentUser.Settings = model.Settings;
                                    }
                                }
                            }
                        }
                        else
                        {
                            CreateAlertMessage("You can only upload image file types (.jpg, .jpeg, .png, .gif, .bmp, .ico).", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                            return RedirectToAction(nameof(UserProfile), new { id = model.User.Id, type = Constants.User });
                        }
                    }

                    if (!string.IsNullOrEmpty(model.User.PhoneNumber))
                    {
                        //check if the user changed the phone
                        if (user.PhoneNumber != model.User.PhoneNumber)
                        {
                            //check if the new phone already belongs to a user
                            var phoneAlreadyExists = work.User.AnyByPhone(model.User.PhoneNumber);

                            if (phoneAlreadyExists)
                            {
                                CreateAlertMessage(Constants.PhoneExistsMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                                return View(model);
                            }

                            //Set the new phone number and send verification code
                            user.PhoneNumber = model.User.PhoneNumber;
                            user.PhoneNumberConfirmed = false;
                            user.PhoneVerificationCode = Utilities.GenerateVerificationCode();

                            var returnUrl = "https://www.app.praisecms.com/Users/_VerifyPhone?verificationCode=" + user.PhoneVerificationCode;
                            var smsMessage = new SmsMessage
                            {
                                Id = Utilities.GenerateUniqueId(),
                                To = user.PhoneNumber,
                                Message = "\n\nPlease click the link below to verify your new phone number.\n\n" + returnUrl +
                                          "\n\nRequested: " + DateTime.Now,
                                CreatedDate = DateTime.Now,
                                CreatedBy = SessionVariables.CurrentUser.User.Id
                            };
                            Utilities.SendMessage(smsMessage);
                        }
                    }

                    if (!string.IsNullOrEmpty(model.User.Email))
                    {
                        //check if the user changed the email
                        if (user.Email != model.User.Email)
                        {
                            //check if the new email already belongs to a user
                            var emailAlreadyExists = work.User.AnyByEmail(model.User.Email);

                            if (emailAlreadyExists)
                            {
                                CreateAlertMessage(Constants.EmailExistsMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                                return View(model);
                            }

                            //Set the new email
                            user.Email = model.User.Email.ToLower();
                            user.UserName = model.User.Email.ToLower();
                            user.EmailConfirmed = false;
                            user.EmailVerificationCode = Utilities.GenerateVerificationCode();

                            var callbackUrl = Url.Action("Login", "Account", new { userId = user.Id, code = user.SecurityStamp }, protocol: Request.Url.Scheme);
                            var content = EmailTemplates.EmailUpdate_body.Replace("{btn_url}", callbackUrl);
                            var email = new Email
                            {
                                Id = Utilities.GenerateUniqueId(),
                                Message = content,
                                To = user.Email,
                                Attachments = null,
                                Subject = "Email verification",
                                CreatedBy = SessionVariables.CurrentUser.User.Id,
                                CreatedDate = DateTime.Now
                            };
                            Emailer.SendEmail(email);
                        }
                    }

                    user.FirstName = model.User.FirstName;
                    user.LastName = model.User.LastName;
                    user.PhoneNumber = model.User.PhoneNumber;
                    user.Address1 = model.User.Address1;
                    user.Address2 = model.User.Address2;
                    user.City = model.User.City;
                    user.State = model.User.State;
                    user.Zip = model.User.Zip;

                    work.User.Update(user);

                    if (person.IsNotNullOrEmpty())
                    {
                        person.FirstName = model.User.FirstName;
                        person.LastName = model.User.LastName;
                        person.PhoneNumber = model.User.PhoneNumber;
                        person.Address1 = model.User.Address1;
                        person.Address2 = model.User.Address2;
                        person.City = model.User.City;
                        person.State = model.User.State;
                        person.Zip = model.User.Zip;
                        person.MaritalStatus = model.User.Person.MaritalStatus;
                        person.Ethnicity = model.User.Person.Ethnicity;
                        person.Education = model.User.Person.Education;
                        person.EmploymentStatus = model.User.Person.EmploymentStatus;
                        person.FamilySize = model.User.Person.FamilySize;
                        person.Gender = model.User.Person.Gender;
                        person.PrimaryLanguage = model.User.Person.PrimaryLanguage;
                        person.DOB = model.User.Person.DOB;
                        person.DeceasedDate = model.User.Person.DeceasedDate;
                        person.BaptismDate = model.User.Person.BaptismDate;
                        person.FirstVisitDate = model.User.Person.FirstVisitDate;
                        person.MemberVisitorStatus = model.User.Person.MemberVisitorStatus;
                        person.MembershipDate = model.User.Person.MembershipDate;
                        work.Person.Update(person);
                    }

                    //Update current user session variable if same as user being edited
                    if (user.Id == SessionVariables.CurrentUser.User.Id)
                    {
                        SessionVariables.CurrentUser.User = user;
                    }

                    CreateAlertMessage(Constants.SavedMessage, AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return RedirectToAction(nameof(UserProfile), new { id = model.User.Id, type = Constants.User });
                }
            }
            else
            {
                if (!TryValidateModel(model.Person)) return View(model);

                var person = work.Person.Get(model.Person.Id);
                if (ImageFile?.ContentLength > 0)
                {
                    if (Utilities.IsImage(ImageFile.FileName))
                    {
                        var newFileName = Utilities.GenerateUniqueFileName(ImageFile.FileName);
                        var success = AwsHelpers.UploadImage(newFileName, ImageFile, "Uploads/ProfileImages");

                        if (success)
                        {
                            if (!string.IsNullOrEmpty(person.ProfileImage))
                            {
                                AwsHelpers.DeleteImage(person.ProfileImage, "Uploads/ProfileImages");
                            }

                            if (!string.IsNullOrEmpty(newFileName))
                            {
                                person.ProfileImage = newFileName;
                            }
                        }
                    }
                    else
                    {
                        CreateAlertMessage("You can only upload image file types (.jpg, .jpeg, .png, .gif, .bmp, .ico).", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                        return RedirectToAction(nameof(UserProfile), new { id = model.Person.Id, type = Constants.Person });
                    }
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    //check if the user changed the phone
                    if (person.PhoneNumber != model.PhoneNumber)
                    {
                        //check if the new phone already belongs to a user
                        var phoneAlreadyExists = work.Person.GetByPhone(model.PhoneNumber).IsNotNullOrEmpty();

                        if (phoneAlreadyExists)
                        {
                            CreateAlertMessage("Uh-oh! This phone number has already been registered. Please try another phone number.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                            return View(model);
                        }

                        person.PhoneNumber = model.PhoneNumber;
                    }
                }

                if (!string.IsNullOrEmpty(model.Person.Email))
                {
                    //check if the user changed the email
                    if (person.Email != model.Person.Email)
                    {
                        //check if the new email already belongs to a user
                        var emailAlreadyExists = work.Person.GetByEmail(model.Person.Email).IsNotNullOrEmpty();

                        if (emailAlreadyExists)
                        {
                            CreateAlertMessage("Uh-oh! This email has already been assigned to another user. Please try another email or contact us if you feel this was made in error.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                            return View(model);
                        }

                        person.Email = model.Person.Email.ToLower();
                    }
                }

                person.FirstName = model.Person.FirstName;
                person.LastName = model.Person.LastName;
                person.PhoneNumber = model.Person.PhoneNumber;
                person.Address1 = model.Person.Address1;
                person.Address2 = model.Person.Address2;
                person.City = model.Person.City;
                person.State = model.Person.State;
                person.Zip = model.Person.Zip;
                person.MaritalStatus = model.Person.MaritalStatus;
                person.Ethnicity = model.Person.Ethnicity;
                person.Education = model.Person.Education;
                person.EmploymentStatus = model.Person.EmploymentStatus;
                person.FamilySize = model.Person.FamilySize;
                person.Gender = model.Person.Gender;
                person.PrimaryLanguage = model.Person.PrimaryLanguage;
                person.DOB = model.Person.DOB;
                person.DeceasedDate = model.Person.DeceasedDate;
                person.BaptismDate = model.Person.BaptismDate;
                person.FirstVisitDate = model.Person.FirstVisitDate;
                person.MemberVisitorStatus = model.Person.MemberVisitorStatus;
                person.MembershipDate = model.Person.MembershipDate;

                work.Person.Update(person);
                CreateAlertMessage(Constants.SavedMessage, AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction(nameof(UserProfile), new { id = model.Person.Id, type = Constants.Person });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> AccountInfo(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = SessionVariables.CurrentUser.User.Id;
            }

            ViewBag.Tab = "account-info";

            var model = new UserView
            {
                Type = Constants.User,
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                User = UserManager.FindById(userId) ?? new ApplicationUser(),
                Roles = GetAllRoles(),
                CurrentUserRoles = work.User.GetRolesByUserId(userId).Select(x => x.Id).ToList(),
                Settings = work.UserSetting.GetByUserId(userId)
            };

            var person = work.Person.GetByUserId(userId);
            model.DonorStatus = work.Giving.GetDonorStatus(person.Id);

            if (model.Settings == null)
            {
                model.Settings = new UserSetting
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = userId,
                    PrimaryChurchId = SessionVariables.CurrentChurch.Id,
                    PrimaryChurchCampusId = SessionVariables.CurrentCampus.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    DarkModeEnabled = false
                };
                work.UserSetting.Create(model.Settings);
            }

            model.Notes = work.Note.GetAllByUserId(model.User.Id);
            model.Users = (from u in db.Users
                           join n in db.Notes on u.Id equals n.TypeId
                           where u.Id == model.User.Id
                           select u).ToList();
            model.Attachments = work.Attachment.GetAll(model.User.Id, Constants.User);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AccountInfo(UserView model)
        {
            if (ModelState.IsValid)
            {
                model.Settings.PrimaryChurchCampusId = model.Settings.PrimaryChurchCampusId.IsNullOrEmpty() ? "unregister" : model.Settings.PrimaryChurchCampusId;

                if (SessionVariables.CurrentUser.IsSuperAdmin || SessionVariables.CurrentUser.IsAdmin)
                {
                    adoData.UpdateUserRoles(model.User.Id, model.CurrentUserRoles);
                    var user = work.User.Get(model.Settings.UserId);

                    if (!user.IsActive.Equals(model.User.IsActive))
                    {
                        user.IsActive = model.User.IsActive;
                        work.User.Update(user);
                    }
                }

                model.Settings.ModifiedDate = DateTime.Now;
                model.Settings.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.UserSetting.Update(model.Settings);

                //Update the session variable user settings to match changes if the user is the same as current user
                if (model.Settings.UserId.Equals(SessionVariables.CurrentUser.User.Id))
                {
                    var user = work.User.Get(model.Settings.UserId);
                    Utilities.SetSessionVariables(user, SessionVariables.CurrentChurch.Id);
                }

                CreateAlertMessage(Constants.SavedMessage, AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction("accountinfo", "Users", new { userId = model.User.Id });
            }

            model.User = UserManager.FindById(model.User.Id) ?? new ApplicationUser();
            model.Roles = GetAllRoles();
            model.CurrentUserRoles = adoData.ReadRolesByUserId(model.User.Id).Select(x => x.Id).ToList();

            return View(model);
        }

        [HttpGet]
        public ActionResult SecurityPreferences(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = SessionVariables.CurrentUser.User.Id;
            }

            ViewBag.Tab = "security-preferences";
            var model = new UserView
            {
                Type = Constants.User,
                User = work.User.Get(userId)
            };

            var person = work.Person.GetByUserId(userId);
            model.DonorStatus = work.Giving.GetDonorStatus(person.Id);

            return View(model);
        }

        [HttpGet]
        public ActionResult _ChangePassword(string userId)
        {
            var model = new ChangePasswordViewModel { UserId = userId };
            return PartialView("_ChangePassword", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_ChangePassword", model);

            try
            {
                var result = await UserManager.ChangePasswordAsync(model.UserId, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    //Is it necessary to find the user and sign them in if they have changed the password?  Does this change the session at all?
                    var user = await UserManager.FindByIdAsync(model.UserId);

                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }

                    CreateAlertMessage("Your password has been changed.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    var content = EmailTemplates.PasswordChanged_body.Replace("{btn_url}", ApplicationCache.Instance.SiteConfiguration.Url + "Account/Login");
                    var email = new Email
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = content,
                        To = user.Email,
                        Attachments = null,
                        Subject = "Password Changed",
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    };
                    Emailer.SendEmail(email);
                }
                else
                {
                    CreateAlertMessage(ConvertToAlertString(result), AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }

                var errorObj = logRepository.JsonConverter();
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Password Changed", string.Empty, LogStatuses.Error, errorObj);

                return AjaxRedirectTo("/users/securitypreferences?userId=" + model.UserId);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"{Constants.DefaultErrorMessage} {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_ChangePassword", model);
        }

        public ActionResult ResetUserPassword(string userId, string email, string token)
        {
            var user = work.User.GetBySecurityStamp(userId, token);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = user.SecurityStamp }, protocol: Request.Url.Scheme);
            var content = EmailTemplates.ResetPassword_body.Replace("{btn_url}", callbackUrl);

            var emailModel = new Email
            {
                Id = Utilities.GenerateUniqueId(),
                Message = content,
                To = email,
                Attachments = null,
                Subject = "Reset Password",
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            var sent = Emailer.SendEmail(emailModel);
            var logObj = logRepository.JsonConverter("Email", email);

            if (sent)
            {
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Reset User Password", userId, LogStatuses.Done, logObj);
                CreateAlertMessage("A reset password email has been sent to the user.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Reset User Password", userId, LogStatuses.Error, logObj);
                CreateAlertMessage("There was a problem sending the reset password email. Please try again later.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            var url = $"/users/securitypreferences?userId={userId}";

            return AjaxRedirectTo(url);
        }

        public string ResetPassword(string userId, string currentPassword, string newPassword, string verifyPassword)
        {
            //what is this action used for?
            if (!currentPassword.IsNotNullOrEmpty()) return null;

            var user = work.User.Get(userId);
            var result = SignInManager.UserManager.CheckPassword(user, currentPassword);

            if (!result) return "Your current password is incorrect";

            if (!newPassword.IsNotNullOrEmpty() || !verifyPassword.IsNotNullOrEmpty())
                return "New password or verify password is required";

            if (newPassword.Length < 6) return "Password must be at least 6 characters long";

            if (newPassword != verifyPassword) return "New password and verify password must be same";

            user.PasswordHash = UserManager.PasswordHasher.HashPassword(newPassword);
            work.User.Update(user);

            return "Success";
        }

        [HttpGet]
        public ActionResult _togglesidebar()
        {
            var settings = work.UserSetting.Get(SessionVariables.CurrentUser.Settings.Id);
            settings.ModifiedDate = DateTime.Now;
            settings.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            settings.SidebarCollapsed = !settings.SidebarCollapsed;

            work.UserSetting.Update(settings);
            SessionVariables.CurrentUser.Settings.SidebarCollapsed = settings.SidebarCollapsed;

            return null;
        }

        public ActionResult TwoFactorEnabled(bool value)
        {
            var result = work.User.TwoFactorEnabled(value, SessionVariables.CurrentUser.User.Id);

            return Json(result.ResultType == ResultType.Success ? new ResponseModel { Success = true, Message = result.Message } : new ResponseModel { Success = false, Message = result.Message });
        }

        public ActionResult ViewByRole(string id)
        {
            ViewBag.RoleName = work.Role.Get(id).Name;

            var model = new UsersViewModel
            {
                ApplicationUsers = work.User.GetByRoleId(id, SessionVariables.CurrentChurch.Id),
                UserSettings = work.UserSetting.GetByChurchId(SessionVariables.CurrentChurch.Id)
            };

            // Get a list of user IDs from the application users
            var userIds = model.ApplicationUsers.Select(u => u.Id).ToList();

            // Populate the ApplicationRoles dictionary
            model.ApplicationRoles = adoData.ReadRolesByUserIds(userIds);

            return View(model);
        }

        #region Template Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }

            return RedirectToAction("UsersLogins", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }

            return RedirectToAction("MyProfile", "Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }

            return RedirectToAction("MyProfile", "Users");
        }

        [AllowAnonymous]
        public ActionResult SetPassword(string userId, string token)
        {
            if (UserManager.HasPassword(userId) ||
                !UserManager.VerifyUserToken(userId, "SET_PASSWORD", token.Replace(" ", "+")))
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }

            return View(new SetPasswordViewModel { UserId = userId });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(model.UserId, model.NewPassword);

                if (result.Succeeded)
                {
                    return RedirectToActionPermanent(nameof(AccountController.Login), "Account");
                }

                AddErrors(result);
            }

            DisplayErrors();

            return View(model);
        }
        #endregion

        #region Helpers
        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return user?.PasswordHash != null;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        private List<ApplicationRoles> GetAllRoles()
        {
            string churchId = SessionVariables.CurrentChurch?.Id;
            bool isSuperAdmin = SessionVariables.CurrentUser.IsSuperAdmin;

            // Fetch roles based on the churchId
            var roles = adoData.ReadAllRoles(churchId);

            // Filter out the SuperAdmin role if the user is not a super admin
            if (!isSuperAdmin)
            {
                roles = roles.Where(role => role.Name != Roles.SuperAdmin).ToList();
            }

            return roles;
        }

        public static string ConvertToAlertString(ModelStateDictionary modelState)
        {
            return modelState.ToList().SelectMany(item => item.Value.Errors).Aggregate(string.Empty, (current, val) => current + val.ErrorMessage + "</br>");
        }

        public static string ConvertToAlertString(IdentityResult identityResult)
        {
            return identityResult.Errors.Aggregate(string.Empty, (current, item) => current + item + "</br>");
        }
        #endregion

        #region Verification Actions
        [AllowAnonymous]
        [HttpGet]
        public ActionResult _SendVerificationPhoneCode(string id)
        {
            var user = work.User.Get(id);

            if (user != null && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
                work.User.Update(user);

                var smsMessage = new SmsMessage
                {
                    Id = Utilities.GenerateUniqueId(),
                    To = user.PhoneNumber,
                    Message = (SessionVariables.CurrentChurch != null && !string.IsNullOrEmpty(SessionVariables.CurrentChurch.Name) ? SessionVariables.CurrentChurch.Name : "Praise CMS") +
                    "\n\nPlease enter the code " + user.PhoneVerificationCode + " to verify your phone number." +
                          "\n\nRequested: " + DateTime.Now + " by " + SessionVariables.CurrentUser.User.FullName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                    Type = SmsMessageType.SignIn
                };
                Utilities.SendMessage(smsMessage);
            }

            return PartialView("_SendVerificationPhoneCode", user);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult _SendVerificationPhoneCode(string verificationCode, string id)
        {
            var user = work.User.Get(id);

            if (user?.PhoneVerificationCode.Equals(verificationCode) == true)
            {
                user.PhoneNumberConfirmed = true;
                work.User.Update(user);

                return AjaxRedirectTo("/Users/UserProfile");
            }

            ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");

            //make sure no one can just go to this action and access the users page
            return PartialView("_SendVerificationPhoneCode", user);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult _RetrySendVerificationPhoneCode(string id)
        {
            var user = work.User.Get(id);

            if (user == null || string.IsNullOrEmpty(user.PhoneNumber)) return JavaScript("StopLoading();");

            user.PhoneVerificationCode = Utilities.GenerateVerificationCode();
            work.User.Update(user);

            var smsMessage = new SmsMessage
            {
                Id = Utilities.GenerateUniqueId(),
                To = user.PhoneNumber,
                Message = (SessionVariables.CurrentChurch != null && !string.IsNullOrEmpty(SessionVariables.CurrentChurch.Name) ? SessionVariables.CurrentChurch.Name : "Praise CMS") +
                          "\n\nPlease enter the code " + user.PhoneVerificationCode + " to verify your phone number." +
                          "\n\nRequested: " + DateTime.Now + " by " + SessionVariables.CurrentUser.User.FullName,
                CreatedDate = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(user.Id) ? user.Id : Constants.System,
                Type = SmsMessageType.SignIn
            };
            Utilities.SendMessage(smsMessage);

            return JavaScript("StopLoading();");
        }

        [AllowAnonymous]
        public ActionResult _VerifyPhone(string verificationCode, string id)
        {
            var user = work.User.Get(id);

            if (user?.PhoneVerificationCode.Equals(verificationCode) != true)
                return RedirectToAction("MyProfile", new { userId = id, tab = "profile" });

            user.PhoneNumberConfirmed = true;
            work.User.Update(user);

            return RedirectToAction("MyProfile", new { userId = id, tab = "profile", methodVerified = "phone" });

            //make sure no one can just go to this action and access the users page
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult _SendVerificationEmail(string id)
        {
            var user = work.User.Get(id);

            if (user == null)
                return JavaScript("$('#SendEmailCode').hide();$('#EmailCodeStatus').show();StopLoading();");

            user.EmailVerificationCode = Utilities.GenerateUniqueId();
            work.User.Update(user);

            var content = EmailTemplates.VerifyEmail_body.Replace("{btn_url}", ApplicationCache.Instance.SiteConfiguration.Url + "/manage/_VerifyEmail?verificationCode=" + user.EmailVerificationCode + "&id=" + id);
            var email = new Email
            {
                Id = Utilities.GenerateUniqueId(),
                Message = content,
                To = user.Email,
                Attachments = null,
                Subject = "Email verification",
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };
            Emailer.SendEmail(email);

            return JavaScript("$('#SendEmailCode').hide();$('#EmailCodeStatus').show();StopLoading();");
        }

        [AllowAnonymous]
        public ActionResult _VerifyEmail(string verificationCode, string id)
        {
            var user = work.User.GetByVerificationIdAndCode(id, verificationCode);

            if (!user.IsNotNull()) return Redirect("/");

            user.EmailConfirmed = true;
            work.User.Update(user);

            return Redirect("/");
        }
        #endregion

        #region Import File
        public ActionResult _ImportUser()
        {
            return PartialView("_ImportUser");
        }

        public ActionResult Upload()
        {
            if (Request == null) return RedirectToAction("Index");

            var file = Request.Files["UploadedFile"];

            if (file == null || file.ContentLength <= 0 || string.IsNullOrEmpty(file.FileName))
                return RedirectToAction("Index");

            var fileStream = file.InputStream;
            var reader = ExcelReaderFactory.CreateReader(fileStream);
            var result = reader.AsDataSet();
            var tables = new List<DataTable>(result.Tables.Cast<DataTable>());

            foreach (var table in tables.Where(table => table.TableName.IsNotNullOrEmpty()))
            {
                if (table.Rows.Count > 0)
                {
                    var headers = table.Rows[0];
                    var address = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("address"));
                    var firstName = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("firstname"));
                    var lastName = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("lastname"));
                    var type = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("type"));
                    var company = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("company"));
                    var email = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("email"));
                    var dateOfBirth = Array.FindIndex(headers.ItemArray, m => m.ToString().ContainsIgnoreCase("dateofbirth"));
                    var users = new List<ApplicationUser>();

                    for (var i = 1; i <= table.Rows.Count - 1; i++)
                    {
                        var row = table.Rows[i];
                        var user = new ApplicationUser
                        {
                            FirstName = firstName > -1 ? row.ItemArray[firstName].ToString() : string.Empty,
                            LastName = lastName > -1 ? row.ItemArray[lastName].ToString() : string.Empty,
                            Address1 = address > -1 ? row.ItemArray[address].ToString() : string.Empty,
                            Email = email > -1 ? row.ItemArray[email].ToString() : string.Empty
                        };
                        users.Add(user);
                    }
                }
                break;
            }

            return RedirectToAction("Index");
        }
        #endregion

        public ActionResult History(string id, string startDate, string endDate, string fundId, string campusId)
        {
            var dashboard = work.Giving.GetHistory(SessionVariables.CurrentChurch.Id, null, personId: id, startDate, endDate, fundId, campusId);
            dashboard.Data.PersonId = id;

            return View(dashboard.Data);
        }

        public ActionResult DownloadGivingHistory(string id, string startDate, string endDate, string fundId, string campusId)
        {
            //Change to dynamic - user to select year or none.
            var payments = work.Giving.GetHistory(SessionVariables.CurrentChurch.Id, null, personId: id, startDate, endDate, fundId, campusId);
            var rows = new List<string> { "Date,Fund,Campus,Payment Method,Amount" };

            foreach (var payment in payments.Data.MyGiving)
            {
                var columns = new List<string>
                {
                    payment.CreatedDate.ToShortDateString()
                };

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

            var totalsRow = new List<string> { "Total", string.Empty, string.Empty, string.Empty, payments.Data.MyGiving.Sum(x => x.Amount).ToCurrencyString() };
            rows.Add(string.Join(",", totalsRow.Select(x => "\"" + x + "\"")));

            var data = string.Join("\r\n", rows);
            var person = !string.IsNullOrEmpty(id) ? work.Person.Get(id) : null;

            var filenameSuffix = person.IsNotNullOrEmpty() ? person?.Display.FilenameFriendlyLower() : SessionVariables.CurrentChurch.Name.FilenameFriendlyLower();

            var suggestedFilename = $"givinghistory_{filenameSuffix}.csv";

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult DeleteUser(string userId)
        {
            var result = work.User.DeleteUserAndRelatedData(userId);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage("The user has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction("Index", "Users");
            }
            else
            {
                CreateAlertMessage("Uh-oh! There was a problem creating the user.<br>" + result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return RedirectToAction("Index", "Users");
            }
        }
    }
}