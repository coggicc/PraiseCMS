using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class DashboardTemplateVM
    {
        public DashboardTemplateVM()
        {
            DashboardTemplates = new List<DashboardTemplate>();
            Churches = new List<Church>();
            WidgetCategoryTypes = new List<WidgetCategoryType>();
            Church = new Church();
            UserSettings = new UserSetting();
            User = new ApplicationUser();
        }

        public List<DashboardTemplate> DashboardTemplates { get; set; }
        public List<WidgetCategoryType> WidgetCategoryTypes { get; set; }
        public List<Church> Churches { get; set; }
        public Church Church { get; set; }
        public UserSetting UserSettings { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class DashboardTemplateCreateEditModel
    {
        public DashboardTemplateCreateEditModel()
        {
            DashboardTemplate = new DashboardTemplate();
            WidgetVM = new WidgetViewModel();
            Roles = new List<ApplicationRoles>();
            DashboardWidgets = new List<DashboardWidget>();
            DashboardTemplatePermissions = new List<DashboardTemplatePermission>();
            SelectedWidgets = new List<string>();
            SelectedRoles = new List<string>();
        }

        public DashboardTemplate DashboardTemplate { get; set; }
        public WidgetViewModel WidgetVM { get; set; }
        public List<ApplicationRoles> Roles { get; set; }
        public List<DashboardWidget> DashboardWidgets { get; set; }
        public List<DashboardTemplatePermission> DashboardTemplatePermissions { get; set; }

        [Required]
        public List<string> SelectedWidgets { get; set; }

        [Required]
        public List<string> SelectedRoles { get; set; }
    }

    public class WidgetViewModel
    {
        public WidgetViewModel()
        {
            Widgets = new List<Widget>();
            WidgetCategoryTypes = new List<WidgetCategoryType>();
            WidgetCategories = new List<WidgetCategory>();
        }

        public List<Widget> Widgets { get; set; }
        public List<WidgetCategoryType> WidgetCategoryTypes { get; set; }
        public List<WidgetCategory> WidgetCategories { get; set; }
    }

    public class CategoryWidgets
    {
        public WidgetCategoryType WidgetCategoryType { get; set; }
        public List<Widget> Widgets { get; set; }
    }

    public class CustomDashboardVM
    {
        public CustomDashboardVM()
        {
            CategoryWidgets = new List<CategoryWidgets>();
            Widgets = new List<string>();
        }

        public List<CategoryWidgets> CategoryWidgets { get; set; }
        public List<string> Widgets { get; set; }
        [Required(ErrorMessage = "Dashboard name can not be empty.")]
        public string Template { get; set; } = "My Dashboard";
        public string CustomTemplateId { get; set; }
    }
}