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
    public class ServeTeamOperations : GenericRepository
    {
        public ServeTeamOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public ServeTeamMember Get(string id)
        {
            return Read<ServeTeamMember>().FirstOrDefault(x => x.Id == id);
        }

        public List<ServeTeamMember> GetAll(string churchId)
        {
            return Read<ServeTeamMember>().Where(x => x.ChurchId == churchId).ToList();
        }

        #region CRUD
        public Result<ServeTeamMember> Create(ServeTeamMember entity)
        {
            try
            {
                Create<ServeTeamMember>(entity);
                SaveChanges();
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ServeTeamMember> Update(ServeTeamMember entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<ServeTeamMember>(entity);
                SaveChanges();
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ServeTeamMember> Delete(ServeTeamMember entity)
        {
            try
            {
                Delete<ServeTeamMember>(entity);
                SaveChanges();
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ServeTeamMember> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<ServeTeamMember>(entity);
                SaveChanges();
                return new Result<ServeTeamMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ServeTeamMember>
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