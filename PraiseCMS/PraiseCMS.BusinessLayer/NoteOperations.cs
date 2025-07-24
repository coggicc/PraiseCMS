using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class NoteOperations : GenericRepository
    {
        public NoteOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Note Get(string id)
        {
            return Read<Note>().FirstOrDefault(x => x.Id == id);
        }

        public Note GetByChurchAndType(string churchId, string type)
        {
            return Read<Note>().FirstOrDefault(x => x.ChurchId == churchId && x.Type == type);
        }

        public List<Note> GetAll(string churchId)
        {
            return Read<Note>().Where(x => x.ChurchId == churchId).OrderByDescending(q => q.CreatedDate).ToList();
        }

        public List<Note> GetAllByUserId(string userId)
        {
            return Read<Note>().Where(x => x.TypeId == userId).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public NotesViewModel GetNotesViewModel(string type, string typeId)
        {
            var view = new NotesViewModel(type, typeId) { Notes = new List<Note>(), Users = new List<ApplicationUser>() };

            view.Notes = Db.Notes.Where(x => x.Type == type && x.TypeId == typeId).ToList();
            var userids = view.Notes.Select(x => x.TypeId).ToList();
            userids = userids.Distinct().ToList();
            view.Users = Db.Users.Where(x => userids.Contains(x.Id)).ToList();

            return view;
        }

        #region CRUD
        public Result<Note> Create(Note entity)
        {
            try
            {
                Create<Note>(entity);
                SaveChanges();
                return new Result<Note>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Note>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Note> Update(Note entity)
        {
            try
            {
                Update<Note>(entity);
                SaveChanges();
                return new Result<Note>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Note>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Note> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Note>(entity);
                SaveChanges();
                return new Result<Note>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Note>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Note> Delete(Note entity)
        {
            try
            {
                Delete<Note>(entity);
                SaveChanges();
                return new Result<Note>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Note>
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