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
    
    [RequirePermission(ModuleId = "722786668516bb3f10fd9341c8908e")]
    public class DenominationsController : BaseController
    {
        public ActionResult Index()
        {
            var denomination = work.Denomination.GetAll();
            return View(denomination);
        }

        [HttpGet]
        public ActionResult _AddDenomination()
        {
            var denomination = new Denomination()
            {
                Id = Utilities.GenerateUniqueId()
            };

            return PartialView("_CreateEdit", denomination);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddDenomination(Denomination denomination)
        {
            if (ModelState.IsValid)
            {
                denomination.CreatedDate = DateTime.Now;
                denomination.CreatedBy = SessionVariables.CurrentUser.User.Id;
                work.Denomination.Create(denomination);

                return AjaxRedirectTo("/denominations/");
            }

            return PartialView("_CreateEdit", denomination);
        }

        [HttpGet]
        public ActionResult _EditDenomination(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var denomination = work.Denomination.Get(id);

            if (denomination == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEdit", denomination);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditDenomination(Denomination denomination)
        {
            if (ModelState.IsValid)
            {
                denomination.ModifiedDate = DateTime.Now;
                denomination.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.Denomination.Update(denomination);

                return AjaxRedirectTo("/denominations");
            }

            return PartialView("_CreateEdit", denomination);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            work.Denomination.Delete(id);
            return RedirectToAction("index");
        }
    }
}