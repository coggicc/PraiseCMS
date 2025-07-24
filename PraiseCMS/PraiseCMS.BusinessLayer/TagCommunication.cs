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
    public class TagCommunication : GenericRepository
    {
        public TagCommunication(ApplicationDbContext db, Work work) : base(db, work) { }

        public CommunicationHistory Get(string id)
        {
            return Read<CommunicationHistory>().FirstOrDefault(x => x.Id == id);
        }

        public List<CommunicationHistory> GetAllByTag(string tagId)
        {
            return Read<CommunicationHistory>().Where(x => x.TagId == tagId).ToList();
        }

        #region CRUD
        public Result<CommunicationHistory> Create(CommunicationHistory entity)
        {
            try
            {
                Create<CommunicationHistory>(entity);
                SaveChanges();
                return new Result<CommunicationHistory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationHistory>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationHistory> Delete(CommunicationHistory entity)
        {
            try
            {
                Delete<CommunicationHistory>(entity);
                SaveChanges();
                return new Result<CommunicationHistory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationHistory>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationHistory> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<CommunicationHistory>(entity);
                SaveChanges();
                return new Result<CommunicationHistory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationHistory>
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