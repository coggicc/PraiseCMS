using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class BuildingOperations : GenericRepository
    {
        public BuildingOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Building Get(string id)
        {
            return Read<Building>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<Building> GetAll(string churchId)
        {
            return Read<Building>().Where(x => x.ChurchId == churchId).OrderBy(x => x.BuildingName).ToList();
        }

        public List<Building> GetAll(string churchId, IEnumerable<string> ids)
        {
            return Read<Building>().Where(x => ids.Contains(x.Id) && x.ChurchId.Equals(churchId)).OrderBy(x => x.BuildingName).ToList();
        }

        public List<Building> GetAllByCampus(string campusId)
        {
            return Read<Building>().Where(x => x.CampusId == campusId).OrderBy(x => x.BuildingName).ToList();
        }

        #region CRUD
        public Result<Building> CreateBuilding(Building entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<Building>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Building>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Building> UpdateBuilding(Building entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<Building>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Building>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Building> DeleteBuilding(Building entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<Building>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Building>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Building> DeleteBuilding(string id)
        {
            try
            {
                var entity = Get(id);
                Delete(entity);
                SaveChanges();
                return new Result<Building>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Building>
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