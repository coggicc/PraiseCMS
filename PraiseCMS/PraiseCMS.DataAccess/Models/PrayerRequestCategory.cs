using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("PrayerRequestCategories")]
    public class PrayerRequestCategory : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}