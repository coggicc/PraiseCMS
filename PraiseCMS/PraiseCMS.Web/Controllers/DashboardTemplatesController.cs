using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "0238154837c438eb3a17f74844b713")]
    public class DashboardTemplatesController : BaseController
    {
        #region Dashboard Templates
        public ActionResult Index()
        {
            var dashboard = work.DashboardTemplate.GetDashboardForUser(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, SessionVariables.CurrentUser.Roles);

            if (dashboard.DashboardTemplates.Count == 1)
            {
                UpdateDashboardTemplateSessionVariables(SessionVariables.CurrentUser.User.Id, dashboard.DashboardTemplates[0]?.Id);
            }
            return View(dashboard);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult CreateEdit(string id = null)
        {
            var vm = work.DashboardTemplate.GetCreateEditModel(id, SessionVariables.Roles, SessionVariables.CurrentUser.IsSuperAdmin);
            return View(vm);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateEdit(DashboardTemplateCreateEditModel model)
        {
            if (!ModelState.IsValid || model.SelectedWidgets.Count == 0 || model.SelectedRoles.Count == 0)
            {
                if (model.SelectedRoles.Count == 0)
                {
                    ModelState.AddModelError("SelectedRoles", "Please select at least one role.");
                }

                if (model.SelectedWidgets.Count == 0)
                {
                    ModelState.AddModelError("SelectedWidgets", "Please select at least one widget.");
                }

                var vm = work.DashboardTemplate.GetCreateEditModel(model.DashboardTemplate.Id, SessionVariables.Roles, SessionVariables.CurrentUser.IsSuperAdmin);
                vm.SelectedRoles = model.SelectedRoles;
                vm.SelectedWidgets = model.SelectedWidgets;
                vm.DashboardTemplate = model.DashboardTemplate;

                return View(vm);
            }

            model.DashboardTemplate.ChurchId = SessionVariables.CurrentChurch.Id;

            if (model.DashboardTemplate.Id.IsNotNullOrEmpty())
            {
                model.DashboardTemplate.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                model.DashboardTemplate.ModifiedDate = DateTime.Now;
            }
            else
            {
                model.DashboardTemplate.CreatedBy = SessionVariables.CurrentUser.IsSuperAdmin ? Constants.System : SessionVariables.CurrentUser.User.Id;

                model.DashboardTemplate.CreatedDate = DateTime.Now;
            }

            var result = work.DashboardTemplate.CreateEdit(model);

            CreateAlertMessage(result.Message, result.ResultType, result.ResultIcon);

            if (result.ResultType == ResultType.Success)
            {
                if (SessionVariables.CurrentUser.Settings.DashboardTemplateId.IsNotNullOrEmpty() && SessionVariables.CurrentUser.Settings.DashboardTemplateId == model.DashboardTemplate.Id)
                {
                    UpdateDashboardTemplateSessionVariables(SessionVariables.CurrentUser.User.Id, model.DashboardTemplate.Id);
                }

                return RedirectToAction("index");
            }

            return View(work.DashboardTemplate.GetCreateEditModel(model.DashboardTemplate.Id, SessionVariables.Roles, SessionVariables.CurrentUser.IsSuperAdmin));
        }

        public ActionResult Delete(string id)
        {
            var result = work.DashboardTemplate.Delete(id);
            CreateAlertMessage(result.Message, result.ResultType, result.ResultIcon);

            return AjaxRedirectTo("/dashboardtemplates");
        }

        [RequirePermission(ModuleId = "0400969134a9ec79411a7e43b7b5f2")]
        public ActionResult Categories()
        {
            var categories = SessionVariables.CurrentUser.IsSuperAdmin ? work.DashboardTemplate.GetAllWidgetCategoryTypesDefault()
                : work.DashboardTemplate.GetWidgetCategoryTypesByRoles(SessionVariables.Roles);

            var vm = new DashboardTemplateCategoriesVM
            {
                Categories = categories,
                Users = work.User.GetAllByChurchIds(categories.Select(x => x.ChurchId).ToList())
            };

            return View(vm);
        }

        public ActionResult SetPrimary(string userId, string templateId)
        {
            UpdateDashboardTemplateSessionVariables(userId, templateId);
            CreateAlertMessage("Your primary dashboard has been updated.", ResultType.Success, ResultIcon.Success);

            return RedirectToAction("index");
        }

        public ActionResult ManageLayout(string templateId)
        {
            var sorting = work.DashboardTemplate.GetManageLayoutDashboard(SessionVariables.CurrentUser.User.Id, templateId);

            return View(sorting);
        }

        public void UpdateWidgetSorting(string templateId, List<string> data)
        {
            work.DashboardTemplate.UpdateDashboardWidgetSortOrder(SessionVariables.CurrentUser.User.Id, templateId, data);
            UpdateDashboardTemplateSessionVariables(SessionVariables.CurrentUser.User.Id, templateId);
        }

        [HttpGet]
        public ActionResult CustomizeDashboard()
        {
            //var roles = SessionVariables.Roles.Select(x => x.Id).ToList();
            var result = work.DashboardTemplate.GetCustomizedDashboard();
            return PartialView("_CustomizeDashboard", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomizeDashboard(CustomDashboardVM model)
        {
            if (ModelState.IsValid && model.Widgets.IsNotNullOrEmpty() && model.Widgets.Any())
            {
                var result = work.DashboardTemplate.CreateUpdateCustomizedDashboard(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage(result.Message, AlertMessageTypes.Success, AlertMessageIcons.Success);
                    UpdateDashboardTemplateSessionVariables(SessionVariables.CurrentUser.User.Id, result.Data.Id);

                    return AjaxRedirectTo("/");
                }
                else
                {
                    CreateAlertMessage($"{result.Message} Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }
            }

            CreateAlertMessage("Please select at least one widget for your custom dashboard.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            //var roles = SessionVariables.Roles.Select(x => x.Id).ToList();
            model = work.DashboardTemplate.GetCustomizedDashboard(model.Widgets.Any());

            return PartialView("_CustomizeDashboard", model);
        }

        public ActionResult Clone(string id)
        {
            work.DashboardTemplate.Clone(id, SessionVariables.CurrentUser.User.Id, SessionVariables.CurrentChurch.Id);
            return AjaxRedirectTo("/dashboardtemplates");
        }

        //Needs to be updated to not use session variables, I believe.
        public ActionResult Preview(string id)
        {
            var dashboard = work.DashboardTemplate.Get(id);

            ViewBag.Title = dashboard != null && !string.IsNullOrEmpty(dashboard.Name) ? "Preview: " + dashboard.Name : "Preview Dashboard";

            var workWeek = SessionVariables.CurrentChurch.WorkWeek.IsNotNullOrEmpty() ? SessionVariables.CurrentChurch.WorkWeek.Split('-')[0] : WorkWeeks.SundaySaturday;

            var model = new DashboardViewModel
            {
                Attendance = work.Attendance.GetAll(SessionVariables.CurrentChurch.Id, ExtensionMethods.GetCurrentYearDateRange()),
                Baptisms = work.Baptism.GetAll(SessionVariables.CurrentChurch.Id, ExtensionMethods.GetCurrentYearDateRange()),
                Events = work.Event.GetEvents(SessionVariables.CurrentChurch.Id),
                Giving = work.Giving.GetGiving(SessionVariables.CurrentChurch.Id, Utilities.GetDateRangesOfLastNumberOfWeeks(1, workWeek)),
                MyGiving = work.Giving.GetHistory(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id).Data,
                NewDonors = work.Giving.NewDonors(SessionVariables.CurrentChurch.Id),
                Notifications = work.Notification.GetAllByChurchId(SessionVariables.CurrentChurch.Id),
                OfflineGiving = work.OfflineGiving.GetAll(SessionVariables.CurrentChurch.Id, null, ExtensionMethods.GetCurrentYearDateRange()),
                Payments = work.Payment.GetAll(SessionVariables.CurrentChurch.Id, null, ExtensionMethods.GetCurrentYearDateRange()),
                PrayerRequests = work.PrayerRequest.GetAll(SessionVariables.CurrentChurch.Id, ExtensionMethods.GetCurrentYearDateRange()),
                RecentDeaths = work.Person.GetDeceasedPeople(SessionVariables.CurrentChurch.Id, new DateRange { EndDate = DateTime.Now.Date, StartDate = DateTime.Now.AddMonths(-1).Date }),
                Salvations = work.Salvation.GetAll(SessionVariables.CurrentChurch.Id, ExtensionMethods.GetCurrentYearDateRange()),
                SmallGroups = work.SmallGroup.GetAll(SessionVariables.CurrentChurch.Id, ExtensionMethods.GetCurrentYearDateRange()),
                UpcomingBirthdays = work.Person.GetUpcomingBirthdays(SessionVariables.CurrentChurch.Id)
            };
            return View(model);
        }

        #endregion

        #region Widgets
        [RequirePermission(ModuleId = "0527533005d361e762c281450aa5f2")]
        public ActionResult Widgets()
        {
            var widgets = work.DashboardTemplate.GetWidgetsView(SessionVariables.Roles, SessionVariables.CurrentUser.IsSuperAdmin);
            return View(widgets);
        }

        [RequirePermission(ModuleId = "0527533005d361e762c281450aa5f2")]
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult CreateEditWidget(string id = null)
        {
            var vm = new CreateEditWidgetVM
            {
                Files = GetWidgetFiles(),
                WidgetCategoryTypes = work.DashboardTemplate.GetAllWidgetCategoryTypes(),
                Roles = work.Role.GetAll()
            };

            if (id.IsNotNullOrEmpty())
            {
                vm.Widget = work.DashboardTemplate.GetWidget(id);
                vm.WidgetPermissions = work.DashboardTemplate.GetAllWidgetPermissions(id);
                vm.WidgetCategories = work.DashboardTemplate.GetAllWidgetCategoriesByWidget(id);
            }

            return View(vm);
        }

        [RequirePermission(ModuleId = "0527533005d361e762c281450aa5f2")]
        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateEditWidget(CreateEditWidgetVM model, HttpPostedFileBase image)
        {
            if (!ModelState.IsValid || model.SelectedRoles.Count == 0 || model.SelectedCategories.Count == 0)
            {
                if (model.SelectedRoles.Count == 0)
                {
                    ModelState.AddModelError("SelectedRoles", "Please select at least one role.");
                }

                if (model.SelectedCategories.Count == 0)
                {
                    ModelState.AddModelError("SelectedCategories", "Please select at least one category.");
                }

                model.WidgetCategoryTypes = work.DashboardTemplate.GetAllWidgetCategoryTypes();
                model.Roles = work.Role.GetAll();
                model.Files = GetWidgetFiles();

                if (model.Widget.Id.IsNotNullOrEmpty())
                {
                    model.Widget = work.DashboardTemplate.GetWidget(model.Widget.Id);
                    model.WidgetPermissions = work.DashboardTemplate.GetAllWidgetPermissions(model.Widget.Id);
                }

                return View(model);
            }

            if (image.IsNotNull())
            {
                if (model.Widget.Id.IsNotNullOrEmpty() && model.Widget.ImageUrl.IsNotNullOrEmpty())
                {
                    Uploader.DeleteImage(model.Widget.ImageUrl);
                }

                model.Widget.ImageUrl = Utilities.GenerateUniqueId() + Path.GetExtension(image.FileName);
                Uploader.UploadImage(model.Widget.ImageUrl, image);
            }

            var result = work.DashboardTemplate.CreateUpdateWidget(model);
            CreateAlertMessage(result.Message, result.ResultType, result.ResultIcon);

            // Updating session variable widgets here
            if (result.ResultType == ResultType.Success)
            {
                if (SessionVariables.Widgets.Any(x => x.Widget.IsNotNullOrEmpty() && x.Widget.Id == model.Widget.Id))
                {
                    UpdateDashboardTemplateSessionVariables(SessionVariables.CurrentUser.User.Id, SessionVariables.CurrentUser.Settings.DashboardTemplateId);
                }
            }

            return RedirectToAction("widgets");
        }

        [RequirePermission(ModuleId = "0400969134a9ec79411a7e43b7b5f2")]
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult CreateEditCategory(string id = null)
        {
            var vm = new CreateEditWidgetCategoryVM();

            if (id.IsNotNullOrEmpty())
            {
                //Editing
                vm.WigetCategoryType = work.DashboardTemplate.GetWidgetCategoryType(id);
                vm.WidgetCategoryRoles = work.DashboardTemplate.GetAllWidgetCategoryRoles(id);
            }
            else
            {
                //Creating
                vm.WigetCategoryType = new WidgetCategoryType
                {
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                };
            }

            if (SessionVariables.CurrentUser.IsSuperAdmin)
            {
                vm.Roles = work.Role.GetAll(SessionVariables.CurrentChurch.Id);
            }
            else
            {
                vm.Roles = work.Role.GetAll(SessionVariables.CurrentChurch.Id).Where(x => x.Name != Roles.SuperAdmin).ToList();
            }

            return View(vm);
        }

        [RequirePermission(ModuleId = "0400969134a9ec79411a7e43b7b5f2")]
        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateEditCategory(CreateEditWidgetCategoryVM model)
        {
            if (!ModelState.IsValid || model.SelectedRoles.Count == 0)
            {
                if (model.SelectedRoles.Count == 0)
                {
                    ModelState.AddModelError("SelectedRoles", "Please select at least one role.");
                }

                model.Roles = work.Role.GetAll();

                return View(model);
            }

            model.WigetCategoryType.ChurchId = SessionVariables.CurrentChurch.Id;

            if (model.WigetCategoryType.Id.IsNotNullOrEmpty())
            {
                model.WigetCategoryType.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                model.WigetCategoryType.ModifiedDate = DateTime.Now;
            }
            else
            {
                model.WigetCategoryType.CreatedBy = Constants.System;
                model.WigetCategoryType.CreatedDate = DateTime.Now;
            }

            var result = work.DashboardTemplate.CreateEditWidgetCategory(model);
            CreateAlertMessage(result.Message, result.ResultType, result.ResultIcon);

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult DeleteWidget(string id)
        {
            work.DashboardTemplate.DeleteWidget(id);
            return AjaxRedirectTo("/dashboardtemplates/widgets");
        }

        public List<SelectListItem> GetWidgetFiles()
        {
            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Views/DashboardTemplates/Widgets"));
            var files = new List<SelectListItem>();
            var tilesOptionGroup = new SelectListGroup() { Name = "Top Tiles" };
            var widgetsOptionGroup = new SelectListGroup() { Name = "Widgets" };

            foreach (string file in filePaths)
            {
                string fileName = Path.GetFileName(file);
                files.Add(new SelectListItem
                {
                    Text = fileName,
                    Value = fileName,
                    Group = fileName.Contains("Tile") ? tilesOptionGroup : widgetsOptionGroup
                });
            }

            return files;
        }

        [HttpGet]
        public ActionResult GetInfo(string id)
        {
            var result = work.DashboardTemplate.GetWidget(id);
            return PartialView("_WidgetInfo", result);
        }
        #endregion

        private void UpdateDashboardTemplateSessionVariables(string userId, string templateId)
        {
            var settings = work.UserSetting.GetByUserId(userId);
            settings.DashboardTemplateId = templateId;
            settings.ModifiedDate = DateTime.Now;
            settings.ModifiedBy = userId;
            work.UserSetting.Update(settings);

            if (SessionVariables.CurrentUser.User.Id == userId)
            {
                SessionVariables.Widgets = work.DashboardTemplate.GetActiveWidgetSortable(userId);
                SessionVariables.CurrentUser.Settings = settings;
            }
        }
    }
}