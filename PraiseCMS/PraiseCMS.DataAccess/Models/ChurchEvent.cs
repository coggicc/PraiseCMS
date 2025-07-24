using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.Shared.Methods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ChurchEvents")]
    public class ChurchEvent : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        //[DisplayName("Church Event Type Id")]
        //[Required(ErrorMessage = "Please select a church event type.")]
        public string ChurchEventTypeId { get; set; }

        public bool IsDeleted { get; set; }
        public string CustomEventName { get; set; }
        public string Description { get; set; }
        public string CalendarColor { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Frequency { get; set; }
        public string EventEnds { get; set; }
        public int? MaxOccurrences { get; set; }
        public DateTime? SpecificEndDate { get; set; }

        // Navigation properties
        //public virtual ChurchEventType EventType { get; set; } // Only ChurchEvent has a link to ChurchEventType
        public virtual Church Church { get; set; } // Only ChurchEvent has a link to Church
        public virtual ICollection<ChurchEventTime> EventTimes { get; set; }
        public virtual ICollection<Campus> Campuses { get; set; }

        [NotMapped]
        public string DisplayName { get; set; }
    }

    #region New View Models
    public class EventDashboard
    {
        public List<EventViewModel> UpComingEvents { get; set; }
        public EventsByDate EventsByDate { get; set; }
    }

    public class EventsByDate
    {
        public List<EventSD> Events { get; set; }
        public DateTime Date { get; set; }
    }

    public class ChurchEventViewModel
    {
        public ChurchEvent Event { get; set; }
        public ChurchEventTypesViewModel EventType { get; set; }
        public List<ChurchEventTypesViewModel> EventTypes { get; set; }
        public List<string> Campuses { get; set; }

        //new properties for calendar rework. above should be removed
        public string Id { get; set; }
        public string ChurchId { get; set; }
        public string CalendarColor { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string TypeId { get; set; }
        public string CampusId { get; set; }
        public string CampusName { get; set; }
        public bool AllDay { get; set; }
        public string ShowEventAt { get; set; }
        public string HideEventAt { get; set; }
        public bool Complete { get; set; }
        public string EventTimeId { get; set; }
        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";
    }
    public class ChurchEventDetail
    {
        public ChurchEventDetail()
        {
            CampusWithTime = new List<ChurchEventScheduler>();
            Event = new ChurchEvent();
        }

        public ChurchEvent Event { get; set; }
        //public List<string> CampusIds => Event.CampusId.IsNotNullOrEmpty() ? Event.CampusId.SplitToList().ToList() : new List<string>();
        public List<ChurchEventScheduler> CampusWithTime { get; set; }
    }

    public class DuplicateTimeModel
    {
        public List<ChurchEventScheduler> ChurchEventSchedulers { get; set; }
        public ChurchEvent ChurchEvent { get; set; }
        public string CampusId { get; set; }
        public string SelectedItem { get; set; }
    }
    #endregion

    public class ChurchEventCheckIn
    {
        public ChurchEventCheckIn()
        {
            CheckIn = new CheckIn();
        }

        public string Households { get; set; }
        public CheckIn CheckIn { get; set; }
    }

    public class ChurchEventView
    {
        public ChurchEventView()
        {
            ChurchEvents = new List<ChurchEvent>();
            ChurchEventTypes = new List<ChurchEventType>();
            CurrentChurchEvent = new ChurchEvent();
        }

        [Required(ErrorMessage = "Please select at least one Campus.")]
        public List<string> Campuses { get; set; }
        public ChurchEvent CurrentChurchEvent { get; set; }
        public List<ChurchEvent> ChurchEvents { get; set; }
        public List<ChurchEventType> ChurchEventTypes { get; set; }
    }

    public class ChurchEventWithSchedulerVM
    {
        public ChurchEventWithSchedulerVM()
        {
            Campuses = new List<Campus>();
            ChurchEvent = new ChurchEvent();
            ChurchEventScheduler = new List<ChurchEventScheduler>();
        }

        public List<Campus> Campuses { get; set; }
        public ChurchEvent ChurchEvent { get; set; }
        public List<ChurchEventScheduler> ChurchEventScheduler { get; set; }
    }
}