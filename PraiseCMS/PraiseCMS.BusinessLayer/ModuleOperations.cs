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
    public class ModuleOperations : GenericRepository
    {
        public ModuleOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Modules Get(string id)
        {
            return Read<Modules>().FirstOrDefault(x => x.Id == id);
        }

        public Modules GetByName(string name)
        {
            return name.IsNotNullOrEmpty() ? Read<Modules>().FirstOrDefault(x => x.Name.ToLower().Trim().Equals(name.ToLower().Trim())) : null;
        }

        public List<Modules> GetAll()
        {
            return Read<Modules>().ToList();
        }

        public List<Modules> GetAll(string parentId)
        {
            return Read<Modules>().Where(x => x.ParentId == parentId).ToList();
        }

        public List<Modules> GetAllByIdAndParent(string id, string parentId)
        {
            return Read<Modules>().Where(x => x.Id == id && x.ParentId == parentId).ToList();
        }

        #region CRUD
        public Result<Modules> Create(Modules entity)
        {
            try
            {
                Create<Modules>(entity);
                SaveChanges();
                return new Result<Modules>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Modules>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Modules> Update(Modules entity)
        {
            try
            {
                Update<Modules>(entity);
                SaveChanges();
                return new Result<Modules>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Modules>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Modules> Delete(Modules entity)
        {
            try
            {
                var hasChild = Work.Module.HasChild(entity.Id);

                if (hasChild)
                {
                    return new Result<Modules>
                    {
                        Message = $"Please delete child module(s) before deleting this module. ({entity.Name})",
                        ResultType = ResultType.Failure
                    };
                }

                foreach (var r in Work.Permission.GetRoleModulesRoleByModuleId(entity.Id))
                {
                    Work.Permission.DeleteRoleModule(r);
                }

                foreach (var p in Work.Permission.GetByModuleId(entity.Id))
                {
                    Work.Permission.Delete(p);
                }

                Delete<Modules>(entity);
                SaveChanges();
                return new Result<Modules>
                {
                    Message = $"The module ({entity.Name}) has been deleted.",
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Modules>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Modules> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                var hasChild = Work.Module.HasChild(id);

                if (hasChild)
                {
                    return new Result<Modules>
                    {
                        Message = $"Please delete child module(s) before deleting this module. ({entity.Name})",
                        ResultType = ResultType.Failure
                    };
                }

                foreach (var r in Work.Permission.GetRoleModulesRoleByModuleId(id))
                {
                    Work.Permission.DeleteRoleModule(r);
                }

                foreach (var p in Work.Permission.GetByModuleId(id))
                {
                    Work.Permission.Delete(p);
                }

                Delete<Modules>(entity);
                SaveChanges();

                return new Result<Modules>
                {
                    Message = $"The module ({entity.Name}) has been deleted.",
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Modules>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public bool HasChild(string moduleId)
        {
            return Read<Modules>().Any(x => x.ParentId == moduleId);
        }

        public ModuleAutoCompleteModel GetAutoCompleteModel(bool blankText = true)
        {
            var model = new ModuleAutoCompleteModel
            {
                Value = Utilities.GenerateUniqueId()
            };

            var modules = GetAll();

            if (blankText)
            {
                model.Modules.Add(new ModuleAutoCompleteModel { Text = string.Empty, Value = null });
            }

            foreach (var parent in modules.Where(x => x.ParentId == null).ToList())
            {
                var p = new ModuleAutoCompleteModel
                {
                    Value = parent.Id,
                    Text = parent.Name
                };
                model.Modules.Add(p);

                foreach (var module in modules.Where(x => x.ParentId == parent.Id).ToList())
                {
                    var m = new ModuleAutoCompleteModel
                    {
                        Value = module.Id,
                        Text = $"{parent.Name} > {module.Name}"
                    };
                    model.Modules.Add(m);

                    foreach (var child in modules.Where(x => x.ParentId == module.Id).ToList())
                    {
                        var c = new ModuleAutoCompleteModel
                        {
                            Value = child.Id,
                            Text = $"{parent.Name} > {module.Name} > {child.Name}"
                        };
                        model.Modules.Add(c);
                    }
                }
            }

            return model;
        }

        public void AddEdit(ModuleViewModel model)
        {
            // Get all modules
            var allModules = Work.Module.GetAll();

            // Filter modules where Id or ParentId matches model.Id
            var result = allModules.Where(x => x.Id == model.Id || x.ParentId == model.Id).ToList();

            // Find the specific module to edit
            var module = result.FirstOrDefault(x => x.Id == model.Id);
            var hasChild = result.Any(x => x.ParentId == model.Id);

            if (module != null)
            {
                // Update module properties
                module.Name = model.Name;

                // Update ParentId based on conditions
                if ((module.ParentId == null && !hasChild) || module.ParentId != null)
                {
                    module.ParentId = model.ParentId;
                }

                Update(module);
            }
        }

        public ModuleAutoCompleteModel GetAutoCompleteModel(string moduleId)
        {
            var result = new ModuleAutoCompleteModel();
            var modules = GetAll();

            result.Modules.Add(new ModuleAutoCompleteModel { Text = string.Empty, Value = null });

            foreach (var parent in modules.Where(x => x.ParentId == null).ToList())
            {
                var p = new ModuleAutoCompleteModel
                {
                    Value = parent.Id,
                    Text = parent.Name
                };
                result.Modules.Add(p);

                foreach (var module in modules.Where(x => x.ParentId == parent.Id).ToList())
                {
                    var m = new ModuleAutoCompleteModel
                    {
                        Value = module.Id,
                        Text = $"{parent.Name} > {module.Name}"
                    };
                    result.Modules.Add(m);
                }
            }

            var editModule = modules.FirstOrDefault(x => x.Id == moduleId);

            if (editModule != null)
            {
                result.ParentId = editModule.ParentId;
                result.Text = editModule.Name;
                result.Value = editModule.Id;
            }

            return result;
        }

        public void SetModulePermissionForStandardPlan(Modules module)
        {
            var standardSubscription = Read<SubscriptionType>().FirstOrDefault(q => q.Name.Equals(PlanType.Premium) && q.IsActive);

            if (standardSubscription.IsNullOrEmpty())
            {
                return;
            }

            if (module.ParentId.IsNotNullOrEmpty())
            {
                var type = Convert.ToString(PermissionType.Subscription);
                var parent = Read<Permissions>().FirstOrDefault(x => x.ModuleId == module.ParentId && x.Type.Equals(type) && x.TypeId.Equals(standardSubscription.Id));

                if (parent.IsNull())
                {
                    Create(new Permissions
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ModuleId = module.ParentId,
                        ModuleValue = null,
                        OperationId = Operations.ReadWrite,
                        Type = Convert.ToString(PermissionType.Subscription),
                        TypeId = standardSubscription.Id
                    });
                }
            }

            if (!module.Id.Equals(module.ParentId))
            {
                Create(new Permissions
                {
                    Id = Utilities.GenerateUniqueId(),
                    ModuleId = module.Id,
                    ModuleValue = null,
                    OperationId = Operations.ReadWrite,
                    Type = Convert.ToString(PermissionType.Subscription),
                    TypeId = standardSubscription.Id
                });
            }

            SaveChanges();
        }

        public void SetModulePermissionForSuperAdmin(string moduleId, bool createRoleModule = false)
        {
            var rolesIds = Db.Roles.Where(x => !Roles.LimitedAccessItems.Contains(x.Name)).Select(x => x.Id).ToList();

            Db.Permissions.AddRange(rolesIds.Select(q => new Permissions
            {
                Id = Utilities.GenerateUniqueId(),
                ModuleId = moduleId,
                ModuleValue = null,
                OperationId = Operations.ReadWrite,
                Type = nameof(PermissionType.Role),
                TypeId = q
            }).ToList());

            if (createRoleModule)
            {
                Db.RoleModules.AddRange(rolesIds.Select(q => new RoleModules
                {
                    Id = Utilities.GenerateUniqueId(),
                    ModuleId = moduleId,
                    RoleId = q
                }).ToList());
            }
            Db.SaveChanges();
        }
    }
}