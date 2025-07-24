using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Floors")]
    public class Floor : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [Required(ErrorMessage = "Please select a campus.")]
        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [Required(ErrorMessage = "Please select a building.")]
        [DisplayName("Building")]
        public string BuildingId { get; set; }

        [NotMapped]
        public Building Building { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        [DisplayName("Floor Name")]
        public string FloorName { get; set; }

        public string Display => !string.IsNullOrEmpty(FloorName) ? FloorName : Constants.DisplayDefaultText;
    }

    public class FloorViewModel
    {
        public string FloorId { get; set; }
        public string ChurchId { get; set; }
        public string CampusId { get; set; }
        public string BuildingId { get; set; }
        public string FloorName { get; set; }
        public string Display => !string.IsNullOrEmpty(FloorName) ? FloorName : Constants.DisplayDefaultText;
    }
}