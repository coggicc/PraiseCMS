using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("WidgetCategoryRoles")]
    public class WidgetCategoryRole
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Category")]
        public string CategoryTypeId { get; set; }

        [DisplayName("Role")]
        public string RoleId { get; set; }
    }
}