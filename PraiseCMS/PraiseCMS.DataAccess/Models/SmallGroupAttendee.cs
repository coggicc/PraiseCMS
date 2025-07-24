using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SmallGroupAttendee")]
    public class SmallGroupAttendee
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Attendee Id")]
        public string UserId { get; set; }

        [Required]
        public int GroupId { get; set; }
    }
}