using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class CalendarOperations : GenericRepository
    {
        public CalendarOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public void CreateEventOnDragAndDrop(EventViewModel viewModel)
        {
            var churchEvent = new ChurchEvent()
            {
                Id = viewModel.Id, // The unique event ID generated on the client side
                ChurchId = viewModel.ChurchId,
                ChurchEventTypeId = viewModel.TypeId,
                StartDate = Convert.ToDateTime(viewModel.StartDate),
                EndDate = string.IsNullOrEmpty(viewModel.EndDate) ? (DateTime?)null : Convert.ToDateTime(viewModel.EndDate),
                Frequency = viewModel.Frequency,
                EventEnds = viewModel.EventEnds,
                MaxOccurrences = viewModel.MaxOccurrences,
                CreatedDate = DateTime.Now,
                CreatedBy = viewModel.CreatedBy,
                //CampusId = viewModel.CampusId,
                IsDeleted = false
            };

            // Create EventTime objects for each campus and its associated schedules (multiple times per campus)
            var eventTimes = new List<ChurchEventTime>();

            foreach (var campusTime in viewModel.CampusTimes)
            {
                foreach (var schedule in campusTime.Times)
                {
                    var eventTime = new ChurchEventTime
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchEventId = churchEvent.Id,
                        CampusId = campusTime.CampusId,
                        StartTime = TimeSpan.Parse(schedule.StartTime), // Convert string to TimeSpan
                        EndTime = TimeSpan.Parse(schedule.EndTime), // Convert string to TimeSpan
                        DayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), schedule.DayOfWeek) // Convert string to DayOfWeek enum
                    };

                    eventTimes.Add(eventTime);
                }
            }

            // Save the event and the times
            Work.ChurchEvents.Create(churchEvent);
            Work.ChurchEvents.CreateTime(eventTimes);

            //var schedulers = SessionVariables.Campuses.Select(x => new ChurchEventScheduler()
            //{
            //    Id = Utilities.GenerateUniqueId(),
            //    CampusId = x.Id,
            //    CreatedBy = viewModel.CreatedById,
            //    CreatedDate = DateTime.Now,
            //    EventId = viewModel.Id,
            //    Frequency = ChurchEventFrequency.DoesNotRepeat,
            //    StartDate = Convert.ToDateTime(viewModel.StartDate),
            //    EventEnds = EventEnds.OnSpecificDate,
            //    EndDate = Convert.ToDateTime(viewModel.StartDate)
            //}).ToList();

            //var times = schedulers.Select(x => new ChurchEventTime()
            //{
            //    Id = Utilities.GenerateUniqueId(),
            //    ChurchEventSchedulerId = x.Id,
            //    IsDeleted = false,
            //    CreatedBy = viewModel.CreatedById,
            //    CreatedDate = DateTime.Now,
            //    StartTime = DateTime.Now.AddHours(1).AddMinutes(30).ToString("hh:mm tt"),
            //    EndTime = DateTime.Now.AddHours(2).AddMinutes(30).ToString("hh:mm tt"),
            //    ShowEventAt = DateTime.Now.AddMinutes(30).ToString("hh:mm tt"),
            //    HideEventAt = DateTime.Now.AddHours(3).ToString("hh:mm tt"),
            //}).ToList();

            //Work.ChurchEvents.Create(model);
            //Work.ChurchEvents.CreateScheduler(schedulers);
            //Work.ChurchEvents.CreateTime(times);
        }

        public void CreateEvent(EventViewModel viewModel)
        {
            string churchEventTypeId;

            // Admins can create new ChurchEventTypes when IsStandard is true
            if (viewModel.IsStandard)
            {
                // Create a new ChurchEventType as a standard type
                var eventType = new ChurchEventType
                {
                    Id = Utilities.GenerateUniqueId(),
                    Type = viewModel.Title,                // Base name for standard event types
                    CalendarColor = viewModel.CalendarColor,
                    CreatedDate = DateTime.Now,
                    CreatedBy = viewModel.CreatedBy,
                    IsDeleted = false
                };
                Work.ChurchEventType.Create(eventType);
                churchEventTypeId = eventType.Id;
            }
            else if (!string.IsNullOrEmpty(viewModel.TypeId))
            {
                // If a standard event type is selected, reference it by TypeId
                churchEventTypeId = viewModel.TypeId;
            }
            else
            {
                churchEventTypeId = null;
                //// Handle case where no standard type is selected (possibly a custom event only in ChurchEvents)
                //throw new InvalidOperationException("Either a standard event type must be selected, or the user must be an admin to create a new event type.");
            }

            // Step 2: Create a new ChurchEvent entry
            var churchEvent = new ChurchEvent
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = viewModel.ChurchId,
                ChurchEventTypeId = churchEventTypeId,
                // Set CustomEventName based on the given conditions
                CustomEventName = viewModel.IsStandard
                    ? (!string.IsNullOrEmpty(viewModel.CustomName) ? viewModel.CustomName : null)
                    : (!string.IsNullOrEmpty(viewModel.Title) ? viewModel.Title : null),
                //CustomEventName = !viewModel.IsStandard ? viewModel.CustomName : null, // Custom name for non-standard events
                Description = viewModel.Description,
                CalendarColor = !viewModel.IsStandard ? viewModel.CalendarColor : null, // Custom name for non-standard events
                StartDate = Convert.ToDateTime(viewModel.StartDate),
                EndDate = string.IsNullOrEmpty(viewModel.EndDate) ? (DateTime?)null : Convert.ToDateTime(viewModel.EndDate),
                Frequency = viewModel.Frequency ?? ChurchEventFrequency.DoesNotRepeat,
                EventEnds = viewModel.EventEnds,
                SpecificEndDate = string.IsNullOrEmpty(viewModel.SpecificEndDate) ? (DateTime?)null : Convert.ToDateTime(viewModel.SpecificEndDate),
                MaxOccurrences = viewModel.MaxOccurrences,
                CreatedDate = DateTime.Now,
                CreatedBy = viewModel.CreatedBy,
                IsDeleted = false
            };

            // Save the ChurchEvent
            Work.ChurchEvents.Create(churchEvent);

            // Step 3: Create ChurchEventTime entries for each campus
            List<ChurchEventTime> eventTimes = new List<ChurchEventTime>();

            // Handle each campus and each day
            if (viewModel.Campuses?.Any() == true)
            {
                foreach (var campusId in viewModel.Campuses)
                {
                    if (viewModel.SelectedWeekdays?.Any() == true)
                    {
                        foreach (var weekday in viewModel.SelectedWeekdays)
                        {
                            if (Enum.TryParse<DayOfWeek>(weekday, out var dayOfWeek))
                            {
                                eventTimes.Add(CreateEventTime(churchEvent.Id, campusId, dayOfWeek, viewModel));
                            }
                        }
                    }
                    else
                    {
                        eventTimes.Add(CreateEventTime(churchEvent.Id, campusId, null, viewModel));
                    }
                }
            }
            else
            {
                if (viewModel.SelectedWeekdays?.Any() == true)
                {
                    foreach (var weekday in viewModel.SelectedWeekdays)
                    {
                        if (Enum.TryParse<DayOfWeek>(weekday, out var dayOfWeek))
                        {
                            eventTimes.Add(CreateEventTime(churchEvent.Id, null, dayOfWeek, viewModel));
                        }
                    }
                }
                else
                {
                    eventTimes.Add(CreateEventTime(churchEvent.Id, null, null, viewModel));
                }
            }

            // Save the ChurchEventTime entries
            Work.ChurchEvents.CreateTime(eventTimes);
        }

        // Helper method to create ChurchEventTime objects
        private ChurchEventTime CreateEventTime(string churchEventId, string campusId, DayOfWeek? dayOfWeek, EventViewModel viewModel)
        {
            return new ChurchEventTime
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchEventId = churchEventId,
                CampusId = campusId,
                AllDay = viewModel.AllDay,
                StartTime = viewModel.AllDay ? TimeSpan.Zero : DateTime.Parse(viewModel.StartTime).TimeOfDay, // Midnight if AllDay
                EndTime = viewModel.AllDay ? new TimeSpan(23, 59, 59) : DateTime.Parse(viewModel.EndTime).TimeOfDay, // 11:59 PM if AllDay
                DayOfWeek = dayOfWeek,
                CreatedDate = DateTime.Now,
                CreatedBy = viewModel.CreatedBy,
                IsDeleted = false
            };
        }

        public List<ChurchEventViewModel> GetChurchEventByFrequency(string churchId, string dateRange, string campusId = null, string eventTypeId = null)
        {
            var events = new List<ChurchEventViewModel>();

            // Retrieve all campus data for the specified church
            var campuses = Work.Campus.GetAll(churchId).ToDictionary(c => c.Id, c => c.Display);

            // Query to load church-specific events and associated event times
            var churchEventsRaw = (from CE in Db.ChurchEvents
                                   join CET in Db.ChurchEventTypes on CE.ChurchEventTypeId equals CET.Id into cetJoin
                                   from CET in cetJoin.DefaultIfEmpty() // Left join to allow for ChurchEventTypeId being null
                                   join CETM in Db.ChurchEventTime on CE.Id equals CETM.ChurchEventId
                                   where CE.ChurchId == churchId && // Only events for the specified church
                                         !CE.IsDeleted && !CETM.IsDeleted &&
                                         (string.IsNullOrEmpty(campusId) || CETM.CampusId == campusId) &&
                                         (string.IsNullOrEmpty(eventTypeId) || CE.ChurchEventTypeId == eventTypeId || CE.ChurchEventTypeId == null)
                                   select new
                                   {
                                       EventId = CE.Id,
                                       ChurchId = CE.ChurchId,
                                       CalendarColor = CET != null ? CET.CalendarColor : null, // Handle potential null ChurchEventType
                                       Description = CE.Description,
                                       StartDate = CE.StartDate,
                                       EndDate = CE.EndDate,
                                       EventTimeId = CETM.Id,
                                       StartTime = CETM.StartTime,
                                       EndTime = CETM.EndTime,
                                       CustomEventName = CE.CustomEventName,
                                       Type = CET != null ? CET.Type : null, // Handle potential null ChurchEventType
                                       ChurchEventTypeId = CE.ChurchEventTypeId,
                                       CampusId = CETM.CampusId,
                                       AllDay = CETM.AllDay,
                                       ShowEventAt = CETM.ShowEventAt,
                                       HideEventAt = CETM.HideEventAt
                                   }).ToList();

            var churchEvents = churchEventsRaw.Select(evt => new ChurchEventViewModel
            {
                Id = evt.EventId,
                ChurchId = evt.ChurchId,
                CalendarColor = evt.CalendarColor,
                Description = evt.Description,
                StartDate = evt.StartDate,
                EndDate = evt.EndDate,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                Title = evt.CustomEventName ?? evt.Type, // Show custom name if available; otherwise, type
                Type = evt.Type,
                TypeId = evt.ChurchEventTypeId,
                CampusId = evt.CampusId,
                CampusName = evt.CampusId != null && campuses.ContainsKey(evt.CampusId) ? campuses[evt.CampusId] : "Campus Not Specified",
                AllDay = evt.AllDay,
                ShowEventAt = evt.ShowEventAt,
                HideEventAt = evt.HideEventAt,
                Complete = evt.EndTime < DateTime.Now.TimeOfDay,
                EventTimeId = evt.EventTimeId
            }).ToList();

            return churchEvents;
        }
    }

    public class RecurrenceService
    {
        public List<DateTime> GetEventOccurrences(ChurchEvent churchEvent, DateTime fromDate, DateTime toDate)
        {
            var occurrences = new List<DateTime>();

            // Ensure the event has a valid start date
            if (churchEvent.StartDate > toDate || (churchEvent.EndDate.HasValue && churchEvent.EndDate < fromDate))
                return occurrences; // Event does not occur in this range

            var currentDate = churchEvent.StartDate;
            int occurrencesCount = 0;

            while (currentDate <= toDate)
            {
                // If the event occurs within the requested range
                if (currentDate >= fromDate && currentDate <= toDate)
                {
                    occurrences.Add(currentDate);
                }

                // Check event frequency
                if (churchEvent.Frequency == EventFrequency.Weekly)
                {
                    currentDate = currentDate.AddDays(7); // Move to next week
                }
                else if (churchEvent.Frequency == EventFrequency.Daily)
                {
                    currentDate = currentDate.AddDays(1); // Move to next day
                }
                else
                {
                    break; // No recurrence
                }

                occurrencesCount++;

                // If the event has a limited number of occurrences, stop after reaching the max
                if (churchEvent.EventEnds == EventEnds.AfterEventOccurrences && occurrencesCount >= churchEvent.MaxOccurrences)
                    break;

                // Stop if we reach the event's end date
                if (churchEvent.EventEnds == EventEnds.OnSpecificDate && churchEvent.EndDate.HasValue && currentDate > churchEvent.EndDate.Value)
                    break;
            }

            return occurrences;
        }
    }
}