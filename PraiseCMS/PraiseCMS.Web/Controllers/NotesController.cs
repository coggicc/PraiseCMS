using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    public class NotesController : BaseController
    {
        [HttpGet]
        public ActionResult _Widget(string type, string typeId)
        {
            var model = GetNotesView(type, typeId);
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _AddNoteToWidget(string type, string typeId, string note, string category)
        {
            //Insert the new note
            var noteSd = new Note
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                Type = type,
                TypeId = typeId,
                Description = note,
                Category = !string.IsNullOrEmpty(category) ? category : "Uncategorized",
                ChurchId = SessionVariables.CurrentChurch.Id
            };

            work.Note.Create(noteSd);

            //Return partial back to replace the div
            var model = GetNotesView(type, typeId);

            return JavaScript("$('.note-widget-loading-" + typeId + "').show();$('#note-widget-" + typeId + "').load('/notes/_widget?type=" + type + "&typeId=" + typeId + "');");
        }

        [HttpGet]
        public ActionResult _Edit(string id)
        {
            var model = work.Note.Get(id);
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _Edit(Note model)
        {
            var note = work.Note.Get(model.Id);
            note.Category = model.Category;
            note.Description = model.Description;
            work.Note.Update(model);

            return JavaScript("$('.note-widget-loading-" + note.TypeId + "').show();$('#note-widget-" + note.TypeId + "').load('/notes/_widget?type=" + note.Type + "&typeId=" + note.TypeId + "');$('#ajax-modal').modal('hide');");
        }

        [HttpGet]
        public ActionResult _Clear(string id)
        {
            var note = work.Note.Get(id);
            note.Deleted = true;
            note.DeletedBy = SessionVariables.CurrentUser.User.Id;
            note.DeletedDate = DateTime.Now;

            work.Note.Update(note);

            return JavaScript("$('.note-content-" + id + "').addClass('deleted');$('.note-action-" + id + "').addClass('deleted');$('.note-action-" + id + "').hide();$('.note-meta-" + id + "').append('<span>| Cleared by " + SessionVariables.CurrentUser.User.FullName + " on " + DateTime.Now.ToShortDateString() + "</span>');");
        }

        [HttpGet]
        public ActionResult _Delete(string id)
        {
            work.Note.Delete(id);
            return JavaScript("$('.note-" + id + "').hide();");
        }

        public NotesViewModel GetNotesView(string type, string typeId)
        {
            return work.Note.GetNotesViewModel(type, typeId);
        }
    }
}