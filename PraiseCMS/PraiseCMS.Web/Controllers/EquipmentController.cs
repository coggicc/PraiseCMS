using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "80018421523fde4273e3354ceb8dca")]
    public class EquipmentController : BaseController
    {
        #region Equipment
        public ActionResult Index()
        {
            var vm = work.Equipment.GetWithCategory(SessionVariables.CurrentChurch.Id);
            return View(vm);
        }

        public ActionResult ViewAll(string categoryId)
        {
            var model = work.Equipment.GetByCategory(categoryId);
            return View(model);
        }

        [HttpGet]
        public ActionResult _AddEquipment()
        {
            var model = new EquipmentView()
            {
                EquipmentCategories = work.Equipment.GetAllCategoriesByChurch(SessionVariables.CurrentChurch.Id),
                Equipment = new Equipment()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                }
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _AddEquipment(EquipmentView model)
        {
            if (ModelState.IsValid)
            {
                work.Equipment.Create(model.Equipment);

                return AjaxRedirectTo("/equipment/");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult _EditEquipment(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var equipment = work.Equipment.Get(id);

            if (equipment == null)
            {
                return HttpNotFound();
            }

            var model = new EquipmentView()
            {
                EquipmentCategories = work.Equipment.GetAllCategoriesByChurch(SessionVariables.CurrentChurch.Id),
                Equipment = equipment
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditEquipment(EquipmentView model)
        {
            if (ModelState.IsValid)
            {
                work.Equipment.Update(model.Equipment);

                return AjaxRedirectTo("/equipment/");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult DeleteEquipment(string id)
        {
            var equipment = work.Equipment.Get(id);
            work.Equipment.Delete(equipment);

            return RedirectToAction("index");
        }
        #endregion

        #region Equipment Categories
        public ActionResult Categories()
        {
            var categories = work.Equipment.GetAllCategoriesByChurch(SessionVariables.CurrentChurch.Id);
            return View(categories);
        }

        [HttpGet]
        public ActionResult _CreateEquipmentCategory()
        {
            var model = new EquipmentView()
            {
                EquipmentCategories = work.Equipment.GetAllCategoriesByChurch(SessionVariables.CurrentChurch.Id),
                EquipmentCategory = new EquipmentCategory
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                }
            };

            return PartialView("_CreateEditEquipmentCategory", model);
        }

        [HttpPost]
        public ActionResult _CreateEquipmentCategory(EquipmentView model)
        {
            if (ModelState.IsValid)
            {
                work.Equipment.CreateCategory(model.EquipmentCategory);

                return AjaxRedirectTo("/equipment/");
            }

            var errorObj = logRepository.JsonConverter();
            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Create Equipment Category", string.Empty, LogStatuses.Error, errorObj);

            return PartialView("_CreateEditEquipmentCategory", model);
        }

        [HttpGet]
        public ActionResult _EditEquipmentCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = work.Equipment.GetCategory(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            var model = new EquipmentView()
            {
                EquipmentCategories = work.Equipment.GetAllCategoriesByChurch(SessionVariables.CurrentChurch.Id),
                EquipmentCategory = category
            };

            return PartialView("_CreateEditEquipmentCategory", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditEquipmentCategory(EquipmentView model)
        {
            if (ModelState.IsValid)
            {
                work.Equipment.Update(model.EquipmentCategory);

                return AjaxRedirectTo("/equipment/");
            }

            return PartialView("_CreateEditEquipmentCategory", model);
        }

        [HttpGet]
        public ActionResult DeleteEquipmentCategory(string id)
        {
            work.Equipment.DeleteCategory(id);
            return RedirectToAction("index");
        }
        #endregion
    }
}