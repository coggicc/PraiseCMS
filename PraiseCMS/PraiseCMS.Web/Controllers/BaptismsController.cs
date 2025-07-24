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
    
    [RequirePermission(ModuleId = "1537168360aba20b87b11d491c8694")]
    public class BaptismsController : BaseController
    {
        public ActionResult Index()
        {
            return View(work.Baptism.GetAll(SessionVariables.CurrentChurch.Id));
        }

        [HttpGet]
        public ActionResult _CreateBaptisms()
        {
            var baptism = new Baptism()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEdit", baptism);
        }

        [HttpPost]
        public ActionResult _CreateBaptisms(Baptism baptism)
        {
            if (ModelState.IsValid)
            {
                work.Baptism.Create(baptism);

                return AjaxRedirectTo("/Baptisms");
            }

            return PartialView("_CreateEdit", baptism);
        }

        [HttpGet]
        public ActionResult _EditBaptisms(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var baptism = work.Baptism.Get(id);
            if (baptism == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEdit", baptism);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditBaptisms(Baptism baptism)
        {
            if (ModelState.IsValid)
            {
                baptism.ModifiedDate = DateTime.Now;
                baptism.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Baptism.Update(baptism);

                return AjaxRedirectTo("/Baptisms");
            }

            return PartialView("_CreateEdit", baptism);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var baptism = work.Baptism.Get(id);

            if (baptism == null)
            {
                return HttpNotFound();
            }

            work.Baptism.Delete(baptism);

            return RedirectToAction("index");
        }
    }
}