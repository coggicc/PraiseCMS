using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class RoleTableViewModel
    {
        public IEnumerable<ApplicationRoles> Roles { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
