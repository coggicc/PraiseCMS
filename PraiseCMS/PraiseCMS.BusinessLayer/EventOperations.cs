using PraiseCMS.API.Models;
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
using System.Data.Entity.Migrations;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class EventOperations : GenericRepository
    {
        public EventOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        private int _multiDay = 0;

        public EventSD Get(string id)
        {
            return Read<EventSD>().FirstOrDefault(x => x.Id == id);
        }

        public List<Fund> GetByChurch(string churchId)
        {
            return Read<Fund>().Where(x => x.ChurchId == churchId && !x.Hidden && !x.Closed).ToList();
        }

        public List<EventViewModel> GetUpComingEvents(string churchId)
        {
            var todayDate = DateTime.Now.Date;
            var tomorrowDate = DateTime.Now.Date.AddDays(1);
            var upcomingEvents = new List<EventViewModel>();

            // Use ToDateRange with a custom string if needed
            var upcomingDateRange = $"{todayDate:MM/dd/yyyy}-{tomorrowDate:MM/dd/yyyy}".ToDateRange();

            //var churchEvents = GetChurchEventByFrequency(churchId, upcomingDateRange)
            //                       .Where(q => q.StartDate.Date.Equals(todayDate) || q.StartDate.Date.Equals(tomorrowDate))
            //                       .ToList();
            var churchEvents = Work.Calendar.GetChurchEventByFrequency(SessionVariables.CurrentChurch.Id, upcomingDateRange.ToString());

            var events = churchEvents.Select(x => new EventViewModel
            {
                Id = x.Id,
                CampusId = x.CampusId,
                CampusName = x.CampusName,
                //EventId = x.ChurchEventSchedulerId,
                Title = x.Title,
                EndDate = x.EndDate.ToString(),
                //EndTime = x.AllDay ? "09:45 PM" : x.EndTime,
                Type = x.Type,
                Description = x.Description,
                //StartTime = x.AllDay ? "05:00 AM" : x.StartTime,
                //StartDate = x.StartDate.Date == todayDate ? "Today" : "Tomorrow",
                CalendarColor = !string.IsNullOrEmpty(x.CalendarColor) ? x.CalendarColor : "primary",
                //CreatedBy = x.CreatedBy
            }).OrderBy(x => x.Title).ToList();

            if (events.Any())
            {
                upcomingEvents = events.Where(x => x.EndDate.IsNotNullOrEmpty() && x.EndTime.IsNotNullOrEmpty()
                && new DateTime(Convert.ToDateTime(x.EndDate).Year, Convert.ToDateTime(x.EndDate).Month, Convert.ToDateTime(x.EndDate).Day, Convert.ToDateTime(x.EndTime).Hour, Convert.ToDateTime(x.EndTime).Minute, Convert.ToDateTime(x.EndTime).Second) > DateTime.Now)
                    .OrderBy(o => o.StartDate).ThenBy(x => Convert.ToDateTime(x.StartTime)).ThenBy(x => x.Title).ToList();
            }

            return upcomingEvents;
        }

        public List<EventSD> GetEventsByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new List<EventSD>();
            }

            var results = Db.Event.Where(x => (x.Type.Equals(Constants.User)
                         && x.TypeId == SessionVariables.CurrentUser.User.Id) ||
                         (x.Type.Equals("Church") && x.ChurchId.Equals(SessionVariables.CurrentChurch.Id)));

            return results.Where(q => !string.IsNullOrEmpty(q.Title) && q.Title.ToUpper().Trim().Contains(name.ToUpper().Trim())).ToList();
        }

        internal List<EventSD> GetChurchEventByFrequency(string churchId, DateRange dateRange, string campusId = null, string eventTypeId = null)
        {
            var events = new List<EventSD>();

            // Retrieve all campus data for the specified church
            var campuses = Work.Campus.GetAll(churchId).ToDictionary(c => c.Id, c => c.Display);

            // Load the data into memory first to allow for additional filtering if needed
            var churchEvents = (from CETM in Db.ChurchEventTime
                                join CES in Db.ChurchEventScheduler
                                 on CETM.ChurchEventId equals CES.Id
                                join CE in Db.ChurchEvents
                                on CES.EventId equals CE.Id
                                join CET in Db.ChurchEventTypes
                                on CE.ChurchEventTypeId equals CET.Id
                                where CE.ChurchId == churchId &&
                                !CETM.IsDeleted && !CES.IsDeleted &&
                                (string.IsNullOrEmpty(campusId) || CES.CampusId == campusId) && // Filter directly on ChurchEventScheduler's CampusId
                                (string.IsNullOrEmpty(eventTypeId) || CE.ChurchEventTypeId == eventTypeId) // Filter by eventTypeId if provided
                                select new ChurchEventsViewModel
                                {
                                    ChurchEvent = CE,
                                    ChurchEventScheduler = CES,
                                    ChurchEventTime = CETM,
                                    ChurchEventType = CET
                                }).ToList(); // Force data to be loaded into memory

            // Map the campus names after loading the data into memory
            foreach (var eventItem in churchEvents)
            {
                eventItem.CampusName = campuses.ContainsKey(eventItem.ChurchEventScheduler.CampusId)
                    ? campuses[eventItem.ChurchEventScheduler.CampusId]
                    : "Unknown";
            }

            var groupedEvents = churchEvents
                                 .GroupBy(ce => ce.ChurchEventScheduler.Frequency)
                                 .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var frequencyGroup in groupedEvents)
            {
                var frequency = frequencyGroup.Key;
                var items = frequencyGroup.Value;

                var multiDays = items.Select(item => GetMultiDaySpan(item.ChurchEventScheduler.StartDate, item.ChurchEventTime.EndDate)).ToList();

                foreach (var item in items)
                {
                    var eventCounts = CalculateEventCounts(item.ChurchEventScheduler.StartDate, item.ChurchEventScheduler.EndDate, item.ChurchEventScheduler.EventEnds, item.ChurchEventScheduler.Occurrences);

                    _multiDay = multiDays.First();

                    switch (frequency)
                    {
                        case ChurchEventFrequency.Daily:
                            events.AddRange(GenerateDailyEvents(item, eventCounts, _multiDay));
                            break;
                        case ChurchEventFrequency.Weekly:
                            events.AddRange(GenerateWeeklyEvents(item, dateRange));
                            break;
                        case ChurchEventFrequency.Monthly:
                            events.AddRange(GenerateMonthlyEvents(item, eventCounts, _multiDay));
                            break;
                        case ChurchEventFrequency.Yearly:
                            events.AddRange(GenerateYearlyEvents(item, dateRange));
                            break;
                        case ChurchEventFrequency.EveryWeekday:
                            events.AddRange(GenerateWeekdayEvents(item, dateRange));
                            break;
                        case ChurchEventFrequency.Custom:
                            events.AddRange(GenerateCustomEvents(item, dateRange));
                            break;
                        case ChurchEventFrequency.DoesNotRepeat:
                            events.Add(GenerateNonRepeatingEvent(item, _multiDay));
                            break;
                    }
                }
            }

            return events;
        }

        private int GetMultiDaySpan(DateTime startDate, DateTime? endDate)
        {
            // Check if endDate has a value
            if (!endDate.HasValue)
            {
                return 0; // or another default value if necessary
            }

            // Calculate the difference in days if endDate is not null
            return (int)(endDate.Value.Date - startDate.Date).TotalDays;
        }

        public static int CalculateEventCounts(DateTime startDate, DateTime? endDate, string eventEnds, int? occurrences)
        {
            DateTime actualEndDate = endDate ?? DateTime.Now.AddYears(1);

            int eventCounts = 0;

            switch (eventEnds)
            {
                case EventEnds.AfterEventOccurrences:
                    eventCounts = occurrences ?? 0;
                    break;

                case EventEnds.OnSpecificDate:
                    eventCounts = (int)((actualEndDate - startDate).TotalDays) + 1;
                    break;

                default:
                    eventCounts = (int)((actualEndDate - startDate).TotalDays) + 1;
                    break;
            }

            return eventCounts;
        }

        private DateTime CalculateSystemEndDate(DateTime startDate, DateTime? systemEndDate, string dateRange)
        {
            if (systemEndDate == null || systemEndDate < DateTime.Now)
            {
                systemEndDate = DateTime.Now.AddYears(1);
            }
            else if (systemEndDate.Value.Date.CalculateRemainingInDays() < 30)
            {
                systemEndDate = systemEndDate.Value.AddYears(1);
            }

            if (!string.IsNullOrEmpty(dateRange))
            {
                var endDateRange = dateRange.ToDateRange().EndDate.Date;
                if (endDateRange.CalculateRemainingInDays() > systemEndDate.Value.Date.CalculateRemainingInDays())
                {
                    systemEndDate = endDateRange;
                }
            }

            return systemEndDate.Value;
        }

        private List<EventSD> GenerateDailyEvents(ChurchEventsViewModel item, int eventCounts, int multiDay)
        {
            var sequence = Enumerable.Range(0, eventCounts).ToArray();

            return sequence.Select(x => new EventSD
            {
                ChurchId = SessionVariables.CurrentChurch.Id,
                CalendarColor = item.ChurchEventType.CalendarColor,
                //Description = item.ChurchEventType.Description,
                Id = item.ChurchEventTime.Id,
                ChurchEventId = item.ChurchEvent.Id,
                ChurchEventSchedulerId = item.ChurchEventScheduler.Id,
                CreatedBy = item.ChurchEvent.CreatedBy,
                CreatedDate = item.ChurchEvent.CreatedDate,
                StartDate = item.ChurchEventScheduler.StartDate.AddDays(x),
                EndDate = item.ChurchEventScheduler.StartDate.AddDays(x).AddDays(multiDay),
                //StartTime = item.ChurchEventTime.StartTime,
                //EndTime = item.ChurchEventTime.EndTime,
                Title = $"{(!string.IsNullOrEmpty(item.ChurchEventType.Type) ? item.ChurchEventType.Type : item.ChurchEventType.Type)}",
                Type = item.ChurchEventType?.Display,
                TypeId = item.ChurchEventType?.Id,
                CampusId = item.ChurchEventScheduler.CampusId,
                CampusName = item.CampusName,
                AllDay = item.ChurchEventTime.AllDay,
                ShowEventAt = item.ChurchEventTime.ShowEventAt,
                HideEventAt = item.ChurchEventTime.HideEventAt
            }).ToList();
        }

        private List<EventSD> GenerateWeeklyEvents(ChurchEventsViewModel item, DateRange dateRange)
        {
            var dateList = new List<DateTime>();
            var fullWeek = ExtensionMethods.GetDatesOfWeek(item.ChurchEventScheduler.StartDate);

            // Ensure RepeatOn is not null and has a default value if necessary
            var selectedDays = item.ChurchEventScheduler.RepeatOn?.Split(',')?.Select(int.Parse).ToArray() ?? new int[0];
            var selectedDaysDates = fullWeek.Where(q => selectedDays.Contains((int)q.DayOfWeek)).ToList();

            if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.AfterEventOccurrences))
            {
                foreach (var date in selectedDaysDates)
                {
                    dateList.Add(date);
                    dateList.AddRange(ExtensionMethods.GetDateList(date, date.AddYears(100), (int)item.ChurchEventScheduler.EveryCount));
                }
                dateList = dateList.OrderBy(q => q).Take((int)item.ChurchEventScheduler.Occurrences).ToList();
            }
            else if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
            {
                foreach (var date in selectedDaysDates)
                {
                    dateList.Add(date);
                    dateList.AddRange(ExtensionMethods.GetDateList(date, Convert.ToDateTime(item.ChurchEventScheduler.EndDate), (int)item.ChurchEventScheduler.EveryCount));
                }
            }
            else
            {
                SetOrExtendSystemEndDate(item.ChurchEventScheduler, dateRange);
                foreach (var date in selectedDaysDates)
                {
                    dateList.Add(date);
                    dateList.AddRange(ExtensionMethods.GetDateList(date, Convert.ToDateTime(item.ChurchEventScheduler.SystemEndDate), (int)item.ChurchEventScheduler.EveryCount));
                }
            }

            var customWeekEvents = dateList.Select(x => new EventSD
            {
                ChurchId = SessionVariables.CurrentChurch.Id,
                CalendarColor = item.ChurchEventType.CalendarColor,
                //Description = item.ChurchEventType.Description,
                Id = item.ChurchEventTime.Id,
                ChurchEventId = item.ChurchEvent.Id,
                ChurchEventSchedulerId = item.ChurchEventScheduler.Id,
                CreatedBy = item.ChurchEvent.CreatedBy,
                CreatedDate = item.ChurchEvent.CreatedDate,
                StartDate = x,
                EndDate = x.AddDays(_multiDay),
                //StartTime = item.ChurchEventTime.StartTime,
                //EndTime = item.ChurchEventTime.EndTime,
                Title = $"{(!string.IsNullOrEmpty(item.ChurchEventType.Type) ? item.ChurchEventType.Type : item.ChurchEventType.Type)}",
                Type = item.ChurchEventType?.Display,
                TypeId = item.ChurchEventType?.Id,
                CampusId = item.ChurchEventScheduler.CampusId,
                CampusName = item.CampusName,
                AllDay = item.ChurchEventTime.AllDay,
                ShowEventAt = item.ChurchEventTime.ShowEventAt,
                HideEventAt = item.ChurchEventTime.HideEventAt
            }).ToList();

            if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
            {
                customWeekEvents = customWeekEvents.Where(q => Convert.ToDateTime(q.StartDate).Date <= Convert.ToDateTime(item.ChurchEventScheduler.EndDate).Date).ToList();
            }

            return customWeekEvents;
        }

        public List<EventSD> GenerateMonthlyEvents(ChurchEventsViewModel item, int eventCounts, int multiDay)
        {
            var events = new List<EventSD>();

            // Retrieve scheduler and event details from the item parameter
            var scheduler = item.ChurchEventScheduler;
            var churchEvent = item.ChurchEvent;
            var eventTime = item.ChurchEventTime;
            var churchEventType = item.ChurchEventType;

            // Ensure SystemEndDate is set correctly
            if (scheduler.SystemEndDate == null)
            {
                var churchEventScheduler = Work.ChurchEvents.GetScheduler(scheduler.Id);
                scheduler.SystemEndDate = churchEventScheduler.SystemEndDate = DateTime.Now.AddYears(1);
                Work.ChurchEvents.UpdateScheduler(churchEventScheduler);
            }
            else if (scheduler.SystemEndDate.HasValue && scheduler.SystemEndDate.Value.CalculateRemainingInDays() < 30)
            {
                var churchEventScheduler = Work.ChurchEvents.GetScheduler(scheduler.Id);
                scheduler.SystemEndDate = churchEventScheduler.SystemEndDate = scheduler.SystemEndDate.Value.AddYears(1);
                Work.ChurchEvents.UpdateScheduler(churchEventScheduler);
            }

            // Use the updated SystemEndDate or default to now + 1 year
            DateTime endDate = scheduler.SystemEndDate ?? DateTime.Now.AddYears(1);

            // Calculate the number of months between StartDate and SystemEndDate
            var startDate = scheduler.StartDate;
            var zeroTime = new DateTime(1, 1, 1);
            var timeSpan = endDate - startDate;
            var years = (zeroTime + timeSpan).Year - 1;
            var months = 1 + (years * 12);

            if (endDate.Month > startDate.Month && endDate.Day >= startDate.Day)
            {
                months += endDate.Month - startDate.Month;
            }

            var sequence = Enumerable.Range(0, months).ToArray();

            var monthlyChurchEvents = sequence.Select(x => new EventSD
            {
                ChurchId = SessionVariables.CurrentChurch.Id,
                CalendarColor = churchEventType.CalendarColor,
                //Description = item.ChurchEventType.Description,
                Id = eventTime.Id,
                ChurchEventId = churchEvent.Id,
                ChurchEventSchedulerId = scheduler.Id,
                CreatedBy = churchEvent.CreatedBy,
                CreatedDate = churchEvent.CreatedDate,
                StartDate = startDate.AddMonths(x),
                EndDate = startDate.AddMonths(x).AddDays(multiDay),
                //StartTime = eventTime.StartTime,
                //EndTime = eventTime.EndTime,
                Title = $"{(!string.IsNullOrEmpty(churchEventType.Type) ? churchEventType.Type : churchEventType.Type)}",
                Type = item.ChurchEventType?.Display,
                TypeId = eventTime.Id,
                CampusId = scheduler.CampusId,
                CampusName = item.CampusName,
                AllDay = eventTime.AllDay,
                ShowEventAt = eventTime.ShowEventAt,
                HideEventAt = eventTime.HideEventAt
            }).ToList();

            return monthlyChurchEvents;
        }

        private List<EventSD> GenerateYearlyEvents(ChurchEventsViewModel item, DateRange dateRange)
        {
            var eventCounts = 1000;
            var sequence = Enumerable.Range(0, eventCounts).ToArray();

            if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.AfterEventOccurrences))
            {
                eventCounts = (int)item.ChurchEventScheduler.Occurrences;
                sequence = Enumerable.Range(0, eventCounts).ToArray();
            }
            else if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
            {
                var years = (Convert.ToDateTime(item.ChurchEventScheduler.EndDate) - item.ChurchEventScheduler.StartDate).Days / 365;
                sequence = Enumerable.Range(0, years).ToArray();
            }
            else
            {
                SetOrExtendSystemEndDate(item.ChurchEventScheduler, dateRange);
                var years = (Convert.ToDateTime(item.ChurchEventScheduler.SystemEndDate) - item.ChurchEventScheduler.StartDate).Days / 365;
                sequence = Enumerable.Range(0, years).ToArray();
            }

            var yearlyChurchEvents = sequence.Select(x => new EventSD
            {
                ChurchId = SessionVariables.CurrentChurch.Id,
                CalendarColor = item.ChurchEventType.CalendarColor,
                //Description = item.ChurchEventType.Description,
                Id = item.ChurchEventTime.Id,
                ChurchEventId = item.ChurchEvent.Id,
                ChurchEventSchedulerId = item.ChurchEventScheduler.Id,
                CreatedBy = item.ChurchEvent.CreatedBy,
                CreatedDate = item.ChurchEvent.CreatedDate,
                StartDate = item.ChurchEventScheduler.StartDate.AddYears(x),
                EndDate = item.ChurchEventScheduler.StartDate.AddYears(x).AddDays(_multiDay),
                //StartTime = item.ChurchEventTime.StartTime,
                //EndTime = item.ChurchEventTime.EndTime,
                Title = $"{(!string.IsNullOrEmpty(item.ChurchEventType.Type) ? item.ChurchEventType.Type : item.ChurchEventType.Type)}",
                Type = item.ChurchEventType?.Display,
                TypeId = item.ChurchEventType?.Id,
                CampusId = item.ChurchEventScheduler.CampusId,
                AllDay = item.ChurchEventTime.AllDay,
                ShowEventAt = item.ChurchEventTime.ShowEventAt,
                HideEventAt = item.ChurchEventTime.HideEventAt
            }).ToList();

            return yearlyChurchEvents;
        }

        private List<EventSD> GenerateWeekdayEvents(ChurchEventsViewModel item, DateRange dateRange)
        {
            var dateList = new List<DateTime>();

            if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.AfterEventOccurrences))
            {
                dateList.AddRange(ExtensionMethods.GetWeekDaysList(item.ChurchEventScheduler.StartDate, item.ChurchEventScheduler.StartDate.AddYears(100)));
                dateList = dateList.OrderBy(q => q).Take((int)item.ChurchEventScheduler.Occurrences).ToList();
            }
            else if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
            {
                dateList.AddRange(ExtensionMethods.GetWeekDaysList(item.ChurchEventScheduler.StartDate, (DateTime)item.ChurchEventScheduler.EndDate));
            }
            else
            {
                SetOrExtendSystemEndDate(item.ChurchEventScheduler, dateRange);
                dateList.AddRange(ExtensionMethods.GetWeekDaysList(item.ChurchEventScheduler.StartDate, (DateTime)item.ChurchEventScheduler.SystemEndDate));
            }

            var weekDaysEvents = dateList.Select(x => new EventSD
            {
                ChurchId = SessionVariables.CurrentChurch.Id,
                CalendarColor = item.ChurchEventType.CalendarColor,
                //Description = item.ChurchEventType.Description,
                Id = item.ChurchEventTime.Id,
                ChurchEventId = item.ChurchEvent.Id,
                ChurchEventSchedulerId = item.ChurchEventScheduler.Id,
                CreatedBy = item.ChurchEvent.CreatedBy,
                CreatedDate = item.ChurchEvent.CreatedDate,
                StartDate = x,
                EndDate = x.AddDays(_multiDay),
                //StartTime = item.ChurchEventTime.StartTime,
                //EndTime = item.ChurchEventTime.EndTime,
                Title = $"{(!string.IsNullOrEmpty(item.ChurchEventType.Type) ? item.ChurchEventType.Type : item.ChurchEventType.Type)}",
                Type = item.ChurchEventType?.Display,
                TypeId = item.ChurchEventType?.Id,
                CampusId = item.ChurchEventScheduler.CampusId,
                CampusName = item.CampusName,
                AllDay = item.ChurchEventTime.AllDay,
                ShowEventAt = item.ChurchEventTime.ShowEventAt,
                HideEventAt = item.ChurchEventTime.HideEventAt
            }).ToList();

            if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
            {
                weekDaysEvents = weekDaysEvents.Where(q => Convert.ToDateTime(q.StartDate).Date <= Convert.ToDateTime(item.ChurchEventScheduler.EndDate).Date).ToList();
            }

            return weekDaysEvents;
        }

        private List<EventSD> GenerateCustomEvents(ChurchEventsViewModel item, DateRange dateRange)
        {
            var everyCount = item.ChurchEventScheduler.EveryCount;
            var eventCounts = 0;
            var sequence = Enumerable.Range(0, eventCounts).ToArray();

            if (item.ChurchEventScheduler.EveryType.Contains(EventRepeatFrequency.Day))
            {
                if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.AfterEventOccurrences))
                {
                    eventCounts = (int)item.ChurchEventScheduler.Occurrences;
                    sequence = Enumerable.Range(0, eventCounts).ToArray();
                }
                else if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
                {
                    eventCounts = (int)(Convert.ToDateTime(item.ChurchEventScheduler.EndDate) - item.ChurchEventScheduler.StartDate).TotalDays + 1;
                    sequence = Enumerable.Range(0, eventCounts).ToArray();
                }
                else
                {
                    SetOrExtendSystemEndDate(item.ChurchEventScheduler, dateRange);
                    eventCounts = (int)(Convert.ToDateTime(item.ChurchEventScheduler.SystemEndDate) - item.ChurchEventScheduler.StartDate).TotalDays + 1;
                    sequence = Enumerable.Range(0, eventCounts).ToArray();
                }

                var customDayEvents = sequence.Select(x => new EventSD
                {
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CalendarColor = item.ChurchEventType.CalendarColor,
                    //Description = item.ChurchEventType.Description,
                    Id = item.ChurchEventTime.Id,
                    ChurchEventId = item.ChurchEvent.Id,
                    ChurchEventSchedulerId = item.ChurchEventScheduler.Id,
                    CreatedBy = item.ChurchEvent.CreatedBy,
                    CreatedDate = item.ChurchEvent.CreatedDate,
                    StartDate = item.ChurchEventScheduler.StartDate.AddDays(x * (int)everyCount),
                    EndDate = item.ChurchEventScheduler.StartDate.AddDays(x * (int)everyCount).AddDays(_multiDay),
                    //StartTime = item.ChurchEventTime.StartTime,
                    //EndTime = item.ChurchEventTime.EndTime,
                    Title = $"{(!string.IsNullOrEmpty(item.ChurchEventType.Type) ? item.ChurchEventType.Type : item.ChurchEventType.Type)}",
                    Type = item.ChurchEventType?.Display,
                    TypeId = item.ChurchEventType?.Id,
                    CampusId = item.ChurchEventScheduler.CampusId,
                    CampusName = item.CampusName,
                    AllDay = item.ChurchEventTime.AllDay,
                    ShowEventAt = item.ChurchEventTime.ShowEventAt,
                    HideEventAt = item.ChurchEventTime.HideEventAt
                }).ToList();

                if (item.ChurchEventScheduler.EventEnds.Equals(EventEnds.OnSpecificDate))
                {
                    customDayEvents = customDayEvents.Where(q => Convert.ToDateTime(q.StartDate).Date <= Convert.ToDateTime(item.ChurchEventScheduler.EndDate).Date).ToList();
                }

                return customDayEvents;
            }

            return new List<EventSD>();
        }

        private EventSD GenerateNonRepeatingEvent(ChurchEventsViewModel item, int multiDay)
        {
            return new EventSD
            {
                ChurchId = SessionVariables.CurrentChurch.Id,
                CalendarColor = item.ChurchEventType.CalendarColor,
                //Description = item.ChurchEventType.Description,
                Id = item.ChurchEventTime.Id,
                ChurchEventId = item.ChurchEvent.Id,
                ChurchEventSchedulerId = item.ChurchEventScheduler.Id,
                CreatedBy = item.ChurchEvent.CreatedBy,
                CreatedDate = item.ChurchEvent.CreatedDate,
                StartDate = item.ChurchEventScheduler.StartDate,
                EndDate = item.ChurchEventScheduler.StartDate.AddDays(multiDay),
                //StartTime = item.ChurchEventTime.StartTime,
                //EndTime = item.ChurchEventTime.EndTime,
                Title = $"{(!string.IsNullOrEmpty(item.ChurchEventType.Type) ? item.ChurchEventType.Type : item.ChurchEventType.Type)}",
                Type = item.ChurchEventType?.Display,
                TypeId = item.ChurchEventType?.Id,
                CampusId = item.ChurchEventScheduler.CampusId,
                CampusName = item.CampusName,
                AllDay = item.ChurchEventTime.AllDay,
                ShowEventAt = item.ChurchEventTime.ShowEventAt,
                HideEventAt = item.ChurchEventTime.HideEventAt
            };
        }

        public void SetOrExtendSystemEndDate(ChurchEventScheduler scheduler, DateRange dateRange = null)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            // If SystemEndDate is null, set it to one year from now.
            if (scheduler.SystemEndDate == null)
            {
                scheduler.SystemEndDate = DateTime.Now.AddYears(1);
            }
            else
            {
                // If SystemEndDate is less than 30 days away, extend it by one year.
                if (((DateTime)scheduler.SystemEndDate).CalculateRemainingInDays() < 30)
                {
                    scheduler.SystemEndDate = ((DateTime)scheduler.SystemEndDate).AddYears(1);
                }
            }

            // Optionally, adjust SystemEndDate based on the provided date range if applicable.
            if (dateRange != null)
            {
                var endDate = dateRange.EndDate.Date;
                if (endDate.CalculateRemainingInDays() > ((DateTime)scheduler.SystemEndDate).CalculateRemainingInDays())
                {
                    scheduler.SystemEndDate = endDate;
                }
            }

            // Save the changes (assuming Work is accessible here, or pass it as a parameter).
            Work.ChurchEvents.UpdateScheduler(scheduler);
        }

        public List<EventSD> GetEvents(string churchId, string dateRange = null, string campusId = null, string eventTypeId = null)
        {
            var dates = dateRange.ToDateRange();

            //var churchEvents = GetChurchEventByFrequency(churchId, dates, campusId, eventTypeId);
            var churchEvents = Work.Calendar.GetChurchEventByFrequency(SessionVariables.CurrentChurch.Id, dateRange, campusId, eventTypeId);

            //churchEvents.Select(x => { x.UniqueId = $"{x.Id}_{x.StartDate:MM-dd-yyyy}"; return x; }).ToList();
            return new List<EventSD>();
            //if (dateRange.IsNotNullOrEmpty())
            //{
            //    return churchEvents.Where(q => q.StartDate.Date >= dates.StartDate.Date && q.StartDate.Date <= dates.EndDate.Date).OrderBy(o => o.StartDate).ThenBy(x => Convert.ToDateTime(x.StartTime)).ThenBy(x => x.Title).ToList();
            //}
            //else
            //{
            //    return churchEvents.Where(q => (q.StartDate.Date > DateTime.Now.AddMonths(-2).Date || (q.StartDate.Date == DateTime.Now.AddMonths(-2).Date && !q.AllDay && DateTime.Parse(q.EndTime).TimeOfDay > DateTime.Now.TimeOfDay) || (q.StartDate.Date == DateTime.Now.Date && q.AllDay)) && q.StartDate.Date <= DateTime.Now.AddMonths(6).Date).OrderBy(o => o.StartDate).ThenBy(x => Convert.ToDateTime(x.StartTime)).ThenBy(x => x.Title).ToList();
            //}
        }

        public List<EventSD> GetEventOverView(string churchId, string id, string day)
        {
            var dateRange = $"{DateTime.Now:MM/dd/yyyy}-{DateTime.Now:MM/dd/yyyy}";

            if (day.EqualsIgnoreCase("today"))
            {
                dateRange = $"{DateTime.Now:MM/dd/yyyy}-{DateTime.Now:MM/dd/yyyy}";
            }
            else if (day.EqualsIgnoreCase("tomorrow"))
            {
                dateRange = $"{DateTime.Now.AddDays(1):MM/dd/yyyy}-{DateTime.Now.AddDays(1):MM/dd/yyyy}";
            }
            else
            {
                var date = DateTime.Parse(day);
                dateRange = $"{date:MM/dd/yyyy}-{date:MM/dd/yyyy}";
            }

            var events = GetEvents(churchId, dateRange);
            return events.Where(x => x.ChurchEventSchedulerId.Equals(id)).ToList();
        }

        public string Create(EventSD model)
        {
            var eventType = string.Empty;

            var eventModel = Db.Event.FirstOrDefault(x => x.Id == model.Id);

            if (!string.IsNullOrEmpty(model.Id) && eventModel != null)
            {
                eventType = "Update Event";
                model.Number = eventModel.Number;

                if (string.IsNullOrEmpty(Convert.ToString(model.EndDate)))
                {
                    model.EndDate = model.StartDate;
                }

                eventModel = model;
                eventModel.ModifiedDate = DateTime.Now;
                eventModel.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                Db.Event.AddOrUpdate(model);
            }
            else
            {
                eventType = "Create Event";

                if (string.IsNullOrEmpty(Convert.ToString(model.EndDate)))
                {
                    model.EndDate = model.StartDate;
                }

                model.CreatedDate = DateTime.Now;
                model.CreatedBy = SessionVariables.CurrentUser.User.Id;
                Db.Event.Add(model);
            }

            Db.SaveChanges();

            return eventType;
        }

        public void Update(EventSD model)
        {
            var eventModel = Work.Event.Get(model.Id);

            if (!string.IsNullOrEmpty(model.Id) && eventModel != null)
            {
                model.Number = eventModel.Number;
                model.CalendarColor = eventModel.CalendarColor;

                if (string.IsNullOrEmpty(Convert.ToString(model.EndDate)))
                {
                    model.EndDate = model.StartDate;
                }

                eventModel = model;
                eventModel.ModifiedDate = DateTime.Now;
                eventModel.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                Db.Event.AddOrUpdate(model);
                Db.SaveChanges();
            }
        }

        public EventSD Delete(string id)
        {
            var evnt = Work.Event.Get(id);
            Delete(evnt);
            SaveChanges();
            return evnt;
        }
    }
}