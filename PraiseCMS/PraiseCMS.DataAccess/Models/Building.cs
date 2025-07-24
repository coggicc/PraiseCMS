using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Buildings")]
    public class Building : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [Required(ErrorMessage = "Please select a campus.")]
        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [Required(ErrorMessage = "Please enter a name.")]
        [DisplayName("Building Name")]
        public string BuildingName { get; set; }

        public string Display => !string.IsNullOrEmpty(BuildingName) ? BuildingName : Constants.DisplayDefaultText;
    }

    public class BuildingViewModel
    {
        public string BuildingId { get; set; }
        public string ChurchId { get; set; }
        public string CampusId { get; set; }
        public string BuildingName { get; set; }
        public string Display => !string.IsNullOrEmpty(BuildingName) ? BuildingName : Constants.DisplayDefaultText;
    }
}