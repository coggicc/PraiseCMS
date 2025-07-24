using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("DashboardTemplates")]
    public class DashboardTemplate : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(100)]
        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("IsCustom")]
        public bool IsCustom { get; set; }
    }
}