using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models
{
    public class SessionUser
    {
        public ApplicationUser User { get; set; }
        public List<ApplicationRoles> Roles { get; set; }
    }
}