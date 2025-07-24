using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class PermissionOperations : GenericRepository
    {
        public PermissionOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Permissions Get(string id)
        {
            return Read<Permissions>().FirstOrDefault(x => x.Id == id);
        }

        public Permissions Get(IEnumerable<string> ids)
        {
            return Read<Permissions>().FirstOrDefault(x => ids.Contains(x.Id));
        }

        public PermissionViewModel GetDashboard(string churchId, bool isSuperAdmin)
        {
            return new PermissionViewModel
            {
                ApplicationUsers = isSuperAdmin ? Work.User.GetApplicationUsersWithRoles() : Work.User.GetApplicationUsersWithRoles(churchId),
                ApplicationRoles = DAL.ReadAllRolesWithModules(churchId),
                SubscriptionTypes = Work.Subscription.GetAllSubscriptionTypes(),
                Modules = Work.Module.GetAll()
            };
        }

        public PermissionViewModel LoadPermissions(ModulePermissionsModel model)
        {
            var result = new PermissionViewModel();
            var data = Read<Modules>().Where(x => x.Id == model.ModuleId || x.ParentId == model.ModuleId).OrderBy(x => x.Name).ToList();
            var parent = data.Where(x => x.Id == model.ModuleId).ToList();
            var child = data.Where(x => x.ParentId == model.ModuleId).ToList();

            result.Modules.AddRange(parent);
            result.Modules.AddRange(child);

            if (model.Type.Equals(PermissionType.User.ToString()))
            {
                result.ApplicationSingleUser = Work.User.Get(model.TypeId);
            }
            else if (model.Type.Equals(PermissionType.Role.ToString()))
            {
                result.ApplicationSingleRole = DAL.ReadRoleById(model.TypeId);
            }
            else if (model.Type.Equals(PermissionType.Subscription.ToString()))
            {
                result.SubscriptionType = Work.Subscription.GetSubscriptionType(model.Type);
            }

            if (result.Modules.Count > 0)
            {
                var moduleIds = result.Modules.Select(x => x.Id).ToList();

                if (model.Type.Equals(PermissionType.Role.ToString()))
                {
                    result.Permissions = Read<Permissions>().Where(x => moduleIds.Contains(x.ModuleId) && x.Type == model.Type && x.TypeId == model.TypeId).ToList();
                }
                else if (model.Type.Equals(PermissionType.User.ToString()))
                {
                    model.RoleIds = DAL.ReadUserRolesByUserId(model.TypeId).Select(x => x.RoleId).ToList();
                    var roles = DAL.ReadRolesByRoleIds(model.RoleIds);
                    var permissions = Read<Permissions>().Where(x => moduleIds.Contains(x.ModuleId)
                                       && model.RoleIds.Contains(x.TypeId) && x.Type.Equals(PermissionType.Role.ToString())).ToList().Select(q => new Permissions
                                       {
                                           Id = q.Id,
                                           ModuleId = q.ModuleId,
                                           ModuleValue = q.ModuleValue,
                                           OperationId = q.OperationId,
                                           Type = q.Type,
                                           TypeId = q.TypeId,
                                           Role = roles.Any(x => x.Id.Equals(q.TypeId)) ? roles.Find(x => x.Id.Equals(q.TypeId)).Name : string.Empty
                                       }).OrderByDescending(q => q.OperationId).ToList();

                    var rolesWithModule = permissions.GroupBy(x => x.ModuleId).Select(p => new Permissions
                    {
                        Role = string.Join(", ", p.Where(q => q.OperationId.Equals(p.Select(s => s.OperationId).Max())).OrderBy(q => q.Role).Select(q => q.Role)),
                        ModuleId = p.Key
                    }).ToList();

                    //get top access permission if the user gets multiple permissions for the same module because of having more than one role
                    result.Permissions = permissions.GroupBy(x => x.ModuleId, (k, g) => g.Aggregate((a, x)
                                => x.OperationId > a.OperationId ? x : a)).Select(q => new Permissions
                                {
                                    Id = q.Id,
                                    ModuleId = q.ModuleId,
                                    ModuleValue = q.ModuleValue,
                                    OperationId = q.OperationId,
                                    Type = q.Type,
                                    TypeId = q.TypeId,
                                    Role = rolesWithModule.Any(r => r.ModuleId.Equals(q.ModuleId)) ? rolesWithModule.Find(r => r.ModuleId.Equals(q.ModuleId)).Role : q.Role
                                }).ToList();

                    var userPermissions = Read<Permissions>().Where(x => moduleIds.Contains(x.ModuleId) && x.TypeId == model.TypeId).ToList();
                    result.Permissions.AddRange(userPermissions);
                }
                else if (model.Type.Equals(PermissionType.Subscription.ToString()))
                {
                    result.Permissions = Read<Permissions>().Where(x => moduleIds.Contains(x.ModuleId) && x.Type == model.Type && x.TypeId == model.TypeId).ToList();
                }
            }

            result.Module = model;

            return result;
        }

        public void UpdatePermission(ModulePermissionsModel model)
        {
            if (model.Type == PermissionType.Role.ToString())
            {
                var roleModule = Work.Role.GetRoleModule(model.Type, model.ParentModuleId);

                if (model.Prop)
                {
                    if (roleModule == null && model.ModuleId == model.ParentModuleId)
                    {
                        Create(new RoleModules
                        {
                            Id = Utilities.GenerateUniqueId(),
                            ModuleId = model.ParentModuleId,
                            RoleId = model.TypeId
                        });
                        SaveChanges();
                    }
                }
                else
                {
                    if (roleModule != null && model.ParentModuleId == model.ModuleId)
                    {
                        Delete(roleModule);
                        SaveChanges();
                    }
                }
            }

            var permission = Read<Permissions>().FirstOrDefault(x => x.Type == model.Type && x.TypeId == model.TypeId && x.ModuleId == model.ModuleId);
            if (!model.Prop)
            {
                var moduleIds = Read<Modules>().Where(x => x.ParentId == model.ModuleId).Select(x => x.Id).ToList();

                if (moduleIds.Count > 0)
                {
                    var childModules = Read<Modules>().Where(x => moduleIds.Contains(x.ParentId)).Select(x => x.Id).ToList();
                    childModules.AddRange(moduleIds);
                    childModules = childModules.Distinct().ToList();

                    if (childModules.Count > 0)
                    {
                        var childPermissions = Read<Permissions>().Where(x => x.TypeId == model.TypeId && childModules.Contains(x.ModuleId)).ToList();
                        Delete<Permissions>(childPermissions);
                        SaveChanges();
                    }
                }
            }

            if (permission != null)
            {
                if (model.Prop)
                {
                    permission.OperationId = model.OperationId;
                    SaveChanges();
                }
                else
                {
                    Delete<Permissions>(permission);
                    SaveChanges();
                }
            }
            else
            {
                Create(new Permissions
                {
                    Id = Utilities.GenerateUniqueId(),
                    ModuleId = model.ModuleId,
                    ModuleValue = null,
                    OperationId = model.OperationId,
                    Type = model.Type,
                    TypeId = model.TypeId
                });

                var moduleIds = Read<Modules>().Where(x => x.ParentId == model.ModuleId).Select(x => x.Id).ToList();

                if (moduleIds.Count > 0)
                {
                    var childModules = Read<Modules>().Where(x => moduleIds.Contains(x.ParentId)).Select(x => x.Id).ToList();
                    childModules.AddRange(moduleIds);
                    childModules = childModules.Distinct().ToList();

                    if (childModules.Count > 0)
                    {
                        var oldPermissions = Read<Permissions>().Where(x => x.Type == model.Type && x.TypeId == model.TypeId && childModules.Contains(x.ModuleId)).ToList();

                        if (oldPermissions.Any())
                        {
                            Delete<Permissions>(oldPermissions);
                        }

                        Create<Permissions>(childModules.Select(x => new Permissions
                        {
                            Id = Utilities.GenerateUniqueId(),
                            ModuleId = x,
                            ModuleValue = null,
                            OperationId = model.OperationId,
                            Type = model.Type,
                            TypeId = model.TypeId
                        }).ToList());
                    }
                }

                SaveChanges();
            }
        }

        public PermissionViewModel GetUserRole(string churchId, string userId)
        {
            return new PermissionViewModel
            {
                ApplicationSingleUser = Work.User.Get(userId),
                ApplicationRoles = DAL.ReadAllRoles(churchId)
            };
        }

        public List<ApplicationRoles> ReadAllRolesWithModules(string churchId)
        {
            return DAL.ReadAllRolesWithModules(churchId);
        }

        public ApplicationRoles ReadRoleById(string id)
        {
            return DAL.ReadRoleById(id);
        }

        #region CRUD
        public void InsertRole(ApplicationRoles model, string churchId)
        {
            DAL.InsertRole(new ApplicationRoles()
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ChurchId = churchId
            });
        }

        public string InsertUserRole(string userId, string roleId)
        {
            return DAL.InsertUserRole(new AspNetUserRoles { UserId = userId, RoleId = roleId });
        }

        public void UpdateRole(ApplicationRoles model, string churchId)
        {
            DAL.UpdateRole(new ApplicationRoles { Id = model.Id, Name = model.Name, Description = model.Description, ChurchId = churchId });
        }

        public bool DeleteUserRole(string userId, string roleId)
        {
            return DAL.DeleteUserRole(new AspNetUserRoles { UserId = userId, RoleId = roleId });
        }

        public Result<string> DeleteRoleById(string id)
        {
            return DAL.DeleteRoleById(id);
        }

        public Result<Permissions> Delete(Permissions entity)
        {
            try
            {
                Delete<Permissions>(entity);
                SaveChanges();
                return new Result<Permissions>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Permissions>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<RoleModules> DeleteRoleModule(RoleModules entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<RoleModules>
                {
                    Message = "Role Module deleted.",
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<RoleModules>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void RemovePermission(string permissionId)
        {
            Delete(Get(permissionId));
            SaveChanges();
        }

        public void RemovePermission(Permissions permission)
        {
            Delete(permission);
            SaveChanges();
        }

        public void RemovePermission(IEnumerable<string> permissionIds)
        {
            Delete(Get(permissionIds));
            SaveChanges();
        }
        #endregion

        public List<Permissions> GetByModuleId(string moduleId)
        {
            return Read<Permissions>().Where(x => x.ModuleId == moduleId).ToList();
        }

        public List<RoleModules> GetRoleModulesRoleByModuleId(string moduleId)
        {
            return Read<RoleModules>().Where(x => x.ModuleId == moduleId).ToList();
        }

        public Result<ResponseModel> OverridePermission(string moduleId, List<string> roles, int operationId)
        {
            try
            {
                if (roles.Any() && moduleId.IsNotNullOrEmpty() && operationId.IsNotNullOrEmpty())
                {
                    var modules = new List<string> { moduleId };
                    var childModule = Read<Modules>().Where(x => !string.IsNullOrEmpty(x.ParentId) && x.ParentId.Equals(moduleId)).Select(q => q.Id).ToList();

                    if (childModule.Any())
                    {
                        modules.AddRange(childModule);
                        var subChildModule = Read<Modules>().Where(x => !string.IsNullOrEmpty(x.ParentId) && childModule.Contains(x.ParentId)).Select(q => q.Id).ToList();

                        if (subChildModule.Any())
                        {
                            modules.AddRange(subChildModule);
                        }
                    }

                    var oldPermission = Read<Permissions>().Where(x => modules.Contains(x.ModuleId) && x.Type.Equals(PermissionType.Role.ToString()) && roles.Contains(x.TypeId)).ToList();

                    if (oldPermission.Any())
                    {
                        Delete<Permissions>(oldPermission);
                    }

                    //set new override permission
                    foreach (var item in modules)
                    {
                        Create<Permissions>(roles.Select(x => new Permissions
                        {
                            Id = Utilities.GenerateUniqueId(),
                            ModuleId = item,
                            ModuleValue = null,
                            OperationId = operationId,
                            Type = "Role",
                            TypeId = x
                        }).ToList());
                    }

                    SaveChanges();
                }
                return new Result<ResponseModel>
                {
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ResponseModel>
                {
                    ResultType = ResultType.Failure,
                    Exception = ex,
                    Message = $"{Constants.DefaultErrorMessage} {ex.Message}"
                };
            }
        }
    }
}