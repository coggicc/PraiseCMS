using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ModulePermissionsModel
    {
        public string Type { get; set; }
        public string TypeId { get; set; }
        public string ModuleId { get; set; }
        public int OperationId { get; set; }
        public string ParentModuleId { get; set; }
        public bool Prop { get; set; }
        public List<string> RoleIds { get; set; }
        public bool HasListValue(string value, List<string> list)
        {
            return list.Any(x => x == value);
        }
    }

    public class CustomPermissionModel
    {
        public CustomPermissionModel()
        {
            SelectedRoles = new List<string>();
            Roles = new List<ApplicationRoles>();
            Modules = new List<ModuleAutoCompleteModel>();
        }

        public string ModuleId { get; set; }
        public List<string> SelectedRoles { get; set; }
        public List<ApplicationRoles> Roles { get; set; }
        public List<ModuleAutoCompleteModel> Modules { get; set; }
    }
}