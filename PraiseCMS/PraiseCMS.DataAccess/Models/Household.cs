using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Households")]
    public class Household : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(50)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}