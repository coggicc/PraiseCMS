using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class EventViewModel
    {
        public string Id { get; set; }

        public string EventId { get; set; }

        public string CampusId { get; set; }

        public List<string> Campuses { get; set; } // Capture multiple selected campuses

        public string CampusName { get; set; }

        public string Title { get; set; }

        public string CustomName { get; set; }

        public string Description { get; set; }

        public string StartDate { get; set; }

        public string StartTime { get; set; }

        public string EndDate { get; set; }

        public string EndTime { get; set; }

        public bool AllDay { get; set; }

        public string CalendarColor { get; set; }

        public string Type { get; set; }

        public string TypeId { get; set; }

        public string CreatedBy { get; set; }

        public string ChurchId { get; set; }

        public bool IsStandard { get; set; }

        public string Frequency { get; set; }

        public string EventEnds { get; set; }

        public int? MaxOccurrences { get; set; }

        public string SpecificEndDate { get; set; }

        public string RepeatEveryCount { get; set; }

        public string RepeatEveryType { get; set; }

        public List<string> SelectedWeekdays { get; set; }

        public List<SelectListItem> StandardEventTypes { get; set; } // List of standard event types

        // Campus scheduling fields (used for multi-campus churches)
        public List<CampusTimeViewModel> CampusTimes { get; set; } // Multiple campus times
    }

    public class CampusTimeViewModel
    {
        public string CampusId { get; set; } // Campus ID (GUID)
        public List<CampusScheduleViewModel> Times { get; set; } // Multiple times per campus
    }

    public class CampusScheduleViewModel
    {
        public string DayOfWeek { get; set; } // e.g., Sunday, Monday
        public string StartTime { get; set; } // Start time as string (e.g., "09:00 AM")
        public string EndTime { get; set; } // End time as string (e.g., "10:30 AM")
    }
}