using Microsoft.AspNet.Identity;
using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.BusinessLayer
{
    public class AccountOperations : GenericRepository
    {
        public AccountOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public List<ApplicationUser> GetAll()
        {
            return Read<ApplicationUser>().ToList();
        }

        public Result<bool> CombineExternalRegistration(GivingRegisterModel model)
        {
            var hasher = new PasswordHasher();

            try
            {
                var user = new ApplicationUser();

                if (model.LoginProvider == LoginProvider.Facebook || model.LoginProvider == LoginProvider.Google)
                {
                    user = Work.User.GetByExternalProviderId(model.ExternalProviderId);
                    model.Password = $"abcD@{model.ExternalProviderId}";
                }
                else
                {
                    user = null;
                }

                if (user.IsNull())
                {
                    var result = DAL.InsertUser(new ApplicationUser
                    {
                        Id = model.Id,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                        UserName = model.Username,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        PasswordHash = hasher.HashPassword(model.Password),
                        SecurityStamp = Utilities.GenerateUniqueId(),
                        PhoneNumber = model.Phone.IsNullOrEmpty() ? null : model.Phone,
                        PhoneVerificationCode = Utilities.GenerateVerificationCode(),
                        ExternalProvider = model.LoginProvider.IsNotNullOrEmpty() ? model.LoginProvider : null,
                        ExternalProviderId = model.ExternalProviderId.IsNotNullOrEmpty() ? model.ExternalProviderId : null
                    });

                    if (result)
                    {
                        user = Work.User.Get(model.Id);
                        DAL.InsertUserRoleByName(model.Id, Roles.Donor);
                        Work.UserSetting.Create(new UserSetting
                        {
                            Id = Utilities.GenerateUniqueId(),
                            UserId = model.Id,
                            PrimaryChurchId = "unregister",
                            PrimaryChurchCampusId = "unregister",
                            CreatedDate = DateTime.Now,
                            CreatedBy = Constants.System,
                            ProfileImage = model.ProfileImage
                        });
                    }
                }
                else
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.Phone.IsNullOrEmpty() ? null : model.Phone;
                    user.UserName = model.Id;
                    user.EmailConfirmed = true;

                    var uSettings = Work.UserSetting.Get(user.Id);
                    if (uSettings != null) uSettings.ProfileImage = model.ProfileImage;
                    Db.SaveChanges();
                }

                if (user.IsNotNull())
                {
                    Utilities.SetSessionVariables(user, model.ChurchId);
                }

                return new Result<bool> { Data = true, ResultType = ResultType.Success };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<bool> { Data = false, ResultType = ResultType.Exception, Exception = ex };
            }
        }

        public ApplicationUser ConfirmEmailByIdandCode(string userId, string code)
        {
            var user = Work.User.GetBySecurityStamp(userId, code);

            if (user.IsNotNull())
            {
                user.IsActive = true;
                user.EmailConfirmed = true;
                user.SecurityStamp = Utilities.GenerateUniqueId();

                return Work.User.Update(user).Data;
            }

            return null;
        }
    }
}