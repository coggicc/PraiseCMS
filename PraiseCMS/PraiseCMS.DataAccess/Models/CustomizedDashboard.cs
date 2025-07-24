using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("CustomizedDashboards")]
    public class CustomizedDashboard
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Template Id")]
        public string TemplateId { get; set; }

        [DisplayName("User Id")]
        public string UserId { get; set; }
    }
}