using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("DashboardWidgetSortOrders")]
    public class DashboardWidgetSortOrder
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("User Id")]
        public string UserId { get; set; }

        [DisplayName("Template Id")]
        public string TemplateId { get; set; }

        [DisplayName("Widget Id")]
        public string WidgetId { get; set; }

        [DisplayName("Location")]
        public string Location { get; set; }

        [DisplayName("SortOrder")]
        public int SortOrder { get; set; }
    }
}