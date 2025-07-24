using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("WidgetCategories")]
    public class WidgetCategory
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Widget")]
        public string WidgetId { get; set; }

        [DisplayName("Category")]
        public string CategoryTypeId { get; set; }
    }
}