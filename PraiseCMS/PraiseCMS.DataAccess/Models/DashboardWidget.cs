using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("DashboardWidgets")]
    public class DashboardWidget
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Template Id")]
        public string TemplateId { get; set; }

        [DisplayName("Widget Id")]
        public string WidgetId { get; set; }
    }
}