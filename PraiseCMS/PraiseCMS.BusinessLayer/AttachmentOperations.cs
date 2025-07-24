using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class AttachmentOperations : GenericRepository
    {
        public AttachmentOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public AttachmentSD Get(string id)
        {
            return Read<AttachmentSD>().FirstOrDefault(x => x.Id == id);
        }

        public AttachmentsViewModel GetAll()
        {
            var result = new AttachmentsViewModel();
            var data = (from att in Db.Attachments
                        join usr in Db.Users on att.CreatedBy equals usr.Id
                        where att.ChurchId == SessionVariables.CurrentChurch.Id
                        select new AttachmentView
                        {
                            Attachment = att,
                            User = usr
                        }).ToList();

            result.Attachments = data.Select(x => x.Attachment).ToList();
            result.Users = data.Select(x => x.User).ToList();

            return result;
        }

        #region CRUD
        public Result<AttachmentSD> Create(AttachmentSD entity)
        {
            try
            {
                Create<AttachmentSD>(entity);
                SaveChanges();
                return new Result<AttachmentSD>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<AttachmentSD>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<AttachmentSD> Update(SingleAttachmentViewModel model, string newFileName)
        {
            try
            {
                var att = Get(model.Attachment.Id);
                att.Category = model.Attachment.Category;
                att.ChurchId = SessionVariables.CurrentChurch.Id;
                att.CreatedBy = SessionVariables.CurrentUser.User.Id;
                att.CreatedDate = DateTime.Now;
                att.FileName = !string.IsNullOrEmpty(newFileName) ? model.Attachment.FileName : att.FileName;
                att.Name = !string.IsNullOrEmpty(model.Attachment.Name) ? model.Attachment.Name : model.Attachment.FileName;
                att.Notes = model.Attachment.Notes;
                att.Type = model.Attachment.Type;
                att.TypeId = model.Attachment.TypeId;
                SaveChanges();

                return new Result<AttachmentSD>
                {
                    Data = att,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<AttachmentSD>
                {
                    Data = model.Attachment,
                    Exception = ex,
                    Message = "There was a problem updating the record.",
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<AttachmentSD> Delete(AttachmentSD entity)
        {
            try
            {
                Delete<AttachmentSD>(entity);
                SaveChanges();
                return new Result<AttachmentSD>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<AttachmentSD>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public void MoveUp(AttachmentSD attachment)
        {
            var previousAttachment = GetPreviousOne(attachment.Id);

            if (previousAttachment != null)
            {
                var previousSortOrder = Utilities.Clone(previousAttachment.SortOrder);
                previousAttachment.SortOrder = attachment.SortOrder;
                attachment.SortOrder = previousSortOrder;
                SaveChanges();
            }
        }

        public void MoveAttachmentDown(AttachmentSD attachment)
        {
            var nextAttachment = GetNextOne(attachment.Id);

            if (nextAttachment != null)
            {
                var nextSortOrder = Utilities.Clone(nextAttachment.SortOrder);
                nextAttachment.SortOrder = attachment.SortOrder;
                attachment.SortOrder = nextSortOrder;
                SaveChanges();
            }
        }

        public AttachmentSD GetPreviousOne(string id)
        {
            var attachment = Work.Attachment.Get(id);
            return Read<AttachmentSD>().FirstOrDefault(x => x.SortOrder == attachment.SortOrder - 1);
        }

        public AttachmentSD GetNextOne(string id)
        {
            var attachment = Work.Attachment.Get(id);
            return Read<AttachmentSD>().FirstOrDefault(x => x.SortOrder == attachment.SortOrder + 1);
        }

        public AttachmentsViewModel GetAttachmentsView(string type, string typeId, string returnUrl)
        {
            var view = new AttachmentsViewModel(type, typeId, returnUrl) { Attachments = new List<AttachmentSD>(), Users = new List<ApplicationUser>() };
            view.Attachments.Where(x => x.Type == type && x.TypeId == typeId).OrderBy(x => x.SortOrder).ThenByDescending(x => x.CreatedDate).ToList();
            var userIds = view.Attachments.Select(x => x.CreatedBy).Distinct().ToList();
            view.Users = Work.User.GetAll(userIds);

            return view;
        }

        public AttachmentSD GetLastAttachmentFor(string type, string typeId)
        {
            return Read<AttachmentSD>().Where(x => x.Type == type && x.TypeId == typeId).OrderByDescending(x => x.SortOrder).Take(1).FirstOrDefault();
        }

        public List<AttachmentSD> GetAll(string userId, string type)
        {
            return Db.Attachments.Where(x => x.Type == type && x.TypeId == userId).OrderByDescending(x => x.CreatedDate).ToList();
        }
    }
}