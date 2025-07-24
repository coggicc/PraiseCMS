using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PraiseCMS.BusinessLayer
{
    public class ChurchOperations : GenericRepository
    {
        public ChurchOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Church Get(string id)
        {
            return Read<Church>().FirstOrDefault(x => x.Id == id);
        }

        public List<Church> Get(List<string> ids)
        {
            return Read<Church>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public Church GetByName(string name, bool uppercase = false)
        {
            if (uppercase)
            {
                name = name.ToUpper();
            }
            return Read<Church>().FirstOrDefault(x => x.Name.Equals(name));
        }

        public List<Church> GetAll()
        {
            return Read<Church>().OrderBy(x => x.Name).ToList();
        }

        public List<Church> GetAll(List<string> churchIds)
        {
            return Read<Church>().Where(x => churchIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();
        }

        public List<Church> GetAllActive()
        {
            return Read<Church>().Where(x => x.IsActive).OrderBy(x => x.Name).ToList();
        }

        public List<Church> GetAllByStateCode(string stateCode)
        {
            return Read<Church>().Where(x => x.PhysicalState == stateCode).OrderBy(x => x.Name).ToList();
        }

        public List<StateMapVM> GetStateCounts()
        {
            return (from c in Read<Church>()
                    where c.PhysicalState != null
                    group c by c.PhysicalState into g
                    select new StateMapVM { Name = g.Key, Count = g.Count() }).ToList();
        }

        public Result<Church> Create(Church entity)
        {
            try
            {
                Create<Church>(entity);
                SaveChanges();
                return new Result<Church>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Church>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Church> Delete(Church entity)
        {
            try
            {
                Delete<Church>(entity);
                SaveChanges();
                return new Result<Church>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Church>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Church> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Church>(entity);
                SaveChanges();
                return new Result<Church>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Church>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Church> Update(Church entity)
        {
            try
            {
                Update<Church>(entity);
                SaveChanges();
                return new Result<Church>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Church>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        //public void RegisterChurch(Church church)
        //{
        //    church.BillingSameAsPhysical = true;
        //    church.IsActive = true;
        //    church.IsMultiSite = false;
        //    church.CreatedDate = DateTime.Now;
        //    church.CreatedBy = SessionVariables.CurrentUser.User.Id;
        //    church.HasMerchantAccount = false;
        //    church.PaperlessGiving = true;
        //    Create<Church>(church);
        //    SaveChanges();

        //    var titheFund = new Fund() { Id = Utilities.GenerateUniqueId(), ChurchId = church.Id, Name = "Tithe", CreatedDate = DateTime.Now, CreatedBy = Constants.System };
        //    var disasterFund = new Fund() { Id = Utilities.GenerateUniqueId(), ChurchId = church.Id, Name = "Disaster Relief", CreatedDate = DateTime.Now, CreatedBy = Constants.System };
        //    var missionsFund = new Fund() { Id = Utilities.GenerateUniqueId(), ChurchId = church.Id, Name = "Missions", CreatedDate = DateTime.Now, CreatedBy = Constants.System };

        //    var funds = new List<Fund> { titheFund, disasterFund, missionsFund };
        //    Create<Fund>(funds);
        //    SaveChanges();

        //    var userSettings = Work.UserSetting.GetByUserId(SessionVariables.CurrentUser.User.Id);
        //    userSettings.PrimaryChurchId = church.Id;
        //    SaveChanges();

        //    var user = Work.User.Get(SessionVariables.CurrentUser.User.Id);
        //    Utilities.SetSessionVariables(user, church.Id);
        //}

        public ChurchDashboard GetDashboard(string stateCode)
        {
            var vm = new ChurchDashboard
            {
                Churches = stateCode.IsNotNullOrEmpty() ? Work.Church.GetAllByStateCode(stateCode) : GetAll()
            };

            var churchIds = vm.Churches.Select(q => q.Id).ToList();
            vm.Denominations = Work.Denomination.GetAll();
            vm.Subscriptions = Work.Subscription.GetActiveSubscriptionsByChurchIdList(churchIds);

            var prayerRequests = from pr in Db.PrayerRequests
                                 group pr by pr.ChurchId into grp
                                 select new { ChurchId = grp.Key, Count = grp.Count() };

            vm.ChurchPrayerRequests = prayerRequests.ToDictionary(x => x.ChurchId, x => x.Count);

            var giving = from pr in Db.Payments
                         group pr by pr.ChurchId into grp
                         select new { ChurchId = grp.Key, Sum = grp.Sum(x => x.Amount) };

            vm.ChurchGiving = giving.ToDictionary(x => x.ChurchId, x => x.Sum);

            return vm;
        }

        public bool ChurchExists(Church church)
        {
            return Work.Church.GetAll().Any(q =>
                q.Name.Equals(church.Name) && q.PhysicalCity.Equals(church.PhysicalCity)
                                           && q.PhysicalState.Equals(church.PhysicalState) &&
                                           q.PhysicalAddress1.Equals(church.PhysicalAddress1)
                                           && q.PhysicalZip.Equals(church.PhysicalZip));
        }

        public Church GetPraiseChurch()
        {
            return Read<Church>().FirstOrDefault(x => x.Name.Equals("Praise CMS") || x.Email.Equals("info@praisecms.com"));
        }

        public List<ChurchUserCount> CountUsers(List<string> churchIds)
        {
            var churchUsers = Work.Church.GetAllUsers(churchIds);

            return churchIds.Select(id => new ChurchUserCount { ChurchId = id, UserCount = churchUsers.Count(x => x.ChurchId == id) }).ToList();
        }

        #region Church User
        public List<ChurchUser> GetAllUsers()
        {
            return Read<ChurchUser>().ToList();
        }

        public List<ChurchUser> GetAllUsers(string churchId)
        {
            return Read<ChurchUser>().Where(x => x.ChurchId == churchId).ToList();
        }

        public List<ChurchUser> GetAllChurchUsersByUserId(string userId)
        {
            return Read<ChurchUser>().Where(x => x.UserId.Equals(userId)).ToList();
        }

        public List<ChurchUser> GetAllUsers(List<string> churchIds)
        {
            return Read<ChurchUser>().Where(x => churchIds.Contains(x.ChurchId)).ToList();
        }

        public ChurchUser GetUser(string id)
        {
            return Read<ChurchUser>().FirstOrDefault(x => x.Id == id);
        }

        public ChurchUser GetChurchUser(string churchId, string userId)
        {
            return Read<ChurchUser>().FirstOrDefault(x => x.ChurchId == churchId && x.UserId == userId);
        }
        #endregion

        #region CRUD Church User
        public Result<ChurchUser> CreateUser(ChurchUser entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<ChurchUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchUser>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchUser> CreateUser(IEnumerable<ChurchUser> entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<ChurchUser>
                {
                    List = entity.ToList(),
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchUser>
                {
                    List = entity.ToList(),
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchUser> DeleteUser(ChurchUser entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<ChurchUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchUser>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchUser> DeleteUser(string id)
        {
            try
            {
                var entity = GetUser(id);
                Delete(entity);
                SaveChanges();
                return new Result<ChurchUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchUser>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchUser> UpdateUser(ChurchUser entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<ChurchUser>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchUser>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public List<GoogleMapModel> GetLocation()
        {
            return Read<Church>().Where(x => x.Latitude != null && x.Longitude != null)
                .Select(x => new GoogleMapModel { Label = x.Name, Lat = x.Latitude.Value, Lng = x.Longitude.Value }).ToList();
        }

        public bool UpdateChurchSetting(string churchId, string userId, SettingsViewModel model)
        {
            model.Church.ModifiedDate = DateTime.Now;
            model.Church.ModifiedBy = userId;

            if (string.IsNullOrEmpty(model.Church.Country))
            {
                model.Church.Country = "US";
            }

            model.Church.ServiceAreaRequirements = model.Church.ServiceAreaRequirements.SplitToList().OrderBy(x => x).ToList().CombineListToString();
            model.Church.NoteCategories = model.Church.NoteCategories.SplitToList().OrderBy(x => x).ToList().CombineListToString();

            //What is the purpose of this field vs HasMerchantAccount?
            bool GivingAccountSetupCompleted = false;

            if (model.Church.BillingSameAsPhysical)
            {
                model.Church.BillingAddress1 = model.Church.PhysicalAddress1;
                model.Church.BillingAddress2 = model.Church.PhysicalAddress2;
                model.Church.BillingCity = model.Church.PhysicalCity;
                model.Church.BillingState = model.Church.PhysicalState;
                model.Church.BillingZip = model.Church.PhysicalZip;

                if (model.Church.PhysicalAddress1.IsNotNullOrEmpty() && model.Church.PhysicalCity.IsNotNullOrEmpty() && model.Church.PhysicalState.IsNotNullOrEmpty() && model.Church.PhysicalZip.IsNotNullOrEmpty())
                {
                    GivingAccountSetupCompleted = true;
                }
            }

            if (model.Church.HasMerchantAccount)
            {
                model.ChurchMerchantAccount = Read<ChurchMerchantAccount>().FirstOrDefault(x => x.ChurchId == churchId && x.Merchant == MerchantProviders.Nuvei);

                if (model.ChurchMerchantAccount.IsNull())
                {
                    model.ChurchMerchantAccount = new ChurchMerchantAccount();
                }

                model.ChurchMerchantAccount.Address = model.Church.BillingAddress1;
                model.ChurchMerchantAccount.Address2 = model.Church.BillingAddress2;
                model.ChurchMerchantAccount.City = model.Church.BillingCity;
                model.ChurchMerchantAccount.State = model.Church.BillingState;
                model.ChurchMerchantAccount.Zip = model.Church.BillingZip;

                if (model.Church.BillingAddress1.IsNotNullOrEmpty() && model.Church.BillingCity.IsNotNullOrEmpty() && model.Church.BillingState.IsNotNullOrEmpty() && model.Church.BillingZip.IsNotNullOrEmpty())
                {
                    GivingAccountSetupCompleted = true;
                }
            }

            if (GivingAccountSetupCompleted)
            {
                if (model.ChurchMerchantAccount.IsNotNull())
                {
                    GivingAccountSetupCompleted = model.ChurchMerchantAccount.AccountNumber.IsNotNullOrEmpty() && model.ChurchMerchantAccount.RoutingNumber.IsNotNullOrEmpty() && model.ChurchMerchantAccount.BankAccountType.IsNotNullOrEmpty() && model.ChurchMerchantAccount.TaxId.IsNotNullOrEmpty();
                }
                else
                {
                    GivingAccountSetupCompleted = false;
                }
            }

            Db.SaveChanges();

            return GivingAccountSetupCompleted;
        }

        public SettingsViewModel GetSettings(string churchId)
        {
            var settingsVM = new SettingsViewModel();
            var adminUsers = new List<ApplicationUser>();

            if (churchId.IsNotNullOrEmpty())
            {
                settingsVM.Church = Work.Church.Get(churchId);

                if (string.IsNullOrEmpty(settingsVM.Church.PrayerRequestReceivedText))
                {
                    settingsVM.Church.PrayerRequestReceivedText = Constants.DefaultPrayerRequestReceivedText;
                }

                if (string.IsNullOrEmpty(settingsVM.Church.PrayerRequestReceivedFollowUpText))
                {
                    settingsVM.Church.PrayerRequestReceivedFollowUpText = Constants.DefaultPrayerRequestReceivedFollowUpText;
                }

                settingsVM.Funds = Work.Fund.GetAll(churchId);
                settingsVM.Campuses = Work.Campus.GetAll(churchId);

                var merchantAccount = Work.ChurchMerchantAccount.GetByChurchId(churchId) ?? new ChurchMerchantAccount
                {
                    Id = Utilities.GenerateUniqueId()
                };
                settingsVM.ChurchMerchantAccount = merchantAccount;
                settingsVM.ChurchMerchantAccount.AccountNumber = !string.IsNullOrEmpty(merchantAccount.AccountNumber) ? merchantAccount.AccountNumber.Decrypt() : string.Empty;
                settingsVM.ChurchMerchantAccount.RoutingNumber = !string.IsNullOrEmpty(merchantAccount.RoutingNumber) ? merchantAccount.RoutingNumber.Decrypt() : string.Empty;

                settingsVM.Denominations = Read<Denomination>().OrderBy(x => x.Name).ToList();

                var usersWithRoles = DAL.GetAllChurchUsersByRole(churchId, Roles.Administrator);

                if (usersWithRoles != null)
                {
                    adminUsers.AddRange(usersWithRoles);

                    settingsVM.Administrators = adminUsers.OrderBy(x => x.FirstName).ToList();
                }
            }

            return settingsVM;
        }

        public async Task<Church> GetChurchByTwilioNumberAsync(string churchTwilioNumber)
        {
            return await Read<Church>().FirstOrDefaultAsync(x => x.ChurchTwilioNumber == churchTwilioNumber);
        }
    }
}