using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class BlogPostOperations : GenericRepository
    {
        public BlogPostOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Blog Posts
        public BlogPost GetPost(string id)
        {
            return Read<BlogPost>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<BlogPostViewModel> GetAllPosts()
        {
            return Read<BlogPost>()
                .Join(Read<BlogCategory>(),
                      post => post.CategoryId,
                      category => category.Id,
                      (post, category) => new
                      {
                          PostId = post.Id,
                          post.Title,
                          post.Lead,
                          AuthorName = post.AuthorId,
                          post.Status,
                          PublishedDate = post.ModifiedDate ?? post.CreatedDate,
                          CategoryTitle = category.Title
                      })
                .ToList()
                .Select(post => new BlogPostViewModel
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Lead = post.Lead,
                    AuthorName = post.AuthorName,
                    Status = post.Status,
                    PublishedDate = post.PublishedDate.ToShortDateString(),
                    CategoryTitle = post.CategoryTitle
                })
                .ToList();
        }

        public List<BlogPostViewModel> GetAllActivePosts(bool isPublished = true)
        {
            var viewModelList = new List<BlogPostViewModel>();

            var queryResult = from bp in Read<BlogPost>()
                              join bc in Read<BlogCategory>() on bp.CategoryId equals bc.Id
                              join bpt in Read<BlogPostTag>() on bp.Id equals bpt.BlogPostId into bptGroup
                              from bpt in bptGroup.DefaultIfEmpty()
                              join bt in Read<BlogTag>() on bpt.BlogTagId equals bt.Id into btGroup
                              from bt in btGroup.DefaultIfEmpty()
                              where bp.Status == (isPublished ? 1 : 0)
                              select new
                              {
                                  BlogId = bp.Id,
                                  BlogPostTitle = bp.Title,
                                  BlogPostLead = bp.Lead,
                                  AuthorName = bp.AuthorId,
                                  CategoryTitle = bc.Title,
                                  PublishedDate = bc.ModifiedDate ?? bc.CreatedDate,
                                  TagTitle = bt.Title
                              };

            foreach (var group in queryResult.GroupBy(row => new { row.BlogId, row.BlogPostTitle, row.BlogPostLead, row.AuthorName, row.PublishedDate, row.CategoryTitle }))
            {
                var viewModel = new BlogPostViewModel
                {
                    PostId = group.Key.BlogId,
                    Title = group.Key.BlogPostTitle,
                    Lead = group.Key.BlogPostLead,
                    AuthorName = group.Key.AuthorName,
                    CategoryTitle = group.Key.CategoryTitle,
                    PublishedDate = group.Key.PublishedDate.ToShortDateString(),
                    TagTitles = group.Where(row => row.TagTitle != null).Select(row => row.TagTitle).ToList()
                };
                viewModelList.Add(viewModel);
            }

            return viewModelList;
        }

        public List<BlogPost> GetAllPosts(DateRange dateRange)
        {
            return Read<BlogPost>()
                .Where(x => (x.CreatedDate.Date >= dateRange.StartDate.Date && x.CreatedDate.Date <= dateRange.EndDate.Date)
                    || (x.ModifiedDate.HasValue && x.ModifiedDate.Value.Date >= dateRange.StartDate.Date && x.ModifiedDate.Value.Date <= dateRange.EndDate.Date)
                    || (!x.ModifiedDate.HasValue && x.CreatedDate.Date >= dateRange.StartDate.Date && x.CreatedDate.Date <= dateRange.EndDate.Date))
                .ToList();
        }

        public List<BlogPost> GetAllPostsByStatuses(List<BlogStatuses> statuses)
        {
            return Read<BlogPost>().Where(post => statuses.Contains((BlogStatuses)post.Status)).ToList();
        }

        public List<BlogPost> GetLatestPosts(int count)
        {
            return Read<BlogPost>().OrderByDescending(x => x.ModifiedDate ?? x.CreatedDate).Take(count).ToList();
        }

        public BlogPost GetCreateEditModel(string blogPostId)
        {
            return !string.IsNullOrEmpty(blogPostId) ? Work.BlogPost.GetPost(blogPostId) : new BlogPost();
        }
        #endregion

        #region CRUD
        public Result<BlogPost> CreatePost(BlogPost entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return Result<BlogPost>.FromAction(entity);
            }
            catch (Exception ex)
            {
                return Result<BlogPost>.FromException(ex, Constants.CreateExceptionMessage, entity);
            }
        }

        public Result<BlogPost> UpdatePost(BlogPost entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<BlogPost>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogPost>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogPost> DeletePost(string id)
        {
            try
            {
                var entity = GetPost(id);
                Delete(entity);
                SaveChanges();
                return new Result<BlogPost>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogPost>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<BlogPost> DeletePost(BlogPost entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<BlogPost>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<BlogPost>
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