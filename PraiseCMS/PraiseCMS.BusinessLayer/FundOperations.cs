using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PraiseCMS.BusinessLayer
{
    public class FundOperations : GenericRepository
    {
        public FundOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region CRUD
        public Result<Fund> Create(Fund entity)
        {
            try
            {
                Create<Fund>(entity);
                SaveChanges();
                return new Result<Fund>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Fund>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<IEnumerable<Fund>> Create(IEnumerable<Fund> entities)
        {
            try
            {
                // Call the Create method of the base class for each entity
                base.Create(entities);

                // Save changes after all entities are added
                SaveChanges();

                return new Result<IEnumerable<Fund>>
                {
                    Data = entities,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<IEnumerable<Fund>>
                {
                    Data = entities,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void CreateDefaultFunds(string churchId)
        {
            // Create a list to store the funds to be created
            var fundsToCreate = new List<Fund>();

            foreach (var fundName in GivingFunds.Items)
            {
                // Check if the current fund is one of the desired funds
                if (fundName == GivingFunds.TithesAndOfferings || fundName == GivingFunds.General || fundName == GivingFunds.Missions)
                {
                    var id = Utilities.GenerateUniqueId();
                    // Create a new Fund entity for the current fund
                    var newFund = new Fund
                    {
                        Id = id,
                        ChurchId = churchId,
                        Name = fundName,
                        Description = Constants.GivingFundDescriptions.ContainsKey(fundName) ? Constants.GivingFundDescriptions[fundName] : null,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "SYSTEM",
                        IsTaxDeductible = true,
                        IsDefaultFund = fundName == GivingFunds.TithesAndOfferings,
                        QRCodeLink = $"/GivingWorkflow/StartGiving?Id={churchId}&selectedFundId={id}"
                    };

                    // Add the new fund to the list of funds to be created
                    fundsToCreate.Add(newFund);
                }
            }

            // Call the Create method to create all the funds
            var result = Create(fundsToCreate);

            // Check the result and handle as needed
            if (result.ResultType != ResultType.Success)
            {
                // Handle error case
                // Log the error, throw an exception, etc.
            }
        }

        public Result<Fund> Update(Fund entity)
        {
            try
            {
                Update<Fund>(entity);
                SaveChanges();
                return new Result<Fund>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Fund>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void Update(IEnumerable<Fund> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    Update<Fund>(entity);
                }
                SaveChanges();
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
        }

        public Result<Fund> Delete(Fund entity)
        {
            try
            {
                Delete<Fund>(entity);
                SaveChanges();
                return new Result<Fund>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Fund>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Fund> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Fund>(entity);
                SaveChanges();
                return new Result<Fund>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Fund>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public Fund Get(string id)
        {
            return Read<Fund>().FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public Fund GetByName(string churchId, string name)
        {
            return Read<Fund>().FirstOrDefault(x => x.ChurchId == churchId && x.Name.Contains(name) && !x.IsDeleted);
        }

        public async Task<Fund> GetByNameAsync(string churchId, string name)
        {
            return await Read<Fund>().FirstOrDefaultAsync(x => x.ChurchId == churchId && x.Name.Contains(name) && !x.IsDeleted);
        }

        public List<Fund> GetAll(string churchId, IEnumerable<string> ids = null, bool includeDeleted = false, bool hidden = false, bool closed = false)
        {
            var query = Read<Fund>().AsQueryable();

            // Filter by churchId
            if (!string.IsNullOrEmpty(churchId))
            {
                query = query.Where(x => x.ChurchId == churchId);
            }

            // Filter by IDs if provided
            if (ids?.Any() == true)
            {
                query = query.Where(x => ids.Contains(x.Id));
            }

            // Exclude deleted funds unless includeDeleted is true
            if (!includeDeleted)
            {
                query = query.Where(x => !x.IsDeleted);
            }

            // Apply hidden filter if provided
            if (!hidden)
            {
                query = query.Where(x => !x.Hidden);
            }

            // Apply closed filter if provided
            if (!closed)
            {
                query = query.Where(x => !x.Closed);
            }

            return query.OrderBy(x => x.Name).ToList();
        }

        public List<SelectListItem> GetDigitalFundsByChurch(string churchId)
        {
            return Read<Fund>().Where(x => x.ChurchId.Equals(churchId) && !x.Hidden && !x.Closed && !x.IsDeleted).OrderBy(x => x.Name).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id,
                Selected = s.Name.Equals(GivingFunds.TithesAndOfferings)
            }).ToList();
        }

        public bool EnableCloseOrHidden(string churchId, string fundId = null)
        {
            var funds = Read<Fund>().Where(x => x.ChurchId == churchId && !x.IsDeleted).OrderBy(x => x.Name).ToList();

            if (fundId.IsNotNullOrEmpty())
            {
                funds = funds.Where(q => !q.Id.Equals(fundId)).ToList();
            }

            return funds.Any(q => !q.Hidden && !q.Closed);
        }

        public void CloseExpiredFunds()
        {
            var now = DateTime.Now;
            var currentDate = now.Date; // Store the date outside the LINQ query

            if (now.TimeOfDay > new TimeSpan(00, 00, 00) && now.TimeOfDay <= new TimeSpan(01, 30, 00))
            {
                var fundsToUpdate = Read<Fund>()
                    .Where(q => q.ExpirationDate != null && !q.Closed && DbFunctions.TruncateTime(q.ExpirationDate) < currentDate)
                    .ToList();

                if (fundsToUpdate.Any())
                {
                    fundsToUpdate.ForEach(fund => fund.Closed = true);
                    Update(fundsToUpdate);
                }
            }
        }
    }
}