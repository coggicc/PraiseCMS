using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ChurchEventTimes")]
    public class ChurchEventTime : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public TimeSpan StartTime { get; set; } // Time of day when the event starts (e.g., 9:00 AM)

        public TimeSpan EndTime { get; set; } // Time of day when the event ends

        [DisplayName("Show Event At")]
        public string ShowEventAt { get; set; }

        [DisplayName("Hide Event At")]
        public string HideEventAt { get; set; }

        [DisplayName("All Day")]
        public bool AllDay { get; set; }

        [DisplayName("Is Deleted")]
        public bool IsDeleted { get; set; }

        [DisplayName("EndDate")]
        public DateTime? EndDate { get; set; }

        public string ChurchEventId { get; set; } // Reference to the ChurchEvent

        public string CampusId { get; set; } // Reference to the campus for this schedule

        public DayOfWeek? DayOfWeek { get; set; } // The day of the week (e.g., Sunday), if applicable

        // Navigation properties
        public virtual ChurchEvent Event { get; set; } // Reference to the event

        public virtual Campus Campus { get; set; } // Reference to the campus

        [NotMapped]
        public bool ShowMultiday { get; set; }
    }
}