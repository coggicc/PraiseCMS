using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("AspNetRoles")]
    public class ApplicationRoles
    {
        public ApplicationRoles()
        {
            Modules = new List<Modules>();
        }

        public string Id { get; set; }

        [Required(ErrorMessage = "Please provide a name for the role.")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ChurchId { get; set; }
        public List<Modules> Modules { get; set; }
    }

    public class AspNetUserRoles
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }

    public class RoleUserCount
    {
        public string RoleId { get; set; }
        public int UserCount { get; set; }
    }
}