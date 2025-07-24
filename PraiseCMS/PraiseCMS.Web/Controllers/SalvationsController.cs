using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "8827179569f43ed891928d4c54a94b")]
    public class SalvationsController : BaseController
    {
        public ActionResult Index()
        {
            return View(work.Salvation.GetAll(SessionVariables.CurrentChurch.Id));
        }

        [HttpGet]
        public ActionResult _CreateSalvations()
        {
            var salvation = new Salvation()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEdit", salvation);
        }

        [HttpPost]
        public ActionResult _CreateSalvations(Salvation salvation)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEdit", salvation);

            work.Salvation.Create(salvation);

            return AjaxRedirectTo("/salvations");
        }

        [HttpGet]
        public ActionResult _EditSalvations(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var salvation = work.Salvation.Get(id);

            if (salvation == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEdit", salvation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditSalvations(Salvation salvation)
        {
            if (ModelState.IsValid)
            {
                salvation.ModifiedDate = DateTime.Now;
                salvation.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Salvation.Update(salvation);

                return AjaxRedirectTo("/salvations");
            }

            return PartialView("_CreateEdit", salvation);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var salvation = work.Salvation.Get(id);

            if (salvation == null)
            {
                return HttpNotFound();
            }

            work.Salvation.Delete(salvation);

            return RedirectToAction("index");
        }
    }
}