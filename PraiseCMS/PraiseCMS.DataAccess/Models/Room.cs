using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Rooms")]
    public class Room : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        [Required(ErrorMessage = "Please select a campus.")]
        public string CampusId { get; set; }

        [DisplayName("Building Id")]
        [Required(ErrorMessage = "Please select a building.")]
        public string BuildingId { get; set; }
        [NotMapped]
        public Building Building { get; set; }

        [DisplayName("Floor Id")]
        [Required(ErrorMessage = "Please select a floor.")]
        public string FloorId { get; set; }
        [NotMapped]
        public Floor Floor { get; set; }

        [StringLength(50)]
        [DisplayName("Name")]
        [Required(ErrorMessage = "Please enter the room name.")]
        public string Name { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [StringLength(50)]
        [DisplayName("Phone")]
        public string Phone { get; set; }

        [DisplayName("Capacity")]
        public int? Capacity { get; set; }

        [DisplayName("Status")]
        public bool Status { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class RoomViewModel
    {
        public string RoomId { get; set; }
        public string ChurchId { get; set; }
        public string CampusId { get; set; }
        public string BuildingId { get; set; }
        public string FloorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public int? Capacity { get; set; }
        public bool Status { get; set; }
        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}