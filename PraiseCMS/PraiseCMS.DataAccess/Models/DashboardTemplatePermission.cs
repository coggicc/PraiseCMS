using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("DashboardTemplatePermissions")]
    public class DashboardTemplatePermission
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Template Id")]
        public string TemplateId { get; set; }

        [DisplayName("Role Id")]
        public string RoleId { get; set; }
    }
}