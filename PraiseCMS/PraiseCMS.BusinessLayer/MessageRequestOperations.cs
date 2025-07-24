using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class MessageRequestOperations : GenericRepository
    {
        public MessageRequestOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Message Requests
        public MessageRequest GetMessageRequest(string id)
        {
            return Read<MessageRequest>().FirstOrDefault(x => x.Id == id);
        }

        public List<MessageRequest> GetAllMessageRequests(List<string> ids)
        {
            return Read<MessageRequest>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<MessageRequest> GetAllMessageRequests(string churchId, bool includeArchived = false)
        {
            if (includeArchived)
            {
                return Read<MessageRequest>().Where(x => x.ChurchId == churchId && x.Archived).ToList();
            }
            else
            {
                return Read<MessageRequest>().Where(x => x.ChurchId == churchId).ToList();
            }
        }
        #endregion

        #region Message Request Categories
        public List<MessageRequestCategory> GetAllMessageRequestCategories()
        {
            return Read<MessageRequestCategory>().OrderBy(x => x.Name).ToList();
        }

        public MessageRequestCategory GetMessageRequestCategory(string id)
        {
            return Read<MessageRequestCategory>().FirstOrDefault(x => x.Id == id);
        }
        #endregion

        public MessageRequestViewModel GetCreateMessageRequestModel(string churchId)
        {
            var messageRequest = new MessageRequest()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = churchId,
                CreatedDate = DateTime.Now,
                CreatedBy = Constants.System
            };

            return new MessageRequestViewModel()
            {
                MessageRequest = messageRequest,
                MessageRequestCategories = Work.MessageRequest.GetAllMessageRequestCategories()
            };
        }

        public MessageRequestDashboardViewModel GetMessageRequestDashboard(string churchId, bool includeArchived = false)
        {
            var messageRequests = GetAllMessageRequests(churchId, includeArchived);
            var messageRequestCategories = GetAllMessageRequestCategories();

            foreach (var messageRequest in messageRequests)
            {
                messageRequest.SelectedCategory = messageRequestCategories
                    .FirstOrDefault(x => x.Id == messageRequest.MessageRequestCategoryId);
            }

            return new MessageRequestDashboardViewModel
            {
                MessageRequests = messageRequests,
                MessageRequestCategories = messageRequestCategories
            };
        }

        #region CRUD Message Requests
        public Result<MessageRequest> CreateMessageRequest(MessageRequest entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<MessageRequest>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<MessageRequest>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<MessageRequest> UpdateMessageRequest(MessageRequest entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser?.User.IsNotNullOrEmpty() == true ? SessionVariables.CurrentUser.User.Id : Constants.System;
                Update(entity);
                SaveChanges();
                return new Result<MessageRequest>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<MessageRequest>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void DeleteMessageRequest(string id)
        {
            var entity = GetMessageRequest(id);
            DeleteMessageRequest(entity);
        }

        public void DeleteMessageRequest(MessageRequest entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion

        #region CRUD Message Request Categories
        public void CreateMessageRequestCategory(MessageRequestCategory entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateMessageRequestCategory(MessageRequestCategory entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteMessageRequestCategory(string id)
        {
            var entity = GetMessageRequestCategory(id);
            DeleteMessageRequestCategory(entity);
        }

        public void DeleteMessageRequestCategory(MessageRequestCategory entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion
    }
}