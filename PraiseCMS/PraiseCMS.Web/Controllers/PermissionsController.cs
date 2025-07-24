using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    // [RequireRole(Role = PraiseCMS.Shared.Roles.SuperAdmin)]
    [RequirePermission(ModuleId = "28834340961e860481430b4d1fbad1")]
    public class PermissionsController : BaseController
    {
        public ActionResult Index()
        {
            var result = work.Permission.GetDashboard(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.IsSuperAdmin);

            //remove super admin from user list
            result.ApplicationUsers = result.ApplicationUsers.FindAll(q => q.UserRolesList.IsNullOrEmpty() || (q.UserRolesList.IsNotNullOrEmpty() && q.UserRolesList.Any() && !q.UserRolesList.Contains(Shared.Shared.Roles.SuperAdmin))).OrderBy(x => x.Display).ToList();

            if (!SessionVariables.CurrentUser.IsSuperAdmin)
            {
                // Removing super admin role for non super admin user                
                if (result.ApplicationRoles.Count > 0 && result.ApplicationRoles.IsNotNull())
                {
                    var superAdminRole = result.ApplicationRoles.FirstOrDefault(x => x.Name == Shared.Shared.Roles.SuperAdmin);

                    if (superAdminRole.IsNotNull())
                    {
                        result.ApplicationRoles.Remove(superAdminRole);
                    }
                }

                // Removing super admin module for non super admin user
                if (result.Modules.Count > 0 && result.Modules.IsNotNull())
                {
                    var superAdminModule = result.Modules.FirstOrDefault(x => x.Name == Shared.Shared.Roles.SuperAdmin);

                    if (superAdminModule.IsNotNull())
                    {
                        result.Modules.Remove(superAdminModule);
                    }
                }
            }

            return View(result);
        }

        public PartialViewResult UserRoles(string userid)
        {
            var result = work.Permission.GetUserRole(SessionVariables.CurrentChurch.Id, userid);
            return PartialView("_UserRoles", result);
        }

        public ActionResult Roles()
        {
            return View(work.Role.GetAll(SessionVariables.CurrentChurch.Id));
        }

        public PartialViewResult ViewRolePartial()
        {
            PermissionViewModel result = new PermissionViewModel
            {
                ApplicationRoles = work.Permission.ReadAllRolesWithModules(SessionVariables.CurrentChurch.Id),
                Modules = work.Module.GetAll()
            };

            return PartialView("_ViewRolePartial", result);
        }

        public ActionResult AssignRoleToUser(string userId, string roleId)
        {
            try
            {
                var result = work.Permission.InsertUserRole(userId, roleId);
                return Json(new { result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { result = "exception" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveRoleFromUser(string userId, string roleId)
        {
            try
            {
                var result = work.Permission.DeleteUserRole(userId, roleId);

                if (result)
                {
                    return Json(new { result = "removed" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return Json(new { result = "exception" }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult AddEditRolePopup(string RoleId)
        {
            var role = new ApplicationRoles();
            var result = work.Permission.ReadRoleById(RoleId);

            if (result != null)
            {
                role = result;
            }

            return PartialView("AddEditRolePopup", role);
        }

        [HttpGet]
        public ActionResult _CreateRole()
        {
            var model = new ApplicationRoles()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id
            };

            return PartialView("_CreateEditRole", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreateRole(ApplicationRoles model)
        {
            if (ModelState.IsValid)
            {
                work.Permission.InsertRole(model, SessionVariables.CurrentChurch.Id);

                return AjaxRedirectTo("/permissions/roles");
            }

            return PartialView("_CreateEditRole", model);
        }

        [HttpGet]
        public PartialViewResult _EditRole(string Id)
        {
            var role = work.Permission.ReadRoleById(Id);
            return PartialView("_CreateEditRole", role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditRole(ApplicationRoles model)
        {
            work.Permission.UpdateRole(model, SessionVariables.CurrentChurch.Id);
            return AjaxRedirectTo("/permissions/roles");
        }

        public ActionResult DeleteRole(string Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var result = work.Permission.DeleteRoleById(Id);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage(result.Data, AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else if (result.ResultType == ResultType.Failure)
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }
            else if (result.ResultType == ResultType.Exception)
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Message", result.Message, LogStatuses.Exception, result.Exception, "Role Id", Id);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Role", Id, LogStatuses.Error, logObj);
            }

            return RedirectToAction("roles");
        }

        public ActionResult LoadPermissions(ModulePermissionsModel model)
        {
            var result = work.Permission.LoadPermissions(model);
            return PartialView("_Permissions", result);
        }

        public ActionResult UpdatePermission(ModulePermissionsModel model)
        {
            work.Permission.UpdatePermission(model);

            var module = work.Module.Get(model.ModuleId);

            if (module.IsNotNullOrEmpty() && module.ParentId.IsNotNullOrEmpty())
            {
                model.ModuleId = model.ParentModuleId = module.ParentId;
            }
            else
            {
                model.ParentModuleId = module.Id;
            }

            var result = work.Permission.LoadPermissions(model);

            return PartialView("_Permissions", result);
        }

        public ActionResult RemoveUserPermission(string Id)
        {
            var permission = work.Permission.Get(Id);
            work.Permission.RemovePermission(permission);

            //reload partial view
            var model = new ModulePermissionsModel()
            {
                ModuleId = permission.ModuleId,
                Type = permission.Type,
                TypeId = permission.TypeId
            };

            var module = work.Module.Get(model.ModuleId);

            if (module.IsNotNullOrEmpty() && module.ParentId.IsNotNullOrEmpty())
            {
                model.ModuleId = model.ParentModuleId = module.ParentId;
            }
            else
            {
                model.ParentModuleId = module.Id;
            }

            var result = work.Permission.LoadPermissions(model);

            return PartialView("_Permissions", result);
        }

        public ActionResult CustomPermission()
        {
            var model = new CustomPermissionModel
            {
                Modules = work.Module.GetAutoCompleteModel(blankText: false).Modules,
                Roles = work.Role.GetAll(SessionVariables.CurrentChurch.Id).Where(q => q.Name != Shared.Shared.Roles.SuperAdmin).OrderBy(x => x.Name).ToList()
            };

            model.SelectedRoles = model.Roles.Select(x => x.Id).ToList();

            return PartialView("_CustomPermission", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomPermission(CustomPermissionModel model)
        {
            model.Roles = work.Role.GetAll(SessionVariables.CurrentChurch.Id).Where(q => q.Name != Shared.Shared.Roles.SuperAdmin).OrderBy(x => x.Name).ToList();

            if (model.ModuleId.IsNullOrEmpty())
            {
                model.Modules = work.Module.GetAutoCompleteModel(blankText: false).Modules;
                CreateAlertMessage("Please select a module to set permission", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                return PartialView("_CustomPermission", model);
            }

            var notPermittedRoles = model.Roles.Select(x => x.Id).Except(model.SelectedRoles).ToList();
            work.Permission.OverridePermission(model.ModuleId, notPermittedRoles, Operations.NoAccess);
            var result = work.Permission.OverridePermission(model.ModuleId, model.SelectedRoles, Operations.ReadWrite);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage("Your changes have been saved.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                return AjaxRedirectTo("/permissions");
            }
            else
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                model.Modules = work.Module.GetAutoCompleteModel(blankText: false).Modules;
                return PartialView("_CustomPermission", model);
            }
        }
    }
}