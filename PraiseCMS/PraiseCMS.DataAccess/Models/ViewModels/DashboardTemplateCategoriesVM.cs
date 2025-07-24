using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class DashboardTemplateCategoriesVM
    {
        public DashboardTemplateCategoriesVM()
        {
            Categories = new List<WidgetCategoryType>();
            Users = new List<ApplicationUser>();
        }

        public List<WidgetCategoryType> Categories { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}