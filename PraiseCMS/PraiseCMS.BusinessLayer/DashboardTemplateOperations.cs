using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Mapper;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class DashboardTemplateOperations : GenericRepository
    {
        public DashboardTemplateOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Dashboard Template
        public DashboardTemplate Get(string id)
        {
            return Read<DashboardTemplate>().FirstOrDefault(x => x.Id == id);
        }

        public List<DashboardTemplate> GetAll()
        {
            return Read<DashboardTemplate>().ToList();
        }

        public List<DashboardTemplate> GetAllDefault()
        {
            return Read<DashboardTemplate>().Where(x => x.ChurchId.IsNullOrEmpty() && x.CreatedBy.Equals(Constants.System)).OrderBy(x => x.Name).ToList();
        }

        //Get templates based on a list of roles and all system templates
        public List<DashboardTemplate> GetAllForRoles(List<string> roles)
        {
            var templateIds = new List<string>();

            //Get dashboard template permissions for the role list
            if (roles.IsNotNull() && roles.Count > 0)
            {
                templateIds = Read<DashboardTemplatePermission>().Where(x => roles.Contains(x.RoleId)).Select(x => x.TemplateId).Distinct().ToList();
            }

            //Get dashboards based on the dashboard template permissions
            return Read<DashboardTemplate>().Where(x => templateIds.Contains(x.Id)).ToList();
        }

        public List<DashboardTemplate> GetAllForUser(string userId)
        {
            var result = new List<DashboardTemplate>();

            if (!string.IsNullOrEmpty(userId))
            {
                result = Read<DashboardTemplate>().Where(x => x.CreatedBy == userId).ToList();
            }

            return result;
        }

        public DashboardTemplate GetAllForUserCustom(string userId)
        {
            return Read<DashboardTemplate>().FirstOrDefault(x => x.CreatedBy.Equals(userId) && x.IsCustom);
        }

        #region CRUD Dashboard Templates
        public Result<DashboardTemplate> Create(DashboardTemplate entity)
        {
            try
            {
                Create<DashboardTemplate>(entity);
                SaveChanges();
                return new Result<DashboardTemplate>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<DashboardTemplate>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<DashboardTemplateCreateEditModel> CreateEdit(DashboardTemplateCreateEditModel entity)
        {
            try
            {
                var action = "added";
                if (entity.DashboardTemplate.Id.IsNullOrEmpty())
                {
                    entity.DashboardTemplate.Id = Utilities.GenerateUniqueId();
                    Create(entity.DashboardTemplate);
                }
                else
                {
                    action = "updated";
                    Update(entity.DashboardTemplate);

                    var oldDashboardWidgets = GetAllDashboardWidgets(entity.DashboardTemplate.Id);
                    DeleteDashboardWidget(oldDashboardWidgets);
                }

                foreach (var wid in entity.SelectedWidgets)
                {
                    Create(new DashboardWidget
                    {
                        Id = Utilities.GenerateUniqueId(),
                        TemplateId = entity.DashboardTemplate.Id,
                        WidgetId = wid
                    });
                    SaveChanges();
                }

                #region Updating Permissions
                // Update Permissions table here
                var oldDashboardPermissions = GetDashboardTemplatePermissions(entity.DashboardTemplate.Id);
                DeleteDashboardTemplatePermission(oldDashboardPermissions);

                foreach (var roleId in entity.SelectedRoles)
                {
                    Create(new DashboardTemplatePermission
                    {
                        Id = Utilities.GenerateUniqueId(),
                        TemplateId = entity.DashboardTemplate.Id,
                        RoleId = roleId
                    });
                    SaveChanges();
                }
                #endregion

                return new Result<DashboardTemplateCreateEditModel>
                {
                    Data = entity,
                    Message = $"'{entity.DashboardTemplate.Name}' dashboard template has been {action}.",
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<DashboardTemplateCreateEditModel>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultIcon = ResultIcon.Failure,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<DashboardTemplate> Update(DashboardTemplate entity)
        {
            try
            {
                var template = Work.DashboardTemplate.Get(entity.Id);

                template.Name = entity.Name;
                template.ModifiedBy = entity.ModifiedBy;
                template.ModifiedDate = entity.ModifiedDate;
                template.ChurchId = entity.ChurchId;

                Update<DashboardTemplate>(template);
                SaveChanges();
                return new Result<DashboardTemplate>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<DashboardTemplate>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<DashboardTemplate> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<DashboardTemplate>(entity);
                SaveChanges();

                //Delete associations between widgets and this dashboard
                var dashboardWidget = Work.DashboardTemplate.GetAllDashboardWidgets(id);
                if (dashboardWidget.Any())
                {
                    Work.DashboardTemplate.DeleteDashboardWidget(dashboardWidget);
                }

                return new Result<DashboardTemplate>
                {
                    Message = "The dashboard has been deleted.",
                    ResultIcon = ResultIcon.Success,
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<DashboardTemplate>
                {
                    Exception = ex,
                    ResultIcon = ResultIcon.Failure,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public DashboardTemplateCreateEditModel GetCreateEditModel(string templateId, List<ApplicationRoles> roles, bool IsSuperAdmin)
        {
            var model = new DashboardTemplateCreateEditModel
            {
                WidgetVM = new WidgetViewModel
                {
                    Widgets = IsSuperAdmin ? Work.DashboardTemplate.GetAllWidgets() : Work.DashboardTemplate.GetAllWidgetsByRoles(roles),
                    WidgetCategoryTypes = IsSuperAdmin ? Work.DashboardTemplate.GetAllWidgetCategoryTypesDefault() : Work.DashboardTemplate.GetWidgetCategoryTypesByRoles(roles),
                    WidgetCategories = Work.DashboardTemplate.GetAllWidgetCategories(),
                },
                Roles = DAL.ReadAllRoles()
            };

            if (templateId.IsNotNullOrEmpty())
            {
                model.DashboardTemplate = Work.DashboardTemplate.Get(templateId);
                model.DashboardWidgets = Work.DashboardTemplate.GetAllDashboardWidgets(model.WidgetVM.Widgets.Select(x => x.Id).ToList(), templateId);
                model.DashboardTemplatePermissions = GetDashboardTemplatePermissions(templateId);
            }

            return model;
        }
        #endregion

        #region Dashboard Template Permissions
        public List<DashboardTemplatePermission> GetDashboardTemplatePermissions(string templateId)
        {
            return Read<DashboardTemplatePermission>().Where(x => x.TemplateId == templateId).ToList();
        }

        public List<DashboardTemplatePermission> GetDashboardTemplatePermissionsByRoles(List<string> roles)
        {
            List<DashboardTemplatePermission> templatePermissiosns = new List<DashboardTemplatePermission>();

            if (roles.IsNotNull() && roles.Count > 0)
            {
                templatePermissiosns = Read<DashboardTemplatePermission>().Where(x => roles.Contains(x.RoleId)).ToList();
            }

            return templatePermissiosns;
        }

        public Result<List<DashboardTemplatePermission>> DeleteDashboardTemplatePermission(List<DashboardTemplatePermission> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    Delete(entity);
                    SaveChanges();
                }

                return new Result<List<DashboardTemplatePermission>>
                {
                    Data = entities,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<List<DashboardTemplatePermission>>
                {
                    Data = entities,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Dashboard Widgets
        public List<DashboardWidget> GetAllDashboardWidgets(string templateId)
        {
            return Read<DashboardWidget>().Where(x => x.TemplateId == templateId).ToList();
        }

        public List<DashboardWidget> GetAllDashboardWidgets(List<string> widgetIds, string templateId)
        {
            return Read<DashboardWidget>().Where(x => widgetIds.Contains(x.WidgetId) && x.TemplateId == templateId).ToList();
        }

        public List<DashboardWidget> GetAllDashboardWidgetsByWidget(string widgetId)
        {
            return Read<DashboardWidget>().Where(x => x.WidgetId == widgetId).ToList();
        }

        public Result<DashboardWidget> DeleteDashboardWidget(IEnumerable<DashboardWidget> entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<DashboardWidget> { ResultType = ResultType.Success };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<DashboardWidget>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Widgets
        public Widget GetWidget(string id)
        {
            return Read<Widget>().FirstOrDefault(x => x.Id == id);
        }

        public List<Widget> GetAllWidgets()
        {
            return Read<Widget>().OrderBy(x => x.Name).ToList();
        }

        public List<Widget> GetWidgetsByTemplate(string templateId, string widgetLocation = null)
        {
            var widgetsQuery = Read<Widget>()
                .Join(Read<DashboardWidget>(),
                      widget => widget.Id,
                      dashboardWidget => dashboardWidget.WidgetId,
                      (widget, dashboardWidget) => new { widget, dashboardWidget })
                .Where(x => x.dashboardWidget.TemplateId == templateId);

            if (!string.IsNullOrEmpty(widgetLocation))
            {
                widgetsQuery = widgetsQuery.Where(x => x.widget.Location == widgetLocation);
            }

            return widgetsQuery.Select(x => x.widget).Distinct().ToList();
        }

        public List<Widget> GetWidgetsByTemplateIds(IEnumerable<string> templateIds)
        {
            var widgetIds = Read<DashboardWidget>().Where(x => templateIds.Contains(x.TemplateId)).Select(q => q.WidgetId).Distinct().ToList();
            return Read<Widget>().Where(q => widgetIds.Contains(q.Id)).ToList();
        }

        public List<Widget> GetAllWidgetsByRoles(List<ApplicationRoles> roles)
        {
            var roleIds = roles.Select(x => x.Id).Distinct().ToList();
            var widgetIds = Read<WidgetPermission>().Where(x => roleIds.Contains(x.TypeId) && x.Type == WidgetPermissionType.Role).Select(x => x.WidgetId).Distinct().ToList();

            return Read<Widget>().Where(x => widgetIds.Contains(x.Id)).OrderBy(x => x.Name).Distinct().ToList();
        }

        #region CRUD Widgets
        public Result<Widget> CreateUpdateWidget(CreateEditWidgetVM entity)
        {
            try
            {
                if (entity.Widget.Id.IsNullOrEmpty())
                {
                    entity.Widget.Id = Utilities.GenerateUniqueId();
                    Create(entity.Widget);
                }
                else
                {
                    Update(entity.Widget);
                }
                SaveChanges();

                UpdateWidgetCategories(entity);
                UpdateWidgetPermissions(entity);

                return new Result<Widget>
                {
                    Data = entity.Widget,
                    Message = Constants.SavedMessage,
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Widget>
                {
                    Data = entity.Widget,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Widget> DeleteWidget(string id)
        {
            try
            {
                var entity = GetWidget(id);
                Delete(entity);
                SaveChanges();

                var dashboardWidgetSortOrders = GetDashboardWidgetSortOrderByWidget(id);

                if (dashboardWidgetSortOrders.Count > 0 && dashboardWidgetSortOrders.IsNotNull())
                {
                    foreach (var item in dashboardWidgetSortOrders)
                    {
                        Delete(item);
                        SaveChanges();
                    }
                }

                var widgetPermission = GetAllWidgetPermissions(id);

                if (widgetPermission.Count > 0 && widgetPermission.IsNotNull())
                {
                    foreach (var item in widgetPermission)
                    {
                        Delete(item);
                        SaveChanges();
                    }
                }

                var dashboardWidget = GetAllDashboardWidgetsByWidget(id);

                if (dashboardWidget.Count > 0 && dashboardWidget.IsNotNull())
                {
                    foreach (var item in dashboardWidget)
                    {
                        Delete(item);
                        SaveChanges();
                    }
                }

                return new Result<Widget>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Widget>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #endregion

        #region Widget Category Types
        public WidgetCategoryType GetWidgetCategoryType(string id)
        {
            return Read<WidgetCategoryType>().FirstOrDefault(x => x.Id == id);
        }

        public List<WidgetCategoryType> GetAllWidgetCategoryTypes()
        {
            return Read<WidgetCategoryType>().OrderBy(x => x.Name).ToList();
        }

        public List<WidgetCategoryType> GetAllWidgetCategoryTypesDefault()
        {
            return Read<WidgetCategoryType>().Where(x => x.CreatedBy == Constants.System).OrderBy(x => x.Name).ToList();
        }

        public List<WidgetCategoryType> GetWidgetCategoryTypesByRoles(List<ApplicationRoles> roles)
        {
            var roleIds = roles.Select(x => x.Id).Distinct().ToList();
            var categoryIds = Read<WidgetCategoryRole>().Where(x => roleIds.Contains(x.RoleId)).Select(x => x.CategoryTypeId).Distinct().ToList();

            return Read<WidgetCategoryType>().Where(x => categoryIds.Contains(x.Id)).OrderBy(x => x.Name).Distinct().ToList();
        }

        public Result<WidgetCategoryType> CreateEditWidgetCategory(CreateEditWidgetCategoryVM entity)
        {
            try
            {
                var message = string.Empty;
                if (entity.WigetCategoryType.Id.IsNullOrEmpty())
                {
                    entity.WigetCategoryType.Id = Utilities.GenerateUniqueId();
                    Create(entity.WigetCategoryType);
                    message = "The category has been created.";
                }
                else
                {
                    Update(entity.WigetCategoryType);
                    message = "The category has been updated.";
                }

                // Update WidgetCategoryRoles table here
                var categoryRoles = GetAllWidgetCategoryRoles(entity.WigetCategoryType.Id);

                if (categoryRoles.Count > 0 && categoryRoles.IsNotNull())
                {
                    Delete<WidgetCategoryRole>(categoryRoles);
                }

                Create(entity.SelectedRoles.Select(x => new WidgetCategoryRole
                {
                    CategoryTypeId = entity.WigetCategoryType.Id,
                    Id = Utilities.GenerateUniqueId(),
                    RoleId = x
                }));

                SaveChanges();

                return new Result<WidgetCategoryType>
                {
                    Data = entity.WigetCategoryType,
                    Message = message,
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<WidgetCategoryType>
                {
                    Data = entity.WigetCategoryType,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Widget Category Roles
        public List<WidgetCategoryRole> GetAllWidgetCategoryRoles(string categoryId)
        {
            return Read<WidgetCategoryRole>().Where(x => x.CategoryTypeId == categoryId).ToList();
        }
        #endregion

        #region Widget Permissions
        public List<WidgetPermission> GetAllWidgetPermissions(string widgetId)
        {
            return Read<WidgetPermission>().Where(x => x.WidgetId == widgetId).ToList();
        }

        private void UpdateWidgetPermissions(CreateEditWidgetVM entity)
        {
            var widgetPermissions = GetAllWidgetPermissions(entity.Widget.Id);

            Delete<WidgetPermission>(widgetPermissions);

            foreach (var item in entity.SelectedRoles)
            {
                Create(new WidgetPermission
                {
                    WidgetId = entity.Widget.Id,
                    Id = Utilities.GenerateUniqueId(),
                    Type = WidgetPermissionType.Role,
                    TypeId = item
                });
            }
            SaveChanges();
        }
        #endregion

        #region Widget Categories
        public List<WidgetCategory> GetAllWidgetCategories()
        {
            return Read<WidgetCategory>().ToList();
        }

        public List<WidgetCategory> GetAllWidgetCategories(List<ApplicationRoles> roles)
        {
            var roleIds = roles.Select(x => x.Id).Distinct().ToList();
            var categoryIds = Read<WidgetCategoryRole>().Where(x => roleIds.Contains(x.RoleId)).Select(x => x.CategoryTypeId).Distinct().ToList();

            return Read<WidgetCategory>().Where(x => categoryIds.Contains(x.CategoryTypeId)).Distinct().ToList();
        }

        public List<WidgetCategory> GetAllWidgetCategoriesByWidget(string widgetId)
        {
            return Read<WidgetCategory>().Where(x => x.WidgetId == widgetId).ToList();
        }

        private void UpdateWidgetCategories(CreateEditWidgetVM entity)
        {
            var widgetCategories = Read<WidgetCategory>()
                .Where(x => x.WidgetId == entity.Widget.Id)
                .ToList();

            Delete<WidgetCategory>(widgetCategories);

            foreach (var item in entity.SelectedCategories)
            {
                Create(new WidgetCategory
                {
                    WidgetId = entity.Widget.Id,
                    Id = Utilities.GenerateUniqueId(),
                    CategoryTypeId = item
                });
            }
            SaveChanges();
        }
        #endregion

        #region Sorting
        public List<DashboardWidgetSortOrder> GetDashboardWidgetSortOrderByWidget(string widgetId)
        {
            return Read<DashboardWidgetSortOrder>().Where(x => x.WidgetId == widgetId).ToList();
        }

        public List<DashboardWidgetSortOrder> GetDashboardWidgetSortOrders(string userId, string templateId)
        {
            return Read<DashboardWidgetSortOrder>()
                .Where(x => x.UserId == userId && x.TemplateId == templateId)
                .ToList();
        }

        //public List<WidgetSortable> GetDashboardWidgetSortOrder(string userId, string templateId, string widgetLocation)
        //{
        //    var widgets = GetWidgetsByTemplate(templateId, widgetLocation);
        //    var sorting = GetDashboardWidgetSortOrders(userId, templateId, widgetLocation);

        //    var result = new List<WidgetSortable>();

        //    // Create dictionaries for quick lookup
        //    var widgetsDict = widgets.ToDictionary(w => w.Id);
        //    var sortingDict = sorting.GroupBy(s => s.WidgetId)
        //                             .Select(g => g.First())
        //                             .ToDictionary(s => s.WidgetId);

        //    // Add sorted widgets
        //    foreach (var sortOrder in sortingDict.Values)
        //    {
        //        if (widgetsDict.TryGetValue(sortOrder.WidgetId, out var widget))
        //        {
        //            result.Add(new WidgetSortable
        //            {
        //                SortOrder = sortOrder.SortOrder,
        //                Widget = widget
        //            });
        //        }
        //    }

        //    // Add remaining unsorted widgets
        //    var currentSortOrder = sorting.Count > 0 ? sorting.Max(s => s.SortOrder) + 1 : 1;
        //    foreach (var widget in widgets)
        //    {
        //        if (!sortingDict.ContainsKey(widget.Id))
        //        {
        //            result.Add(new WidgetSortable
        //            {
        //                SortOrder = currentSortOrder++,
        //                Widget = widget
        //            });
        //        }
        //    }

        //    // Sort the final list by SortOrder
        //    return result.OrderBy(x => x.SortOrder).ToList();
        //}

        public List<WidgetSortable> GetDashboardWidgetSortOrder(string userId, string templateId)
        {
            var widgets = GetWidgetsByTemplate(templateId);
            var sorting = GetDashboardWidgetSortOrders(userId, templateId);

            var result = new List<WidgetSortable>();

            // Create dictionaries for quick lookup
            var widgetsDict = widgets.ToDictionary(w => w.Id);
            var sortingDict = sorting.GroupBy(s => s.WidgetId)
                                     .Select(g => g.First())
                                     .ToDictionary(s => s.WidgetId);

            // Add sorted widgets
            foreach (var sortOrder in sortingDict.Values)
            {
                if (widgetsDict.TryGetValue(sortOrder.WidgetId, out var widget))
                {
                    result.Add(new WidgetSortable
                    {
                        SortOrder = sortOrder.SortOrder,
                        Widget = widget
                    });
                }
            }

            // Add remaining unsorted widgets
            var currentSortOrder = sorting.Count > 0 ? sorting.Max(s => s.SortOrder) + 1 : 1;
            foreach (var widget in widgets)
            {
                if (!sortingDict.ContainsKey(widget.Id))
                {
                    result.Add(new WidgetSortable
                    {
                        SortOrder = currentSortOrder++,
                        Widget = widget
                    });
                }
            }

            // Sort the final list by SortOrder
            return result.OrderBy(x => x.SortOrder).ToList();
        }

        public ManageLayoutVM GetManageLayoutDashboard(string userId, string templateId)
        {
            var mainWidgets = GetDashboardWidgetSortOrder(userId, templateId)
                                .Where(x => x.Widget.Location.Equals(WidgetLocations.Main))
                                .ToList();

            var tileWidgets = GetDashboardWidgetSortOrder(userId, templateId)
                                .Where(x => x.Widget.Location.Equals(WidgetLocations.Top))
                                .ToList();

            return new ManageLayoutVM
            {
                MainWidgetSortable = mainWidgets,
                TileWidgetSortable = tileWidgets,
                DashboardTemplate = Get(templateId)
            };
        }

        public List<WidgetSortable> GetActiveWidgetSortable(string userId)
        {
            var settings = Work.UserSetting.GetByUserId(userId);

            if (settings.IsNotNull() && settings.DashboardTemplateId.IsNotNullOrEmpty())
            {
                var widgets = GetManageLayoutDashboard(userId, settings.DashboardTemplateId);

                var result = widgets.MainWidgetSortable;
                result.AddRange(widgets.TileWidgetSortable);

                return result;
            }

            return GetDefaultWidgetSortable(userId);
        }

        public List<WidgetSortable> GetDefaultWidgetSortable(string userId)
        {
            var roles = DAL.ReadUserRolesByUserId(userId).Select(x => x.RoleId).ToList();

            if (roles.IsNotNull() && roles.Count > 0)
            {
                var templateId = GetDashboardTemplatePermissionsByRoles(roles).Select(x => x.TemplateId).FirstOrDefault();

                if (templateId.IsNotNull())
                {
                    return GetDashboardWidgetSortOrder(userId, templateId);
                }
            }
            return new List<WidgetSortable>();
        }

        //public List<WidgetSortable> GetDashboardWidgetSortOrder(string userId, string templateId, string widgetLocation)
        //{
        //    var result = new List<WidgetSortable>();

        //    var widgets = GetWidgetsByTemplate(templateId, widgetLocation);

        //    var sorting = GetDashboardWidgetSortOrders(userId, templateId, widgetLocation);

        //    if (sorting.Count > 0)
        //    {
        //        foreach (var item in sorting)
        //        {
        //            result.Add(new WidgetSortable
        //            {
        //                SortOrder = item.SortOrder,
        //                Widget = widgets.FirstOrDefault(x => x.Id == item.WidgetId)
        //            });
        //        }

        //        if (widgets.Count > sorting.Count)
        //        {
        //            foreach (var item in widgets)
        //            {
        //                var newWidget = result.FirstOrDefault(x => x.Widget.Id == item.Id);

        //                if (newWidget.IsNull())
        //                {
        //                    result.Add(new WidgetSortable
        //                    {
        //                        SortOrder = sorting.Count,
        //                        Widget = widgets.FirstOrDefault(x => x.Id == item.Id)
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var i = 1;

        //        foreach (var item in widgets)
        //        {
        //            result.Add(new WidgetSortable
        //            {
        //                SortOrder = i,
        //                Widget = item
        //            });
        //            i++;
        //        }
        //    }

        //    if (result.Count > 0)
        //    {
        //        result.OrderBy(x => x.SortOrder).ToList();
        //    }

        //    return result;
        //}

        public void UpdateDashboardWidgetSortOrder(string userId, string templateId, List<string> data)
        {
            if (data == null) return;

            // Clear existing sort orders for the templateId and userId
            var existingSortOrders = GetDashboardWidgetSortOrders(userId, templateId);

            if (existingSortOrders.Count > 0)
            {
                Delete<DashboardWidgetSortOrder>(existingSortOrders);
            }

            // Create new sort orders for all widgets in 'data'
            Create(data.Select(x => new DashboardWidgetSortOrder
            {
                Id = Utilities.GenerateUniqueId(),
                TemplateId = templateId,
                UserId = userId,
                WidgetId = x.Split('^')[0],
                SortOrder = Convert.ToInt32(x.Split('^')[1]),
                Location = x.Split('^')[2],
            }));

            SaveChanges();
        }
        #endregion

        public DashboardTemplateVM GetDashboardForSuperAdmin(string userId, string churchId)
        {
            var templates = Work.DashboardTemplate.GetAll().OrderBy(x => x.Name).ToList();
            //add custom template added by user
            templates.AddRange(Read<DashboardTemplate>().Where(x => x.CreatedBy == userId && x.IsCustom).ToList());

            var churchIds = templates.Select(x => x.ChurchId).Distinct().ToList();

            return new DashboardTemplateVM
            {
                Churches = Work.Church.GetAll(churchIds),
                Church = Work.Church.Get(churchId),
                DashboardTemplates = templates,
                WidgetCategoryTypes = Work.DashboardTemplate.GetAllWidgetCategoryTypes(),
                UserSettings = Work.UserSetting.GetByUserId(userId),
                User = Work.User.Get(userId)
            };
        }

        public DashboardTemplateVM GetDashboardForUser(string churchId, string userId, List<ApplicationRoles> roles)
        {
            //Get templates that the user has permission to
            var templates = Work.DashboardTemplate.GetAllForRoles(roles.Select(x => x.Id).ToList());

            //Get templates created by the user
            templates.AddRange(Work.DashboardTemplate.GetAllForUser(userId));

            return new DashboardTemplateVM
            {
                Church = Work.Church.Get(churchId),
                DashboardTemplates = templates.OrderBy(x => x.Name).ToList(),
                WidgetCategoryTypes = Work.DashboardTemplate.GetWidgetCategoryTypesByRoles(roles),
                UserSettings = Work.UserSetting.GetByUserId(userId),
                User = Work.User.Get(userId)
            };
        }

        public WidgetViewModel GetWidgetsView(List<ApplicationRoles> roles, bool isSuperAdmin)
        {
            if (isSuperAdmin)
            {
                return new WidgetViewModel
                {
                    WidgetCategoryTypes = Work.DashboardTemplate.GetAllWidgetCategoryTypesDefault(),
                    WidgetCategories = Work.DashboardTemplate.GetAllWidgetCategories(),
                    Widgets = Work.DashboardTemplate.GetAllWidgets()
                };
            }

            return new WidgetViewModel
            {
                WidgetCategoryTypes = Work.DashboardTemplate.GetWidgetCategoryTypesByRoles(roles),
                WidgetCategories = Work.DashboardTemplate.GetAllWidgetCategories(roles),
                Widgets = Work.DashboardTemplate.GetAllWidgetsByRoles(roles)
            };
        }

        public CustomDashboardVM GetCustomizedDashboard(bool defaultSelected = true)
        {
            var model = new CustomDashboardVM();
            var categoryTypes = Work.DashboardTemplate.GetAllWidgetCategoryTypes();
            var widgetCategories = Work.DashboardTemplate.GetAllWidgetCategories();
            var widgets = new List<Widget>();

            var templates = Work.DashboardTemplate.GetAllForRoles(SessionVariables.Roles.Select(x => x.Id).ToList());

            if (templates.IsNotNullOrEmpty() && templates.Any())
            {
                widgets = Work.DashboardTemplate.GetWidgetsByTemplateIds(templates.Select(x => x.Id)).ToList();
            }

            model.CategoryWidgets.AddRange(categoryTypes.Select(q => new CategoryWidgets()
            {
                WidgetCategoryType = q,
                Widgets = widgets.Where(x => widgetCategories.Where(c => c.CategoryTypeId.Equals(q.Id)).Select(s => s.WidgetId).Contains(x.Id)).ToList()
            }).ToList());

            var customTemplate = Work.DashboardTemplate.GetAllForUserCustom(SessionVariables.CurrentUser.User.Id);

            if (customTemplate.IsNotNullOrEmpty())
            {
                model.CustomTemplateId = customTemplate.Id;
                model.Template = customTemplate.Name;

                if (defaultSelected)
                {
                    model.Widgets = Work.DashboardTemplate.GetAllDashboardWidgets(customTemplate.Id).Select(q => q.WidgetId).ToList();
                }
            }
            else if (defaultSelected)
            {
                model.Widgets = model.CategoryWidgets.SelectMany(x => x.Widgets).Select(x => x.Id).ToList();
            }

            return model;
        }

        public Result<DashboardTemplate> CreateUpdateCustomizedDashboard(CustomDashboardVM model)
        {
            try
            {
                var template = new DashboardTemplate();

                if (model.CustomTemplateId.IsNotNullOrEmpty())
                {
                    template.Id = model.CustomTemplateId;
                    var currentWidgets = Work.DashboardTemplate.GetAllDashboardWidgets(template.Id).Select(q => q.WidgetId).ToList();
                    var widgetsToAdd = model.Widgets.Except(currentWidgets).ToList();
                    var widgetsToRemove = currentWidgets.Except(model.Widgets).ToList();

                    Delete<DashboardWidget>(Read<DashboardWidget>().Where(q => widgetsToRemove.Contains(q.WidgetId) && q.TemplateId.Equals(template.Id)).ToList());

                    if (widgetsToAdd.Any())
                    {
                        Create<DashboardWidget>(widgetsToAdd.Select(q => new DashboardWidget()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            TemplateId = template.Id,
                            WidgetId = q
                        }).ToList());
                    }
                    SaveChanges();
                }
                else
                {
                    template = new DashboardTemplate()
                    {
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        Id = Utilities.GenerateUniqueId(),
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        Name = model.Template,
                        IsCustom = true
                    };

                    Create<DashboardWidget>(model.Widgets.Select(q => new DashboardWidget()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        TemplateId = template.Id,
                        WidgetId = q
                    }).ToList());

                    Create(new CustomizedDashboard
                    {
                        Id = Utilities.GenerateUniqueId(),
                        TemplateId = template.Id,
                        UserId = SessionVariables.CurrentUser.User.Id
                    });

                    Work.DashboardTemplate.Create(template);
                }

                return new Result<DashboardTemplate>
                {
                    Data = template,
                    Message = "Your dashboard changes have been saved.",
                    ResultIcon = ResultIcon.Success,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<DashboardTemplate>
                {
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultIcon = ResultIcon.Failure,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void Clone(string templateId, string userId, string churchId)
        {
            // Cloning template
            var template = Work.DashboardTemplate.Get(templateId);
            var clone = new DashboardTemplate
            {
                ChurchId = churchId,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                Id = Utilities.GenerateUniqueId(),
                IsCustom = false,
                Name = $"{template.Name} (Copy)"
            };
            Work.DashboardTemplate.Create(clone);

            // Cloning Widgets
            var widgets = Work.DashboardTemplate.GetAllDashboardWidgets(templateId);

            Create(widgets.Select(x => new DashboardWidget
            {
                Id = Utilities.GenerateUniqueId(),
                TemplateId = clone.Id,
                WidgetId = x.WidgetId
            }));

            //Get the original creator's widget sort order
            var superAdminEmail = ConfigurationManager.AppSettings["SuperAdminEmail"];
            var originallyCreatedBy = template.CreatedBy.Equals(Constants.System) ? Work.User.GetPraiseUser().Id : template.CreatedBy;

            // Cloning sort orders
            var sortOrders = Read<DashboardWidgetSortOrder>().Where(x => x.TemplateId == templateId && x.UserId == originallyCreatedBy).ToList();

            Create(sortOrders.Select(x => new DashboardWidgetSortOrder
            {
                Id = Utilities.GenerateUniqueId(),
                Location = x.Location,
                SortOrder = x.SortOrder,
                TemplateId = clone.Id,
                UserId = userId,
                WidgetId = x.WidgetId
            }));
            SaveChanges();
        }

        #region Home Page Dashboard
        public DashboardViewModel ConstructDashboardViewModel(Church church, string userId)
        {
            var workWeekStartDay = ExtensionMethods.GetWorkWeekStartDay(church.WorkWeek);
            var currentDate = DateTime.Now.Date;
            var dateRanges = GetDateRanges(currentDate);
            var weeklyDateRange = Utilities.GetDateRangesOfLastNumberOfWeeks(2, workWeekStartDay);

            // Get all relevant data
            var givingData = GetGivingData(church.Id, dateRanges, weeklyDateRange);
            var attendanceData = GetAttendanceData(church.Id, dateRanges);
            var baptismData = GetBaptismData(church.Id, dateRanges);
            var salvationData = GetSalvationData(church.Id, dateRanges);

            // Construct and return the view model
            var viewModel = new DashboardViewModel
            {
                Attendance = attendanceData,
                Baptisms = baptismData,
                Events = Work.Event.GetEvents(SessionVariables.CurrentChurch.Id),
                Giving = givingData,
                MyGiving = Work.Giving.GetHistory(church.Id, userId).Data,
                NewDonors = Work.Giving.NewDonors(church.Id),
                Notifications = Work.Notification.GetAllByChurchId(church.Id),
                OfflineGiving = Work.OfflineGiving.GetAll(church.Id, null, ExtensionMethods.GetCurrentYearDateRange()),
                Payments = Work.Payment.GetAll(church.Id, null, ExtensionMethods.GetCurrentYearDateRange()),
                PrayerRequests = Work.PrayerRequest.GetAll(church.Id, ExtensionMethods.GetCurrentYearDateRange()),
                RecentDeaths = Work.Person.GetDeceasedPeople(church.Id, ExtensionMethods.GetLastMonthDateRange()),
                Salvations = salvationData,
                SmallGroups = Work.SmallGroup.GetAll(church.Id, ExtensionMethods.GetCurrentYearDateRange()),
                UpcomingBirthdays = Work.Person.GetUpcomingBirthdays(SessionVariables.CurrentChurch.Id),
                CurrentWeeksAttendance = GetCurrentWeeksAttendance(attendanceData, workWeekStartDay),
                WeeklyAttendanceComparison = GetWeeklyAttendanceComparison(attendanceData, workWeekStartDay),
                CurrentWeeksGivingAmount = GetCurrentWeeksGivingAmount(givingData, workWeekStartDay),
                WeeklyGivingComparison = GetWeeklyGivingComparison(givingData, workWeekStartDay),
                CurrentWeeksBaptisms = GetCurrentWeeksBaptisms(baptismData, workWeekStartDay),
                WeeklyBaptismComparison = GetWeeklyBaptismComparison(baptismData, workWeekStartDay),
                CurrentWeeksSalvations = GetCurrentWeeksSalvations(salvationData, workWeekStartDay),
                WeeklySalvationComparison = GetWeeklySalvationComparison(salvationData, workWeekStartDay)
            };

            return viewModel;
        }

        private (DateRange currentYear, DateRange lastWeekOfPreviousYear) GetDateRanges(DateTime currentDate)
        {
            var currentYearStartDate = new DateTime(currentDate.Year, 1, 1);
            var previousYearEndDate = new DateTime(currentDate.Year - 1, 12, 31);

            var lastWeekOfPreviousYearStartDate = previousYearEndDate.AddDays(-(int)previousYearEndDate.DayOfWeek - 6);
            var lastWeekOfPreviousYearEndDate = lastWeekOfPreviousYearStartDate.AddDays(6);

            return (
                new DateRange { StartDate = currentYearStartDate, EndDate = currentDate },
                new DateRange { StartDate = lastWeekOfPreviousYearStartDate, EndDate = lastWeekOfPreviousYearEndDate }
            );
        }

        private List<MyGivingVM> GetGivingData(string churchId, (DateRange currentYear, DateRange lastWeekOfPreviousYear) dateRanges, DateRange weeklyDateRange)
        {
            var allOfflineGivingCurrentYear = Work.OfflineGiving.GetAll(churchId, null, dateRanges.currentYear);
            var allPaymentsCurrentYear = Work.Payment.GetAll(churchId, null, dateRanges.currentYear);
            var allOfflineGivingPreviousYear = Work.OfflineGiving.GetAll(churchId, null, dateRanges.lastWeekOfPreviousYear);
            var allPaymentsPreviousYear = Work.Payment.GetAll(churchId, null, dateRanges.lastWeekOfPreviousYear);

            var allGivingCurrentYear = Mapper.Map(allPaymentsCurrentYear).Concat(Mapper.Map(allOfflineGivingCurrentYear)).ToList();
            var allGivingPreviousYear = Mapper.Map(allPaymentsPreviousYear).Concat(Mapper.Map(allOfflineGivingPreviousYear)).ToList();

            return allGivingCurrentYear.Concat(allGivingPreviousYear)
                .Where(x => x.CreatedDate.Date >= weeklyDateRange.StartDate.Date && x.CreatedDate.Date <= weeklyDateRange.EndDate.Date)
                .ToList();
        }

        private List<T> GetData<T>(string churchId, (DateRange currentYear, DateRange lastWeekOfPreviousYear) dateRanges, Func<string, DateRange, List<T>> getAllData)
        {
            var currentYearData = getAllData(churchId, dateRanges.currentYear);
            var previousYearData = getAllData(churchId, dateRanges.lastWeekOfPreviousYear);

            return currentYearData.Concat(previousYearData).ToList();
        }

        private List<Attendance> GetAttendanceData(string churchId, (DateRange currentYear, DateRange lastWeekOfPreviousYear) dateRanges)
        {
            return GetData(churchId, dateRanges, (id, dateRange) => Work.Attendance.GetAll(id, dateRange));
        }

        private List<Baptism> GetBaptismData(string churchId, (DateRange currentYear, DateRange lastWeekOfPreviousYear) dateRanges)
        {
            return GetData(churchId, dateRanges, (id, dateRange) => Work.Baptism.GetAll(id, dateRange));
        }

        private List<Salvation> GetSalvationData(string churchId, (DateRange currentYear, DateRange lastWeekOfPreviousYear) dateRanges)
        {
            return GetData(churchId, dateRanges, (id, dateRange) => Work.Salvation.GetAll(id, dateRange));
        }

        private string GetCurrentWeeksGivingAmount(List<MyGivingVM> givingData, string workWeek)
        {
            var currentWeekDates = ExtensionMethods.GetDatesOfWeek(DateTime.Now, workWeek);
            return givingData
                .Where(x => currentWeekDates.Contains(x.CreatedDate.Date))
                .Sum(a => a.Amount)
                .ToCurrencyString();
        }

        private string GetCurrentWeeksTotal<T>(List<T> data, string workWeek, Func<T, DateTime?> getOccurredOnDate, Func<T, int> getTotal)
        {
            var currentWeekDates = ExtensionMethods.GetDatesOfWeek(DateTime.Now, workWeek);
            return data
                .Where(x => getOccurredOnDate(x).HasValue && currentWeekDates.Contains(getOccurredOnDate(x).Value.Date))
                .Sum(getTotal)
                .ToString();
        }

        private string GetCurrentWeeksAttendance(List<Attendance> attendanceData, string workWeek)
        {
            return GetCurrentWeeksTotal(attendanceData, workWeek, x => x.OccurredOnDate, x => x.Total);
        }

        private string GetCurrentWeeksBaptisms(List<Baptism> baptismData, string workWeek)
        {
            return GetCurrentWeeksTotal(baptismData, workWeek, x => x.OccurredOnDate, x => x.Total);
        }

        private string GetCurrentWeeksSalvations(List<Salvation> salvationData, string workWeek)
        {
            return GetCurrentWeeksTotal(salvationData, workWeek, x => x.OccurredOnDate, x => x.Total);
        }

        private WeeklyGivingComparisonViewModel GetWeeklyGivingComparison(List<MyGivingVM> givingData, string workWeek)
        {
            var currentWeekDates = ExtensionMethods.GetDatesOfWeek(DateTime.Now, workWeek);
            var lastWeekDates = ExtensionMethods.GetDatesOfWeek(DateTime.Now.AddDays(-7), workWeek);

            var currentWeeksGiving = givingData
                .Where(x => currentWeekDates.Contains(x.CreatedDate.Date))
                .Sum(s => s.Amount);

            var lastWeeksGiving = givingData
                .Where(x => lastWeekDates.Contains(x.CreatedDate.Date))
                .Sum(s => s.Amount);

            var givingDifference = lastWeeksGiving > 0 ? (currentWeeksGiving - lastWeeksGiving) / lastWeeksGiving * 100 : currentWeeksGiving;

            return new WeeklyGivingComparisonViewModel
            {
                CurrentWeeksGiving = currentWeeksGiving,
                LastWeeksGiving = lastWeeksGiving,
                GivingDifference = givingDifference,
                PercentChange = Math.Abs(givingDifference) // Absolute value for percent change
            };
        }

        private WeeklyComparisonViewModel GetWeeklyComparison<TData>(
            List<TData> data,
            string workWeek,
            Func<TData, DateTime?> getOccurredOnDate,
            Func<TData, int> getTotal)
        {
            var currentWeekDates = ExtensionMethods.GetDatesOfWeek(DateTime.Now, workWeek)
                .Where(x => x.Date <= DateTime.Now.Date)
                .Select(x => x.Date)
                .ToList();

            var lastWeekDates = ExtensionMethods.GetDatesOfWeek(DateTime.Now.AddDays(-7), workWeek)
                .Where(x => currentWeekDates.Select(d => d.DayOfWeek).Contains(x.DayOfWeek))
                .Select(x => x.Date)
                .ToList();

            var currentWeekTotal = data
                .Where(x => getOccurredOnDate(x).HasValue && currentWeekDates.Contains(getOccurredOnDate(x).Value.Date))
                .Sum(x => getTotal(x)); // Keep as int

            var lastWeekTotal = data
                .Where(x => getOccurredOnDate(x).HasValue && lastWeekDates.Contains(getOccurredOnDate(x).Value.Date))
                .Sum(x => getTotal(x)); // Keep as int

            // Calculate difference
            var difference = lastWeekTotal > 0 ? currentWeekTotal - lastWeekTotal : currentWeekTotal;

            // Calculate percentage change as a decimal
            var percentChange = lastWeekTotal > 0
                ? (decimal)(currentWeekTotal - lastWeekTotal) / lastWeekTotal * 100
                : (currentWeekTotal > 0 ? 100 : 0);

            return new WeeklyComparisonViewModel
            {
                CurrentWeekCount = currentWeekTotal,
                LastWeekCount = lastWeekTotal,
                Difference = difference,
                PercentChange = Math.Abs(percentChange) // Absolute value for percent change
            };
        }

        private WeeklyComparisonViewModel GetWeeklyAttendanceComparison(List<Attendance> attendanceData, string workWeek)
        {
            return GetWeeklyComparison(
                attendanceData,
                workWeek,
                x => x.OccurredOnDate,
                x => x.Total
            );
        }

        private WeeklyComparisonViewModel GetWeeklyBaptismComparison(List<Baptism> baptismData, string workWeek)
        {
            return GetWeeklyComparison(
                baptismData,
                workWeek,
                x => x.OccurredOnDate,
                x => x.Total
            );
        }

        private WeeklyComparisonViewModel GetWeeklySalvationComparison(List<Salvation> salvationData, string workWeek)
        {
            return GetWeeklyComparison(
                salvationData,
                workWeek,
                x => x.OccurredOnDate,
                x => x.Total
            );
        }
        #endregion
    }

    public static class DashboardSeedData
    {
        public static readonly List<DashboardTemplate> DefaultDashboards = new List<DashboardTemplate>
        {
            new DashboardTemplate
            {
                Id = "405829766987294ce337fc465fba88",
                Name = "Accounting Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            },
            new DashboardTemplate
            {
                Id = "8331402281923671a996b1412ab9d1",
                Name = "Admin Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            },
            new DashboardTemplate
            {
                Id = "51415774053a713e47406942dd9795",
                Name = "Associate Pastor Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            },
            new DashboardTemplate
            {
                Id = "55971489080ecad87a8e0a4cf9a89a",
                Name = "Pastor Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            },
            new DashboardTemplate
            {
                Id = "612557597406eac23712ee44e6835f",
                Name = "Senior Pastor Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            },
            new DashboardTemplate
            {
                Id = "65668943458cf4bbffb2f3484a926a",
                Name = "System Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            },
            new DashboardTemplate
            {
                Id = "77577512498d39474d49e04962b05e",
                Name = "Youth Pastor Dashboard - Default",
                ChurchId = null,
                IsCustom = false,
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            }
        };
    }
}