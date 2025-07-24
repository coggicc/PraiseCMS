using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("WidgetPermissions")]
    public class WidgetPermission
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Widget")]
        public string WidgetId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }
    }
}