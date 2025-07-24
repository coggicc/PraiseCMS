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
    public class BlogCategoryOperations : GenericRepository
    {
        public BlogCategoryOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Blog Categories
        public BlogCategory GetCategory(string id)
        {
            return Read<BlogCategory>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<BlogCategory> GetAllCategories(bool isPublished = true)
        {
            IQueryable<BlogCategory> categoriesQuery = Read<BlogCategory>();

            if (isPublished)
            {
                categoriesQuery = categoriesQuery.Where(x => x.Status == (int)BlogStatuses.Publish);
            }

            return categoriesQuery.OrderBy(x => x.Title).ToList();
        }

        public List<SelectListItems> GetBlogCategoryDropdownList()
        {
            var blogCategories = GetAllCategories();

            return blogCategories.Select(category => new SelectListItems
            {
                Text = category.Title,
                Value = category.Id
            })
            .OrderBy(q => q.Text)
            .ToList();
        }
        #endregion

        #region CRUD
        public Result<BlogCategory> CreateCategory(BlogCategory entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<BlogCategory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogCategory>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogCategory> UpdateCategory(BlogCategory entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<BlogCategory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogCategory>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogCategory> DeleteCategory(string id)
        {
            try
            {
                var entity = GetCategory(id);
                Delete(entity);
                SaveChanges();
                return new Result<BlogCategory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogCategory>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogCategory> DeleteCategory(BlogCategory entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<BlogCategory>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogCategory>
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