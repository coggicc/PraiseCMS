using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ReportCategories")]
    public class ReportCategory : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [StringLength(50)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        [NotMapped]
        public List<Report> Reports { get; set; }
    }
}