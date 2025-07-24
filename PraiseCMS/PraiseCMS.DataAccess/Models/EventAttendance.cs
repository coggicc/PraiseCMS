using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("EventAttendance")]
    public class EventAttendance : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church Event")]
        public string ChurchEventId { get; set; }

        [DisplayName("Count")]
        public int Count { get; set; }
    }
}