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
    public class BlogTagOperations : GenericRepository
    {
        public BlogTagOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Blog Tags
        public BlogTag GetTag(string id)
        {
            return Read<BlogTag>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<BlogTag> GetAllTags(bool isPublished = true)
        {
            IQueryable<BlogTag> tagsQuery = Read<BlogTag>();

            if (isPublished)
            {
                tagsQuery = tagsQuery.Where(x => x.Status == (int)BlogStatuses.Publish);
            }

            return tagsQuery.OrderBy(x => x.Title).ToList();
        }
        #endregion

        #region CRUD
        public Result<BlogTag> CreateTag(BlogTag entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<BlogTag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogTag>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogTag> UpdateTag(BlogTag entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<BlogTag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogTag>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogTag> DeleteTag(string id)
        {
            try
            {
                var entity = GetTag(id);
                Delete(entity);
                SaveChanges();
                return new Result<BlogTag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogTag>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogTag> DeleteTag(BlogTag entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<BlogTag>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogTag>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion
    }
}