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
    //[RequirePermission(ModuleId = "61301025300ef61e62b5144aeabd31")]
    public class SmallGroupsController : BaseController
    {
        public ActionResult Index()
        {
            return View(work.SmallGroup.GetAll(SessionVariables.CurrentChurch.Id));
        }

        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SmallGroup smallGroup = work.SmallGroup.Get(id);

            if (smallGroup == null)
            {
                return HttpNotFound();
            }

            return View(smallGroup);
        }

        [HttpGet]
        public ActionResult _CreateSmallGroup()
        {
            var model = new SmallGroup()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            ViewBag.Creating = true;

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _CreateSmallGroup(SmallGroup model)
        {
            if (ModelState.IsValid)
            {
                work.SmallGroup.Create(model);

                return AjaxRedirectTo("/smallgroups");
            }

            return PartialView("_CreateEdit", model);
        }

        public ActionResult _EditSmallGroup(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.SmallGroup.Get(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Creating = false;

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditSmallGroup(SmallGroup model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.SmallGroup.Update(model);

                return AjaxRedirectTo("/smallgroups");
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

            var smallGroup = work.SmallGroup.Get(id);

            if (smallGroup == null)
            {
                return HttpNotFound();
            }

            work.SmallGroup.Delete(smallGroup);

            return RedirectToAction("index");
        }
    }
}