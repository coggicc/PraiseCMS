using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SermonTopics")]
    public class SermonTopic : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";
    }
}