using PraiseCMS.DataAccess.Models.ViewModels;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models
{
    public class PermissionViewModel
    {
        public PermissionViewModel()
        {
            Modules = new List<Modules>();
            ApplicationRoles = new List<ApplicationRoles>();
            ApplicationSingleUser = new ApplicationUser();
            ApplicationUsers = new List<ApplicationUser>();
            Permissions = new List<Permissions>();
            ApplicationSingleRole = new ApplicationRoles();
            Module = new ModulePermissionsModel();
        }

        public ModulePermissionsModel Module { get; set; }
        public List<ApplicationRoles> ApplicationRoles { get; set; }
        public List<ApplicationUser> ApplicationUsers { get; set; }
        public ApplicationUser ApplicationSingleUser { get; set; }
        public ApplicationRoles ApplicationSingleRole { get; set; }
        public IEnumerable<SubscriptionType> SubscriptionTypes { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public List<Modules> Modules { get; set; }
        public List<Permissions> Permissions { get; set; }
    }
}