using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ServiceAreas")]
    public class ServiceArea : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        public string ChurchId { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [StringLength(250)]
        public string Requirements { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class ServiceAreaView
    {
        public ServiceArea ServiceArea { get; set; }
        public List<string> CommonServiceAreas { get; set; }
        public string[] Requirements { get; set; }
        public List<string> ChurchServiceAreaRequirements { get; set; }
    }
}