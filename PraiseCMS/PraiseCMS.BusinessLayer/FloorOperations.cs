using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class FloorOperations : GenericRepository
    {
        public FloorOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Floor Get(string id)
        {
            return Read<Floor>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<Floor> GetAll(string churchId, IEnumerable<string> ids = null, bool includeBuilding = false)
        {
            IQueryable<Floor> query = Read<Floor>().Where(x => x.ChurchId.Equals(churchId));

            if (ids?.Any() == true)
            {
                query = query.Where(x => ids.Contains(x.Id));
            }

            var floors = query.OrderBy(x => x.FloorName).ToList();

            if (includeBuilding && floors.Any())
            {
                var building = Work.Building.GetAll(SessionVariables.CurrentChurch.Id, floors.Select(x => x.BuildingId));
                floors.ForEach(x => x.Building = building.Find(a => a.Id.Equals(x.BuildingId)));
            }

            return floors;
        }

        #region CRUD
        public Result<Floor> CreateFloor(Floor entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<Floor>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Floor>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Floor> UpdateFloor(Floor entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update(entity);
                SaveChanges();
                return new Result<Floor>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Floor>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Floor> DeleteFloor(Floor entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<Floor>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Floor>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Floor> DeleteFloor(string id)
        {
            try
            {
                var entity = Get(id);
                Delete(entity);
                SaveChanges();
                return new Result<Floor>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Floor>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion
    }
}