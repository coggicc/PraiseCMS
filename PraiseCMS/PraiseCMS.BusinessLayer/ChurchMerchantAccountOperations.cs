using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PraiseCMS.BusinessLayer
{
    public class ChurchMerchantAccountOperations : GenericRepository
    {
        private readonly ChurchOperations _churchOperations;

        public ChurchMerchantAccountOperations(ApplicationDbContext db, Work work, ChurchOperations churchOperations)
        : base(db, work)
        {
            _churchOperations = churchOperations;
        }

        public ChurchMerchantAccount Get(string id)
        {
            return Read<ChurchMerchantAccount>().FirstOrDefault(x => x.Id == id);
        }

        public ChurchMerchantAccount GetByChurchId(string churchId)
        {
            return Read<ChurchMerchantAccount>().FirstOrDefault(x => x.ChurchId == churchId && x.IsActive && x.Merchant == MerchantProviders.Nuvei);
        }

        public ChurchMerchantAccount GetPraiseChurchAccount()
        {
            // Get the Praise CMS church
            var praiseChurch = _churchOperations.GetPraiseChurch();

            // Check if the Praise CMS church is found
            if (praiseChurch != null)
            {
                // Query the ChurchMerchantAccount for the Praise CMS church
                return Read<ChurchMerchantAccount>().FirstOrDefault(x => x.ChurchId == praiseChurch.Id && x.IsActive && x.Merchant == MerchantProviders.Nuvei);
            }

            // If Praise CMS church is not found, return null or handle as needed
            return null;
        }

        #region CRUD
        public Result<ChurchMerchantAccount> Create(ChurchMerchantAccount entity)
        {
            try
            {
                Create<ChurchMerchantAccount>(entity);
                SaveChanges();
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchMerchantAccount> Update(ChurchMerchantAccount entity)
        {
            try
            {
                Update<ChurchMerchantAccount>(entity);
                SaveChanges();
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchMerchantAccount> Delete(ChurchMerchantAccount entity)
        {
            try
            {
                Delete<ChurchMerchantAccount>(entity);
                SaveChanges();
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ChurchMerchantAccount> Delete(string id)
        {
            try
            {
                var entity = Work.ChurchMerchantAccount.Get(id);
                Delete<ChurchMerchantAccount>(entity);
                SaveChanges();
                return new Result<ChurchMerchantAccount>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchMerchantAccount>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public ChuchGivingAccountViewModel GetChurchGivingAccountSettings(string churchId)
        {
            var viewModel = new ChuchGivingAccountViewModel();

            if (churchId.IsNotNullOrEmpty())
            {
                viewModel.Church = Work.Church.Get(churchId);

                var merchantAccount = Work.ChurchMerchantAccount.GetByChurchId(churchId);

                if (merchantAccount != null)
                {
                    viewModel.Id = merchantAccount.Id;
                    viewModel.Merchant = merchantAccount.Merchant;
                    viewModel.MerchantAccountId = merchantAccount.MerchantAccountId;
                    viewModel.ChurchId = merchantAccount.ChurchId;
                    viewModel.IsActive = merchantAccount.IsActive;
                    viewModel.BusinessType = merchantAccount.BusinessType;
                    viewModel.Address = merchantAccount.Address;
                    viewModel.Address2 = merchantAccount.Address2;
                    viewModel.City = merchantAccount.City;
                    viewModel.State = merchantAccount.State;
                    viewModel.Zip = merchantAccount.Zip;
                    viewModel.Country = merchantAccount.Country;
                    viewModel.Phone = merchantAccount.Phone;
                    viewModel.Email = merchantAccount.Email;
                    viewModel.TaxId = merchantAccount.TaxId;
                    viewModel.BankAccountType = merchantAccount.BankAccountType;
                    viewModel.AccountNumber = merchantAccount.AccountNumber.Decrypt();
                    viewModel.RoutingNumber = merchantAccount.RoutingNumber.Decrypt();
                    viewModel.BusinessWebsite = merchantAccount.BusinessWebsite;
                    viewModel.RespContactFirstName = merchantAccount.RespContactFirstName;
                    viewModel.RespContactLastName = merchantAccount.RespContactLastName;
                    viewModel.RespContactPhone = merchantAccount.RespContactPhone;
                    viewModel.RespContactEmail = merchantAccount.RespContactEmail;
                    viewModel.RespContactDLN = merchantAccount.RespContactDLN;
                    viewModel.RespContactAddress1 = merchantAccount.RespContactAddress1;
                    viewModel.RespContactAddress2 = merchantAccount.RespContactAddress2;
                    viewModel.RespContactCity = merchantAccount.RespContactCity;
                    viewModel.RespContactState = merchantAccount.RespContactState;
                    viewModel.RespContactZip = merchantAccount.RespContactZip;
                    viewModel.RespContactSSN = merchantAccount.RespContactSSN.Decrypt();
                    viewModel.RespContactDOB = merchantAccount.FormattedRespContactDOB;
                    viewModel.CardProcessingFee = merchantAccount.CardProcessingFee;
                    viewModel.ACHProcessingFee = merchantAccount.ACHProcessingFee;
                    viewModel.ApiUsername = merchantAccount.ApiUsername.Decrypt();
                    viewModel.ApiPassword = merchantAccount.ApiPassword.Decrypt();
                    viewModel.Username = merchantAccount.Username.Decrypt();
                    viewModel.Password = merchantAccount.Password.Decrypt();
                }
            }

            return viewModel;
        }
    }
}