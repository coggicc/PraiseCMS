using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class UserOperations : GenericRepository
    {
        readonly string _superAdminEmail = ConfigurationManager.AppSettings["SuperAdminEmail"];
        public UserOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public bool AnyByEmail(string email)
        {
            return Read<ApplicationUser>().Any(x => x.Email == email);
        }

        public bool AnyByPhone(string phone)
        {
            return Read<ApplicationUser>().Any(x => x.PhoneNumber == phone);
        }

        public ApplicationUser Get(string id)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.Id == id);
        }

        public List<ApplicationUser> GetAllByChurchId(string churchId)
        {
            var churchUserIds = GetAllUsersIdsByChurchId(churchId);
            return Work.User.GetAll(churchUserIds).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
        }

        public List<ApplicationUser> GetAllByChurchIds(List<string> churchIds)
        {
            var userIds = Work.Church.GetAllUsers(churchIds).Select(x => x.UserId).ToList();
            return Work.User.GetAll(userIds);
        }

        public List<string> GetAllUsersIdsByChurchId(string churchId)
        {
            return Read<ChurchUser>().Where(x => x.ChurchId == churchId).Select(x => x.UserId).ToList();
        }

        public ApplicationUser GetByPersonId(string personId)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.PersonId.Equals(personId));
        }

        public List<ApplicationUser> GetAll(List<string> ids)
        {
            return Read<ApplicationUser>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<ApplicationUser> GetAll(bool includeInactive = false)
        {
            return Read<ApplicationUser>()
                .Where(q => includeInactive || q.IsActive)
                .ToList();
        }

        public List<ApplicationUser> GetAllByPersonIds(List<string> personIds)
        {
            return Read<ApplicationUser>().Where(x => personIds.Contains(x.PersonId)).OrderBy(q => q.FirstName).ThenBy(q => q.LastName).ToList();
        }

        public List<ApplicationUser> GetAllByEmails(List<string> emails)
        {
            return Read<ApplicationUser>().Where(x => emails.Contains(x.Email)).ToList();
        }

        public List<ApplicationUser> GetAllByEmails(List<Email> emails)
        {
            var results = new List<ApplicationUser>();
            var toEmails = emails.Select(x => x.To).Distinct().ToList();
            var fromIds = emails.Select(x => x.CreatedBy).Distinct().ToList();
            results.AddRange(Work.User.GetAllByEmails(toEmails));
            results.AddRange(Work.User.GetAll(fromIds));

            return results;
        }

        public ApplicationUser GetByVerificationIdAndCode(string id, string code)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.Id == id && x.EmailVerificationCode == code);
        }

        public ApplicationUser GetByExternalProviderId(string externalProviderId)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.ExternalProviderId == externalProviderId);
        }

        public ApplicationUser GetByPhone(string phone)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.PhoneNumber == phone);
        }

        public ApplicationUser GetByPhonePlain(string phone)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.PhoneNumber == phone && (x.ExternalProvider == string.Empty || x.ExternalProvider == null));
        }

        public ApplicationUser GetByEmail(string email)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.Email == email);
        }

        public ApplicationUser GetByEmailPlain(string email)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.Email == email && (x.ExternalProvider == string.Empty || x.ExternalProvider == null));
        }

        public ApplicationUser GetByEmailAndPhone(string email, string phone)
        {
            if (email.IsNotNullOrEmpty())
            {
                var byEmail = GetByEmailPlain(email);

                if (byEmail.IsNotNull())
                {
                    return byEmail;
                }
            }

            if (phone.IsNotNullOrEmpty())
            {
                var byPhone = GetByPhonePlain(phone);

                if (byPhone.IsNotNull())
                {
                    return byPhone;
                }
            }

            return null;
        }

        public List<ApplicationUser> GetByRoleId(string roleId, string churchId = null)
        {
            return DAL.ReadUsersByRoleId(roleId, churchId);
        }

        public List<ApplicationUser> GetUsersByChurch(string churchId)
        {
            var churchUsers = Work.Church.GetAllUsers(churchId);

            if (!churchUsers.Any())
                return new List<ApplicationUser>();

            var userIds = churchUsers.Select(x => x.UserId).ToList();

            return Read<ApplicationUser>()
                .Where(x => userIds.Contains(x.Id))
                .ToList();
        }

        public ApplicationUser VerifyByEmailAndPhone(string email, string phone)
        {
            return Db.Users.FirstOrDefault(x => (x.Email == email && x.Email != null)
            || (x.PhoneNumber == phone && x.PhoneNumber != null));
        }

        public List<ApplicationUser> GetRequester(string createdBy, string modifyBy)
        {
            return Read<ApplicationUser>().Where(x => x.Id == createdBy || x.Id == modifyBy).ToList();
        }

        public ApplicationUser GetUserByFirstLastNameChurchId(string firstName, string lastName, string churchId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return new ApplicationUser();
            }

            var users = Db.Users.Where(p => Db.ChurchUsers.Any(p2 => p2.UserId == p.Id && p2.ChurchId == churchId)).ToList();

            return users.FirstOrDefault(x => string.Equals(x.FirstName, firstName.Trim(), StringComparison.OrdinalIgnoreCase) && string.Equals(x.LastName, lastName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public List<ApplicationUser> GetBaptismUsers(DateRange dateRange)
        {
            var people = Read<Person>().Where(x => x.BaptismDate != null && x.BaptismDate >= dateRange.StartDate && x.BaptismDate <= dateRange.EndDate)
                .OrderByDescending(x => x.BaptismDate).ThenBy(x => x.FirstName).ToList();
            var users = Work.User.GetAllByPersonIds(people.Select(x => x.Id).ToList());

            return users.Select(x => { x.Person = people.Find(q => q.Id.Equals(x.PersonId)); return x; }).ToList();
        }

        public ApplicationUser GetPraiseUser()
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.Email.Equals(_superAdminEmail) || x.UserName.Equals(_superAdminEmail) || x.FirstName.Equals("Praise") || x.LastName.Equals("CMS"));
        }

        public ApplicationUser GetBySecurityStamp(string securityStamp)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.SecurityStamp == securityStamp);
        }

        public ApplicationUser GetByEmailAndSecurityStamp(string securityStamp, string email)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.SecurityStamp.Equals(securityStamp) && x.Email.Equals(email));
        }

        public ApplicationUser GetBySecurityStampAndEmailCode(VerifyCodeViewModel model)
        {
            return model.Provider == "Phone" ? Read<ApplicationUser>().FirstOrDefault(x => x.SecurityStamp == model.Token && x.PhoneVerificationCode == model.Code) : Read<ApplicationUser>().FirstOrDefault(x => x.SecurityStamp == model.Token && x.EmailVerificationCode == model.Code);
        }

        public ApplicationUser GetBySecurityStamp(string userId, string securityStamp)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.Id == userId && x.SecurityStamp == securityStamp);
        }

        public ApplicationUser GetByVerificationCode(string phone, string verificationCode)
        {
            return Read<ApplicationUser>().FirstOrDefault(x => x.PhoneNumber == phone && x.PhoneVerificationCode == verificationCode);
        }

        #region Roles
        public List<ApplicationUser> GetApplicationUsersWithRoles(string churchId)
        {
            if (!churchId.IsNotNullOrEmpty())
                return new List<ApplicationUser>();

            var churchUsers = Work.Church.GetAllUsers(churchId);

            var userIds = churchUsers.Select(x => x.UserId).ToList();

            return GetApplicationUsersWithRoles(userIds);
        }

        public List<ApplicationUser> GetApplicationUsersWithRoles(List<string> ids = null)
        {
            var users = ids.IsNotNullOrEmpty() ? GetAll(ids) : GetAll(true);
            var userRoleIds = DAL.ReadAllUserRoles();
            var userRoles = DAL.ReadAllRoles();

            foreach (var user in users)
            {
                var currentUserRoles = userRoleIds.Where(x => x.UserId == user.Id).Select(x => x.RoleId).ToList();

                if (currentUserRoles.IsNotNull() && currentUserRoles.Count > 0)
                {
                    user.UserRolesList = userRoles.Where(x => currentUserRoles.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList();
                }
            }

            return users;
        }

        public List<ApplicationRoles> GetRolesByUserId(string userId)
        {
            return DAL.ReadRolesByUserId(userId);
        }

        public List<UsersWithRoles> GetAllByChurchRole(string churchId, string role)
        {
            return DAL.GetAllChurchUsersByRole(churchId, role);
        }

        public List<UsersWithRoles> GetAllByChurchRoles(string churchId, List<string> roles)
        {
            return DAL.GetAllChurchUsersByRoles(churchId, roles);
        }

        public List<UsersWithRoles> GetChurchUsersWithRoles(string churchId)
        {
            var churchUsers = Work.Church.GetAllUsers(churchId);

            if (!churchUsers.Any())
                return new List<UsersWithRoles>();

            var userIds = churchUsers.Select(x => x.UserId).ToList();

            return GetUsersWithRoles(userIds);
        }

        public List<UsersWithRoles> GetUsersWithRoles(IEnumerable<string> ids)
        {
            var users = (from u in Read<ApplicationUser>()
                         join us in Read<UserSetting>() on u.Id equals us.UserId
                         where ids.Contains(u.Id)
                         select new UsersWithRoles
                         {
                             AccessFailedCount = u.AccessFailedCount,
                             Address1 = u.Address1,
                             Address2 = u.Address2,
                             AssignedToChurch = u.AssignedToChurch,
                             City = u.City,
                             ConvertedToUserById = u.ConvertedToUserById,
                             ConvertedToUserDate = u.ConvertedToUserDate,
                             CreatedBy = u.CreatedBy,
                             CreatedDate = u.CreatedDate,
                             Email = u.Email,
                             EmailConfirmed = u.EmailConfirmed,
                             EmailVerificationCode = u.EmailVerificationCode,
                             ExternalProvider = u.ExternalProvider,
                             ExternalProviderId = u.ExternalProviderId,
                             FirstName = u.FirstName,
                             Id = u.Id,
                             InboxDensity = u.InboxDensity,
                             InboxType = u.InboxType,
                             IsActive = u.IsActive,
                             LastAccessedDate = u.LastAccessedDate,
                             LastLogin = u.LastLogin,
                             LastName = u.LastName,
                             LockoutEnabled = u.LockoutEnabled,
                             LockoutEndDateUtc = u.LockoutEndDateUtc,
                             Number = u.Number,
                             PasswordHash = u.PasswordHash,
                             PhoneNumber = u.PhoneNumber,
                             PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                             PhoneVerificationCode = u.PhoneVerificationCode,
                             SecurityStamp = u.SecurityStamp,
                             ShowWelcomeMessage = u.ShowWelcomeMessage,
                             TwoFactorEnabled = u.TwoFactorEnabled,
                             State = u.State,
                             PersonId = u.PersonId,
                             UserName = u.UserName,
                             Zip = u.Zip,
                             ProfileImage = us.ProfileImage
                         }).ToList();

            var userRoleIds = DAL.ReadAllUserRoles();
            var userRoles = DAL.ReadAllRoles();

            foreach (var user in users)
            {
                var currentUserRoles = userRoleIds.Where(x => x.UserId == user.Id).Select(x => x.RoleId).ToList();

                if (currentUserRoles.IsNotNull() && currentUserRoles.Count > 0)
                {
                    var roleTitle = userRoles.Where(x => currentUserRoles.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList();
                    user.UserRoles = roleTitle.CombineListToString();
                }
            }

            if (!SessionVariables.CurrentUser.IsSuperAdmin)
            {
                users = users.FindAll(q => q.UserRoles.IsNullOrEmpty() || (q.UserRoles.IsNotNullOrEmpty() && !q.UserRoles.Contains(Roles.SuperAdmin))).ToList();
            }

            return users;
        }
        #endregion

        public List<UserSetting> GetUsersSetting()
        {
            return Read<UserSetting>().ToList();
        }

        #region CRUD
        public Result<ApplicationUser> Create(ApplicationUser entity)
        {
            try
            {
                Create<ApplicationUser>(entity);
                SaveChanges();
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ApplicationUser> Update(ApplicationUser entity)
        {
            try
            {
                Update<ApplicationUser>(entity);
                SaveChanges();
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ApplicationUser> Delete(ApplicationUser entity)
        {
            try
            {
                Delete<ApplicationUser>(entity);
                SaveChanges();
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ApplicationUser> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<ApplicationUser>(entity);
                SaveChanges();
                return new Result<ApplicationUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ApplicationUser>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<string> DeleteUserAndRelatedData(string userId)
        {
            try
            {
                // Add any additional business logic or validation here
                // For example, you might want to check user permissions, logging, etc.
                return DAL.DeleteUserAndRelatedData(userId);
            }
            catch (Exception ex)
            {
                // Log or handle exceptions if necessary
                return Result<string>.FromException(ex, "An error occurred during user deletion.", null);
            }
        }
        #endregion

        public void CreateSettings(ApplicationUser user, CreateAccountViewModel model)
        {
            // Creating user role
            Work.Role.AddUserRole(user.Id, model.TypeId);

            //Add user to churchusers table
            var churchUser = new ChurchUser
            {
                Id = Utilities.GenerateUniqueId(),
                UserId = user.Id,
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
            };
            Work.Church.CreateUser(churchUser);

            //Create user settings
            var userSettings = new UserSetting()
            {
                Id = Utilities.GenerateUniqueId(),
                UserId = user.Id,
                PrimaryChurchId = SessionVariables.CurrentChurch.Id,
                PrimaryChurchCampusId = Work.Campus.GetByChurchId(SessionVariables.CurrentChurch.Id).Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                DarkModeEnabled = false,
                PaperlessGiving = false
            };
            Work.UserSetting.Create(userSettings);
        }

        public Result<string> TwoFactorEnabled(bool value, string userId)
        {
            var user = Work.User.Get(userId);

            if (user.EmailConfirmed || user.PhoneNumberConfirmed)
            {
                SessionVariables.CurrentUser.User.TwoFactorEnabled = value;
                user.TwoFactorEnabled = value;
                Work.User.Update(user);

                if (value)
                {
                    return new Result<string>
                    {
                        Message = "Two-factor authentication has been enabled.",
                        ResultType = ResultType.Success
                    };
                }

                return new Result<string>
                {
                    Message = "Two-factor authentication has been disabled.",
                    ResultType = ResultType.Success
                };
            }

            return new Result<string>
            {
                Message = "A phone number or email address must be verified before two-factor authentication can be enabled.",
                ResultType = ResultType.Failure
            };
        }
    }
}