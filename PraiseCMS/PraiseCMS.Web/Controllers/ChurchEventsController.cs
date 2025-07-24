using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "9065463250b8a4c56fc2034d8da49f")]
    public class ChurchEventsController : BaseController
    {
        //public ActionResult Index()
        //{
        //    var result = work.ChurchEvent.GetAll(SessionVariables.CurrentChurch.Id);
        //    return View(result);
        //}

        //public ActionResult Services(string typeId)
        //{
        //    var churchEventView = new ChurchEventView();
        //    churchEventView.ChurchEvents = work.ChurchEvent.GetByType(typeId);
        //    return View("Index", churchEventView);
        //}

        public ActionResult Dashboard(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var churchEvent = work.ChurchEvent.GetWithScheduler(id);

            if (churchEvent.ChurchEvent.IsNull())
            {
                return HttpNotFound();
            }

            return View(churchEvent);
        }

        [HttpGet]
        public ActionResult _CreateChurchEvent()
        {
            var churchEventView = new ChurchEventView
            {
                CurrentChurchEvent = new ChurchEvent()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                },
                ChurchEventTypes = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Type).ToList()
            };

            return PartialView("_CreateEdit", churchEventView);
        }

        [HttpPost]
        public ActionResult _CreateChurchEvent(ChurchEventView churchEventView)
        {
            if (!ModelState.IsValid)
            {
                churchEventView.ChurchEventTypes = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id);
                return PartialView("_CreateEdit", churchEventView);
            }

            work.ChurchEvent.Create(churchEventView);
            return AjaxRedirectTo("/churchevents");
        }

        [HttpGet]
        public ActionResult _EditChurchEvent(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var churchEvent = work.ChurchEvents.Get(id);

            if (churchEvent == null)
            {
                return HttpNotFound();
            }

            var churchEventView = new ChurchEventView
            {
                CurrentChurchEvent = churchEvent,
                ChurchEventTypes = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Type).ToList()
            };

            return PartialView("_CreateEdit", churchEventView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditChurchEvent(ChurchEventView churchEventView)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEdit", churchEventView);
            }

            work.ChurchEvent.Update(churchEventView);

            return AjaxRedirectTo("/churchevents");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            work.ChurchEvent.Delete(id);

            return RedirectToAction("index");
        }

        //public ActionResult CheckIns(string EventId)
        //{
        //    ChurchEventCheckInView result = new ChurchEventCheckInView();
        //    result.ChurchEvent.Id = EventId;
        //   // result.CheckIn = work.ChurchEvent.GetAllCheckIns(EventId, SessionVariables.CurrentChurch.Id);

        //    var userIds = new List<string>();
        //    var modifiedBy = result.CheckIn.Select(x => x.ModifiedBy).ToList();
        //    var checkedInIds = result.CheckIn.Select(x => x.TypeId).ToList();

        //    userIds.AddRange(modifiedBy);
        //    userIds.AddRange(checkedInIds);

        //    userIds = userIds.Distinct().ToList();
        //    result.Users = work.User.GetByChurchId(SessionVariables.CurrentChurch.Id);

        //    ViewBag.Users = work.User.Get(userIds);

        //    return View(result);
        //}

        //[HttpPost]
        //public ActionResult CheckIns(CheckIn CheckIn)
        //{
        //    var exist = work.ChurchEvent.IsCheckedIn(CheckIn);

        //    if (exist || CheckIn.TypeId == "0")
        //    {
        //        return RedirectToAction("CheckIns", new
        //        {
        //            EventId = CheckIn.ChurchEventId
        //        });
        //    }

        //    work.ChurchEvent.CheckIn(CheckIn);

        //    return RedirectToAction("CheckIns", new
        //    {
        //        EventId = CheckIn.ChurchEventId
        //    });
        //}

        public ActionResult CheckOut(string CheckInId, string EventId)
        {
            work.ChurchEvent.CheckInUpdate(CheckInId);
            return RedirectToAction("CheckIns", new
            {
                EventId
            });
        }

        public ActionResult EventTimes(string EventId)
        {
            var result = work.ChurchEvent.GetWithScheduler(EventId);
            return View(result);
        }

        [HttpGet]
        public ActionResult _CreateEventTime(string eventId)
        {
            var model = new EventTimeViewModel
            {
                ChurchEventScheduler = new ChurchEventScheduler()
                {
                    Id = Utilities.GenerateUniqueId(),
                    EventId = eventId,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                }
            };

            return PartialView("_CreateEditEventTime", model);
        }

        [HttpPost]
        public ActionResult _CreateEventTime(EventTimeViewModel model)
        {
            if (ModelState.IsValid)
            {
                work.ChurchEvent.CreateScheduler(model.ChurchEventScheduler);
                return AjaxRedirectTo("/rooms");    //pass in a return URL here to take user to last spot
            }

            return PartialView("_CreateEditEventTime", model);
        }

        [HttpGet]
        public ActionResult _EditEventTime(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var time = work.ChurchEvent.GetChurchEventScheduler(id);

            if (time == null)
            {
                return HttpNotFound();
            }

            var model = new EventTimeViewModel
            {
                ChurchEventScheduler = time
            };

            return PartialView("_CreateEditEventTime", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditEventTime(EventTimeViewModel model)
        {
            if (ModelState.IsValid)
            {
                work.ChurchEvent.UpdateEventTime(model);
                return AjaxRedirectTo("/rooms");
            }

            return PartialView("_CreateEditEventTime", model);    //pass in a return URL here to take user to last spot
        }

        public ActionResult EventNewTime(EventTimeViewModel model, string Id, string EventId)
        {
            ViewBag.EventId = EventId;

            string campusIds = work.Campus.GetByEventId(EventId);

            if (campusIds == "All")
            {
                ViewBag.CheckedAll = true;
                model.CampusList = work.Campus.GetAll(SessionVariables.CurrentChurch.Id);
            }
            else
            {
                ViewBag.CheckedAll = false;
                var campusIdList = campusIds.SplitToList();
                model.CampusList = work.Campus.GetAll(campusIdList);
            }

            model.ChurchEventScheduler.StartDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Id))
            {
                model.ChurchEventScheduler = work.ChurchEvent.GetChurchEventScheduler(Id);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EventNewTime(EventTimeViewModel model, string EventId)
        {
            model.ChurchEventScheduler.EventId = EventId;
            ViewBag.EventId = model.ChurchEventScheduler.EventId;

            string campusIds = work.Campus.GetByEventId(model.ChurchEventScheduler.EventId);

            if (campusIds == "All")
            {
                ViewBag.CheckedAll = true;
                model.CampusList = work.Campus.GetAll(SessionVariables.CurrentChurch.Id);
            }
            else
            {
                ViewBag.CheckedAll = false;
                var campusIdList = campusIds.SplitToList();
                model.CampusList = work.Campus.GetAll(campusIdList);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            work.ChurchEvent.UpdateChurchEventScheduler(model);

            return RedirectToAction("EventTimes", new
            {
                model.ChurchEventScheduler.EventId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddFirstEvent(string eventId, string eventStartDate, string eventStartTime, string[] Campuses)
        {
            work.ChurchEvent.AddFirstEvent(eventId, eventStartDate, eventStartTime, Campuses);
            return RedirectToAction("EventTimes", new { EventId = eventId });  //Return with errors displayed
        }
    }
}