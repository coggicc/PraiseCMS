using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("EmailCampaigns")]
    public class EmailCampaign : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [StringLength(50)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }
    }
}