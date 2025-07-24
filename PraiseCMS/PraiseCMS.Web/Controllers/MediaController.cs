using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "8284256802208c71b9be9240e68f6b")]
    public class MediaController : BaseController
    {
        public ActionResult Index()
        {
            var vm = work.Media.GetDashboard(SessionVariables.CurrentChurch.Id);
            return View(vm);
        }

        public ActionResult SermonNotes(string sermonId)
        {
            var model = work.Media.GetSermonNote(sermonId);
            return PartialView("_StandardNotesPartial", model);
        }
        public ActionResult SermonFilledNotes(string sermonId)
        {
            var model = work.Media.GetSermonNote(sermonId);
            return PartialView("_StandardFilledNotesPartial", model);
        }

        #region Sermon Series

        public ActionResult SeriesList()
        {
            var seriesList = work.Media.GetAllSermonSeries(SessionVariables.CurrentChurch.Id).OrderByDescending(x => x.CreatedDate).ToList() ?? new List<SermonSeries>();
            return View(seriesList);
        }

        public ActionResult Series(string seriesId)
        {
            var model = work.Media.GetSeriesView(SessionVariables.CurrentChurch.Id, seriesId);
            return View(model);
        }

        [HttpGet]
        public ActionResult _AddSeries()
        {
            var sermonSeriesViewModel = new SermonSeriesViewModel
            {
                Series = new SermonSeries
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                },
                TopicsList = work.Media.GetAllSermonTopic()
            };

            return PartialView("_CreateEditSermonSeries", sermonSeriesViewModel);
        }

        [HttpPost]
        public ActionResult _AddSeries(SermonSeriesViewModel sermonSeriesViewModel)
        {
            if (ModelState.IsValid)
            {
                work.Media.CreateSermonSeries(sermonSeriesViewModel.Series);

                return AjaxRedirectTo("/media/serieslist");
            }

            return PartialView("_CreateEditSermonSeries", sermonSeriesViewModel);
        }

        [HttpGet]
        public ActionResult _EditSeries(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sermonSeries = work.Media.GetSermonSeries(id);

            if (sermonSeries == null)
            {
                return HttpNotFound();
            }

            var sermonSeriesViewModel = new SermonSeriesViewModel
            {
                Series = sermonSeries,
                TopicsList = work.Media.GetAllSermonTopic()
            };

            return PartialView("_CreateEditSermonSeries", sermonSeriesViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditSeries(SermonSeriesViewModel sermonSeriesViewModel)
        {
            if (ModelState.IsValid)
            {
                sermonSeriesViewModel.Series.ModifiedDate = DateTime.Now;
                sermonSeriesViewModel.Series.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Media.UpdateSermonSeries(sermonSeriesViewModel.Series);

                return AjaxRedirectTo("/media/serieslist");
            }

            return PartialView("_CreateEditSermonSeries", sermonSeriesViewModel);
        }

        [HttpGet]
        public ActionResult DeleteSeries(string id)
        {
            work.Media.DeleteSeries(id);

            return RedirectToAction("index");
        }
        #endregion

        #region Sermons

        public ActionResult Sermon(string sermonId)
        {
            var vm = work.Media.SermonDashboard(sermonId);

            return View(vm);
        }

        public ActionResult Sermons(string seriesId, string topicId)
        {
            var sermons = work.Media.GetAllSermon(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Title).ToList();

            if (!string.IsNullOrEmpty(seriesId))
            {
                sermons = sermons.Where(x => x.SeriesId.Equals(seriesId)).OrderBy(x => x.Title).ToList();
            }

            if (!string.IsNullOrEmpty(topicId))
            {
                sermons = sermons.Where(x => x.TopicId.Equals(topicId)).OrderBy(x => x.Title).ToList();
                ViewBag.CurrentTopic = work.Media.GetSermonTopic(topicId).Title;
            }

            return View(sermons);
        }

        //[HttpGet]
        //public ActionResult _AddSermon()
        //{
        //    var sermon = new Sermon()
        //    {
        //        Id = Utilities.GenerateUniqueId(),
        //        ChurchId = SessionVariables.CurrentChurch.Id,
        //        CreatedDate = DateTime.Now,
        //        CreatedBy = SessionVariables.CurrentUser.User.Id
        //    };

        //    return PartialView("_CreateEditSermon", sermon);
        //}

        //[HttpPost]
        //public ActionResult _AddSermon(Sermon sermon)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        work.Media.CreateSermon(sermon);

        //        return AjaxRedirectTo("/media/sermons");
        //    }

        //    return PartialView("_CreateEditSermon", sermon);
        //}

        [HttpGet]
        public ActionResult _AddSermon()
        {
            List<string> rolesList = new List<string> { Shared.Shared.Roles.Pastor, Shared.Shared.Roles.SeniorPastor };
            var sermonViewModel = new SermonViewModel
            {
                Sermon = new Sermon
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                },
                TopicsList = work.Media.GetAllSermonTopic(),
                Pastors = work.User.GetAllByChurchRoles(SessionVariables.CurrentChurch.Id, rolesList),
                SeriesList = work.Media.GetAllSermonSeries(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEditSermon", sermonViewModel);
        }

        [HttpPost]
        public ActionResult _AddSermon(SermonViewModel sermonViewModel)
        {
            if (ModelState.IsValid)
            {
                work.Media.CreateSermon(sermonViewModel.Sermon);

                return AjaxRedirectTo("/media/sermons");
            }

            return PartialView("_CreateEditSermon", sermonViewModel);
        }

        [HttpGet]
        public ActionResult _EditSermon(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sermon = work.Media.GetSermon(id);

            if (sermon == null)
            {
                return HttpNotFound();
            }

            var rolesList = new List<string> { Shared.Shared.Roles.Pastor, Shared.Shared.Roles.SeniorPastor };
            var sermonViewModel = new SermonViewModel
            {
                Sermon = sermon,
                TopicsList = work.Media.GetAllSermonTopic(),
                Pastors = work.User.GetAllByChurchRoles(SessionVariables.CurrentChurch.Id, rolesList),
                SeriesList = work.Media.GetAllSermonSeries(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEditSermon", sermonViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditSermon(SermonViewModel sermonViewModel)
        {
            if (ModelState.IsValid)
            {
                sermonViewModel.Sermon.ModifiedDate = DateTime.Now;
                sermonViewModel.Sermon.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Media.UpdateSermon(sermonViewModel.Sermon);

                return AjaxRedirectTo("/media/sermons");
            }

            return PartialView("_CreateEditSermon", sermonViewModel);
        }

        [HttpGet]
        public ActionResult DeleteSermon(string id)
        {
            work.Media.DeleteSermon(id);

            return RedirectToAction("index");
        }
        #endregion

        #region Sermon Notes

        [HttpGet]
        public ActionResult SermonNotesList(string sermonId)
        {
            var model = work.Media.GetSermonNotesListViewModel(sermonId);
            return View(model);
        }

        [HttpGet]
        public ActionResult SermonFilledNotesList(string sermonId)
        {
            var list = work.Media.GetSermonFilledNotesList(sermonId);
            return View(list);
        }

        [HttpPost]
        public ActionResult GetTemplate(string churchID, string sermonID)
        {
            SermonNote model = work.Media.GetSermonNote(sermonID);
            model.ChurchId = churchID;
            model.SermonId = sermonID;

            return PartialView("_GetTemplate", model);
        }
        [HttpPost]
        public ActionResult AddBlankTemplate(SermonNote sermonNote)
        {
            sermonNote.Id = Utilities.GenerateUniqueId();
            sermonNote.NoteType = "Blank";
            work.Media.CreateSermonNote(sermonNote);

            return AjaxRedirectTo("/media/sermon?sermonId=" + sermonNote.SermonId);
        }

        public ActionResult GetBlankTemplate(string noteID, string sermonID)
        {
            var model = new SermonNoteView
            {
                SermonNote = new SermonNote()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    SermonId = sermonID
                }
            };
            model.SermonNote = work.Media.GetSermonNote(noteID);
            ViewBag.Church = SessionVariables.CurrentChurch.Name;

            return PartialView("_GetBlankNoteById", model);
        }
        public ActionResult SermonBlankNotesList(string sermonId)
        {
            var list = work.Media.GetSermonBlankNotesList(sermonId);
            return View(list);
        }

        [HttpGet]
        public ActionResult GetNoteById(string noteID, string sermonID)
        {
            var model = new SermonNoteView
            {
                SermonNote = new SermonNote()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    SermonId = sermonID
                }
            };
            model.SermonNote = work.Media.GetSermonNote(noteID);
            ViewBag.Church = SessionVariables.CurrentChurch.Name;

            return PartialView("_GetNoteById", model);
        }

        [HttpGet]
        public ActionResult _AddFilledSermonNotes(string sermondId)
        {
            var model = new SermonNoteView
            {
                SermonNote = new SermonNote()
                {
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    SermonId = sermondId
                },
                Sermons = work.Media.GetAllSermon(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Title).ToList()
            };

            return PartialView("_AddFilledNote", model);
        }
        [HttpPost]
        public ActionResult _AddFilledSermonNotes(SermonNoteView model)
        {
            if (ModelState.IsValid)
            {
                model.SermonNote.Id = Utilities.GenerateUniqueId();
                model.SermonNote.NoteType = "Filled";

                work.Media.CreateSermonNote(model.SermonNote);

                return AjaxRedirectTo("/media/sermonnotes");
            }

            return PartialView("_CreateEditSermonNotes", model);
        }

        [HttpPost]
        public ActionResult _EditFilledSermonNotes(SermonNote model)
        {
            if (ModelState.IsValid)
            {
                work.Media.UpdateSermonNote(model);

                return AjaxRedirectTo("/media/SermonFilledNotesList?sermonid=" + model.SermonId);
            }

            return PartialView("_CreateEditSermonNotes", model);
        }

        [HttpGet]
        public ActionResult _AddSermonNotes(string sermondId)
        {
            var model = new SermonNoteView
            {
                SermonNote = new SermonNote()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    SermonId = sermondId
                },
                Sermons = work.Media.GetAllSermon(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Title).ToList()
            };

            return PartialView("_CreateEditSermonNotes", model);
        }

        [HttpPost]
        public ActionResult _AddSermonNotes(SermonNoteView model)
        {
            if (ModelState.IsValid)
            {
                model.SermonNote.NoteType = "Standard";

                work.Media.CreateSermonNote(model.SermonNote);

                //var logObj = logRepository.JsonConverter("Name", equipment.Name, "Church ID", equipment.ChurchId, "Created By", equipment.CreatedBy);
                //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Equipment", "-", LogStatuses.Done, logObj);

                return AjaxRedirectTo("/media/SermonNotesList?sermonid=" + model.SermonNote.SermonId);
            }

            //var errorObj = logRepository.JsonConverter();
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Equipment", "-", LogStatuses.Error, errorObj);

            return PartialView("_CreateEditSermonNotes", model);
        }

        [HttpGet]
        public ActionResult _EditSermonNotes(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sermonNote = work.Media.GetSermonNote(id);

            if (sermonNote == null)
            {
                return HttpNotFound();
            }

            var model = new SermonNoteView
            {
                SermonNote = sermonNote,
                Sermons = work.Media.GetAllSermon(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Title).ToList()
            };

            return PartialView("_CreateEditSermonNotes", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditSermonNotes(SermonNoteView model)
        {
            if (ModelState.IsValid)
            {
                model.SermonNote.ModifiedDate = DateTime.Now;
                model.SermonNote.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Media.UpdateSermonNote(model.SermonNote);

                //var logObj = logRepository.JsonConverter("ID", equipment.Id, "Name", equipment.Name, "Church ID", equipment.ChurchId, "Created By", equipment.CreatedBy);
                //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Edit Equipment", equipment.Id, LogStatuses.Done, logObj);

                return AjaxRedirectTo("/media/sermonnoteslist?sermonId=" + model.SermonNote.SermonId);
            }

            //var errorObj = logRepository.JsonConverter("ID", equipment.Id);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Edit Equipment", equipment.Id, LogStatuses.Error, errorObj);

            return PartialView("_CreateEditSermonNotes", model);
        }

        [HttpGet]
        public ActionResult DeleteSermonNotes(string id)
        {
            work.Media.DeleteSermonNote(id);

            //var logObj = logRepository.JsonConverter("ID", equipment.Id, "Name", equipment.Name, "Church ID", equipment.ChurchId, "Created By", equipment.CreatedBy);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Delete Equipment", SessionVariables.CurrentChurch.Id, LogStatuses.Done, logObj);

            return RedirectToAction("index");
        }
        #endregion

        #region Sermon Topics

        public ActionResult SermonTopics()
        {
            var topics = work.Media.GetAllSermonTopic().OrderBy(x => x.Title).ToList();
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Sermon Topics", null, null, null);

            return View(topics);
        }

        [HttpGet]
        public ActionResult _AddSermonTopic()
        {
            var sermonTopic = new SermonTopic()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            return PartialView("_CreateEditSermonTopic", sermonTopic);
        }

        [HttpPost]
        public ActionResult _AddSermonTopic(SermonTopic sermonTopic)
        {
            if (ModelState.IsValid)
            {
                work.Media.CreateSermonTopic(sermonTopic);

                //var logObj = logRepository.JsonConverter("Name", equipment.Name, "Church ID", equipment.ChurchId, "Created By", equipment.CreatedBy);
                //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Equipment", "-", LogStatuses.Done, logObj);

                return AjaxRedirectTo("/media/sermontopics");
            }

            //var errorObj = logRepository.JsonConverter();
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Add Equipment", "-", LogStatuses.Error, errorObj);

            return PartialView("_CreateEditSermonTopic", sermonTopic);
        }

        [HttpGet]
        public ActionResult _EditSermonTopic(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sermonTopic = work.Media.GetSermonTopic(id);

            if (sermonTopic == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditSermonTopic", sermonTopic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditSermonTopic(SermonTopic sermonTopic)
        {
            if (ModelState.IsValid)
            {
                sermonTopic.ModifiedDate = DateTime.Now;
                sermonTopic.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Media.UpdateSermonTopic(sermonTopic);

                //var logObj = logRepository.JsonConverter("ID", equipment.Id, "Name", equipment.Name, "Church ID", equipment.ChurchId, "Created By", equipment.CreatedBy);
                //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Edit Equipment", equipment.Id, LogStatuses.Done, logObj);

                return AjaxRedirectTo("/media/sermontopics");
            }

            //var errorObj = logRepository.JsonConverter("ID", equipment.Id);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Edit Equipment", equipment.Id, LogStatuses.Error, errorObj);

            return PartialView("_CreateEditSermonTopic", sermonTopic);
        }

        [HttpGet]
        public ActionResult DeleteSermonTopic(string id)
        {
            work.Media.DeleteSermonTopic(id);

            //var logObj = logRepository.JsonConverter("ID", equipment.Id, "Name", equipment.Name, "Church ID", equipment.ChurchId, "Created By", equipment.CreatedBy);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Delete Equipment", SessionVariables.CurrentChurch.Id, LogStatuses.Done, logObj);

            return RedirectToAction("index");
        }
        #endregion
    }
}