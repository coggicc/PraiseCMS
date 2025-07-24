using Microsoft.AspNet.Identity;
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
    //[RequirePermission(ModuleId = "1706283011b4658f081b6f479187ea")]
    public class SmallGroupCategoryTypesController : BaseController
    {
        public ActionResult Index()
        {
            return View(work.SmallGroupCategoryType.GetAll());
        }

        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var categoryType = work.SmallGroupCategoryType.Get(id);

            if (categoryType == null)
            {
                return HttpNotFound();
            }

            return View(categoryType);
        }

        public ActionResult Create()
        {
            var categoryType = new SmallGroupCategoryType()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id
            };

            return View(categoryType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SmallGroupCategoryType categoryType)
        {
            if (ModelState.IsValid)
            {
                categoryType.CreatedDate = DateTime.Now;
                categoryType.CreatedBy = User.Identity.GetUserId();

                work.SmallGroupCategoryType.Create(categoryType);

                return RedirectToAction("Index");
            }

            return View(categoryType);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var categoryType = work.SmallGroupCategoryType.Get(id);

            if (categoryType == null)
            {
                return HttpNotFound();
            }

            return View(categoryType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SmallGroupCategoryType categoryType)
        {
            if (ModelState.IsValid)
            {
                categoryType.ModifiedDate = DateTime.Now;
                categoryType.ModifiedBy = User.Identity.GetUserId();

                work.SmallGroupCategoryType.Update(categoryType);

                return RedirectToAction("Index");
            }
            return View(categoryType);
        }

        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var categoryType = work.SmallGroupCategoryType.Get(id);

            if (categoryType == null)
            {
                return HttpNotFound();
            }

            return View(categoryType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            work.SmallGroupCategoryType.Delete(id);
            return RedirectToAction("Index");
        }
    }
}