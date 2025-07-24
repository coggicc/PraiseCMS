using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("FixedReports")]
    public class FixedReport : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Report Category Id")]
        [Required]
        public string ReportCategoryId { get; set; }

        [StringLength(50)]
        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [StringLength(500)]
        [DisplayName("URL")]
        public string URL { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}