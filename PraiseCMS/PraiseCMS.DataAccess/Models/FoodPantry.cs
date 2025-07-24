using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("FoodPantry")]
    public class FoodPantry : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [DisplayName("Category")]
        public string CategoryId { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Image")]
        public string Image { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [DisplayName("Count")]
        public int Count { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}