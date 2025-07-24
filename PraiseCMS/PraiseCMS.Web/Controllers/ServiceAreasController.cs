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

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    [RequirePermission(ModuleId = "07737697423e093561d03a485a9e64")]
    public class ServiceAreasController : BaseController
    {
        public ActionResult Index()
        {
            return View(work.ServiceArea.GetAll(SessionVariables.CurrentChurch.Id));
        }

        [HttpGet]
        public ActionResult _CreateServiceArea()
        {
            var model = new ServiceAreaView
            {
                ServiceArea = new ServiceArea()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                },
                CommonServiceAreas = work.ServiceArea.GetAll(SessionVariables.CurrentChurch.Id).Any() ? new List<string>() : CommonServiceAreas.Items.OrderBy(q => q).ToList(),
                ChurchServiceAreaRequirements = (SessionVariables.CurrentChurch.ServiceAreaRequirements ?? string.Empty).SplitToList().OrderBy(x => x).ToList()
            };
            model.Requirements = model.ServiceArea.Requirements.SplitToList().ToArray();

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _CreateServiceArea(ServiceAreaView model)
        {
            var isSuccess = false;
            if (!work.ChurchEventType.GetAll(SessionVariables.CurrentChurch.Id).Any())
            {
                if (model.CommonServiceAreas?.Any() == true)
                {
                    foreach (var item in model.CommonServiceAreas)
                    {
                        isSuccess = true;
                        var eventType = new ServiceArea()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            ChurchId = SessionVariables.CurrentChurch.Id,
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            Name = item
                        };
                        work.ServiceArea.Create(eventType);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                model.ServiceArea.Requirements = model.Requirements.CombineToString();
                work.ServiceArea.Create(model.ServiceArea);
                return AjaxRedirectTo("/serviceareas");
            }

            if (isSuccess)
            {
                return AjaxRedirectTo("/serviceareas");
            }

            model.CommonServiceAreas = work.ServiceArea.GetAll(SessionVariables.CurrentChurch.Id).Any() ? new List<string>() : CommonServiceAreas.Items.OrderBy(q => q).ToList();

            return PartialView("_CreateEdit", model);
        }

        public ActionResult _EditServiceArea(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new ServiceAreaView();
            var serviceArea = work.ServiceArea.Get(id);

            if (serviceArea == null)
            {
                return HttpNotFound();
            }

            model.ServiceArea = serviceArea;
            model.Requirements = model.ServiceArea.Requirements.SplitToList().ToArray();

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditServiceArea(ServiceAreaView model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEdit", model);

            model.ServiceArea.Requirements = model.Requirements.CombineToString();
            model.ServiceArea.ModifiedDate = DateTime.Now;
            model.ServiceArea.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            work.ServiceArea.Update(model.ServiceArea);

            return AjaxRedirectTo("/serviceareas");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var serviceArea = work.ServiceArea.Get(id);

            if (serviceArea == null)
            {
                return HttpNotFound();
            }

            work.ServiceArea.Delete(serviceArea);

            return RedirectToAction("index");
        }
    }
}