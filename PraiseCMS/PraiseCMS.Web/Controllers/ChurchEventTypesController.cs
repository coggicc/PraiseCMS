using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using static PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "926911928103512b11b7a34eb98b9c")]
    public class ChurchEventTypesController : BaseController
    {
        public ActionResult Index()
        {
            var result = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id);
            return View(result);
        }

        [HttpGet]
        public ActionResult _CreateChurchEventType(string returnUrl = "")
        {
            var model = work.ChurchEventType.CreateChurchEventTypeModel(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id);
            model.ReturnUrl = returnUrl;
            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _CreateChurchEventType(ChurchEventTypeView model, bool returnValue = false)
        {
            var isSuccess = false;
            if (!work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).Any())
            {
                if (model.CommonEventType.IsNullOrEmpty())
                {
                    model.CommonEventType = ChurchEvents.Items.Where(x => x.Equals(ChurchEvents.WorshipService)).ToList();
                }

                if (model.CommonEventType?.Any() == true)
                {
                    if (!model.CommonEventType.Any(x => x.Equals(ChurchEvents.WorshipService)))
                    {
                        model.CommonEventType.Add(ChurchEvents.Items.Find(x => x.Equals(ChurchEvents.WorshipService)));
                    }

                    foreach (var item in model.CommonEventType)
                    {
                        isSuccess = true;
                        var Id = Utilities.GenerateUniqueId();
                        var eventType = new ChurchEventType()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            Type = item
                        };

                        switch (item)
                        {
                            case ChurchEvents.WorshipService:
                                eventType.CalendarColor = ColorOptions.Blue;
                                break;
                            case ChurchEvents.ServeDay:
                                eventType.CalendarColor = ColorOptions.Red;
                                break;
                            case ChurchEvents.MenConference:
                                eventType.CalendarColor = ColorOptions.Black;
                                break;
                            case ChurchEvents.WomenConference:
                                eventType.CalendarColor = ColorOptions.Purple;
                                break;
                            case ChurchEvents.VolunteerTraining:
                                eventType.CalendarColor = ColorOptions.Yellow;
                                break;
                            case ChurchEvents.VacationBibleSchool:
                                eventType.CalendarColor = ColorOptions.Green;
                                break;
                            case ChurchEvents.MarriageConference:
                                eventType.CalendarColor = ColorOptions.Gray;
                                break;
                        }

                        work.ChurchEventType.Create(eventType);
                    }
                    work.ChurchEventType.SaveChanges();
                }
            }

            if (ModelState.IsValid)
            {
                work.ChurchEventType.Create(model.ChurchEventType);
                work.ChurchEventType.SaveChanges();

                if (returnValue)
                {
                    var lastRecord = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).OrderByDescending(q => q.CreatedDate).FirstOrDefault();
                    return Json(new { list = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id), selectedId = lastRecord?.Id });
                }

                return AjaxRedirectTo(!string.IsNullOrEmpty(model.ReturnUrl) ? model.ReturnUrl : "/churcheventtypes");
            }

            if (isSuccess)
            {
                if (returnValue)
                {
                    var lastRecord = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).OrderByDescending(q => q.CreatedDate).FirstOrDefault();
                    return Json(new { list = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id), selectedId = lastRecord?.Id });
                }

                return AjaxRedirectTo(!string.IsNullOrEmpty(model.ReturnUrl) ? model.ReturnUrl : "/churcheventtypes");
            }

            model.CommonEventType = work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).Any() ? new List<string>() : ChurchEvents.Items.OrderBy(q => q).ToList();

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult _EditChurchEventType(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new ChurchEventTypeView();
            var churchEventType = work.ChurchEventType.Get(id);

            if (churchEventType == null)
            {
                return HttpNotFound();
            }

            model.ChurchEventType = churchEventType;

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditChurchEventType(ChurchEventTypeView model)
        {
            if (ModelState.IsValid)
            {
                model.ChurchEventType.ModifiedDate = DateTime.Now;
                model.ChurchEventType.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.ChurchEventType.Update(model.ChurchEventType);

                if (!string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return AjaxRedirectTo(model.ReturnUrl);
                }

                return AjaxRedirectTo("/churcheventtypes");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var churchEventType = work.ChurchEventType.Get(id);

            if (churchEventType == null)
            {
                return HttpNotFound();
            }

            churchEventType.IsDeleted = true;
            churchEventType.ModifiedDate = DateTime.Now;
            churchEventType.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            work.ChurchEventType.Update(churchEventType);

            return RedirectToAction("index");
        }
    }
}