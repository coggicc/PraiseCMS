using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "9246791019b87d5c6afa054420a092")]
    public class CalendarController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewData["churchEventTypes"] = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, true);
            ViewData["upcomingEvents"] = work.Event.GetUpComingEvents(SessionVariables.CurrentChurch.Id);
            ViewBag.ReturnUrl = "/calendar";

            return View();
        }

        [HttpGet]
        public JsonResult GetEvents(string campusId, string eventTypeId, string isoDateRange)
        {
            var dateRange = isoDateRange.ConvertISODateRangeToCustomFormat();

            // Retrieve events based on frequency
            var events = work.Calendar.GetChurchEventByFrequency(SessionVariables.CurrentChurch.Id, dateRange, campusId, eventTypeId);

            // Define the format for displaying time
            const string timeFormat = "h:mm tt"; // Example: "8:00 AM"

            // Format events for the response
            var formattedEvents = events.Select(evt =>
            {
                DateTime startTime, endTime;
                DateTime today = DateTime.Today; // Use today's date as a base for conversion

                // Convert StartTime from TimeSpan to DateTime using today's date
                if (evt.StartTime.HasValue)
                {
                    startTime = today.Add(evt.StartTime.Value);
                }
                else
                {
                    startTime = default;
                }

                // Convert EndTime from TimeSpan to DateTime using today's date
                if (evt.EndTime.HasValue)
                {
                    endTime = today.Add(evt.EndTime.Value);
                }
                else
                {
                    endTime = default;
                }

                // Combine Date and Time for StartDate and EndDate
                DateTime? startDateTime = evt.StartDate.HasValue
                    ? evt.StartDate.Value.Date.Add(new TimeSpan(startTime.Hour, startTime.Minute, startTime.Second))
                    : (DateTime?)null;

                DateTime? endDateTime = evt.EndDate.HasValue
                    ? evt.EndDate.Value.Date.Add(new TimeSpan(endTime.Hour, endTime.Minute, endTime.Second))
                    : (DateTime?)null;

                return new
                {
                    evt.Id,
                    evt.ChurchId,
                    evt.CalendarColor,
                    evt.Description,
                    StartDate = startDateTime?.ToIso8601DateTimeString(),
                    EndDate = endDateTime?.ToIso8601DateTimeString(),
                    StartTime = evt.StartTime.HasValue ? startTime.ToString(timeFormat) : null,
                    EndTime = evt.EndTime.HasValue ? endTime.ToString(timeFormat) : null,
                    evt.Title,
                    evt.Type,
                    evt.TypeId,
                    evt.CampusId,
                    evt.CampusName,
                    evt.AllDay,
                    evt.ShowEventAt,
                    evt.HideEventAt,
                    evt.Complete,
                    EventTimeId = evt.EventTimeId // Include ChurchEventTime.Id
                };
            }).ToList();

            return Json(formattedEvents, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateEventOnDragAndDrop(EventViewModel viewModel)
        {
            try
            {
                viewModel.CreatedBy = SessionVariables.CurrentUser.User.Id;
                viewModel.ChurchId = SessionVariables.CurrentChurch.Id;
                viewModel.CampusId = string.Join(",", SessionVariables.Campuses.Select(x => x.Id));
                work.Calendar.CreateEventOnDragAndDrop(viewModel);
                return Json(new ResponseModel { Success = true, Message = Constants.SavedMessage }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new ResponseModel { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        //Used with eventResize function - Updating event time
        [HttpPost]
        public JsonResult UpdateEvent(EventSD model)
        {
            try
            {
                var existingEvent = work.ChurchEvent.Get(model.Id);
                //if (existingEvent != null)
                //{
                //    // Update the existing event's properties with the new values from the model
                //    existingEvent.Title = model.Title;
                //    existingEvent.StartDate = DateTime.Parse(model.StartDate);
                //    existingEvent.EndDate = model.EndDate != null ? DateTime.Parse(model.EndDate) : (DateTime?)null;
                //    existingEvent.Description = model.Description;
                //    existingEvent.CalendarColor = model.CalendarColor;
                //    existingEvent.Type = model.Type;
                //    existingEvent.CampusId = model.CampusId;
                //    existingEvent.AllDay = model.AllDay;
                //    existingEvent.Complete = model.Complete;
                //    // Update other relevant fields as necessary
                //}
                return Json(new ResponseModel { Success = true, Message = Constants.SavedMessage }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new ResponseModel { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult _CreateEvent(string type, string typeId, string startDate, string startTime)
        {
            var standardEventTypes = work.ChurchEventType.GetAllViewModel(SessionVariables.CurrentChurch.Id, null, false).Select(et => new SelectListItem
            {
                Value = et.Id,
                Text = et.Type
            }).ToList();

            DateTime startDateParsed;

            // Attempt to parse the startDate passed from the client
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                startDateParsed = parsedDate; // Successfully parsed
            }
            else
            {
                startDateParsed = DateTime.Now.Date; // Default to today's date if parsing fails
            }

            // If start time is provided (only in day/week views), use it, otherwise round to the next 30-minute interval
            DateTime startDateTime;
            if (!string.IsNullOrEmpty(startTime) && DateTime.TryParseExact(startTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
            {
                startDateTime = startDateParsed.Add(parsedTime.TimeOfDay);
            }
            else
            {
                // Get the rounded current time to the next 30-minute interval
                startDateTime = ExtensionMethods.GetRoundedTime(DateTime.Now);
            }

            // Ensure that Constants.ButtonColors is not null and has a matching color
            string calendarColor = Constants.ButtonColors?.FirstOrDefault(x => x.Key.Contains("primary")).Key ?? "primary"; // Fallback to default color if no match is found

            var model = new EventViewModel
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                Title = type ?? string.Empty, // Default to empty string if null
                Type = type ?? string.Empty,
                TypeId = typeId ?? string.Empty,
                StartDate = startDateParsed.ToString("MM/dd/yyyy"),
                EndDate = startDateParsed.ToString("MM/dd/yyyy"),
                StartTime = startDateTime.ToString("hh:mm tt"), // Pass the time or rounded time
                EndTime = startDateTime.AddHours(1).ToString("hh:mm tt"), // Set the end time 1 hour later
                CalendarColor = calendarColor,
                Frequency = ChurchEventFrequency.DoesNotRepeat,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                IsStandard = false,
                StandardEventTypes = standardEventTypes
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _CreateEvent(EventViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.AllDay)
                {
                    viewModel.StartTime = null;
                    viewModel.EndTime = null;
                }

                work.Calendar.CreateEvent(viewModel);

                return AjaxRedirectTo("/calendar");
            }

            return PartialView("_CreateEdit", viewModel);
        }

        [HttpGet]
        public ActionResult _EditEvent(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var existingEvent = work.ChurchEvent.Get(id);
            // Retrieve schedulers and times for the event
            var schedulers = work.ChurchEvents.GetAllScheduler(id, true).ToList();

            // Map the data to the EventViewModel
            var viewModel = new EventViewModel
            {
                Id = existingEvent.Id,
                ChurchId = existingEvent.ChurchId,
                Type = existingEvent.ChurchEventTypeId,
                Title = existingEvent.DisplayName,
                //Description = existingEvent.Description,
                StartDate = schedulers.FirstOrDefault()?.StartDate.ToString("MM/dd/yyyy"),
                EndDate = schedulers.FirstOrDefault()?.EndDate?.ToString("MM/dd/yyyy"),
                //StartTime = schedulers.FirstOrDefault().Times.FirstOrDefault()?.StartTime,
                //EndTime = times.FirstOrDefault()?.EndTime,
                //AllDay = times.FirstOrDefault()?.AllDay ?? false,
                //CalendarColor = existingEvent.CalendarColor,
                Campuses = schedulers.Select(s => s.CampusId).ToList() // Multiple campuses
            };

            // Return the partial view with the populated view model
            return PartialView("_CreateEdit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditEvent(EventSD model)
        {
            if (ModelState.IsValid)
            {
                if (model.AllDay)
                {
                    model.StartTime = null;
                    model.EndTime = null;
                }

                work.Event.Update(model);

                return AjaxRedirectTo("/calendar");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult DeleteEvent(string id)
        {
            work.Event.Delete(id);
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult _View(string id, DateTime? date, string eventTimeId = null, bool isGlobal = false)
        {
            ViewBag.IsChurchEvent = false;
            ViewBag.IsGlobal = isGlobal;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(eventTimeId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Query to get the single event based on the provided id and eventTimeId in ChurchEventTime
            var record = (from CETM in db.ChurchEventTime
                          join CE in db.ChurchEvents
                              on CETM.ChurchEventId equals CE.Id
                          join CET in db.ChurchEventTypes
                              on CE.ChurchEventTypeId equals CET.Id into cetJoin // Left join on ChurchEventTypes
                          from CET in cetJoin.DefaultIfEmpty()
                          join U in db.Users
                              on CE.CreatedBy equals U.Id
                          where CE.Id == id && CETM.Id == eventTimeId
                          select new
                          {
                              EventId = CE.Id,
                              CE.ChurchId,
                              CalendarColor = CET != null ? CET.CalendarColor : "#FFFFFF", // Default color if no ChurchEventType
                              Description = CE.Description,
                              CE.StartDate,
                              CE.EndDate,
                              EventTimeId = CETM.Id,
                              CETM.StartTime,
                              CETM.EndTime,
                              CustomEventName = CE.CustomEventName,
                              Type = CET != null ? CET.Type : "Custom Event",
                              CE.ChurchEventTypeId,
                              CETM.CampusId,
                              CETM.AllDay,
                              CETM.ShowEventAt,
                              CETM.HideEventAt,
                              U.FirstName,
                              U.LastName
                          }).FirstOrDefault();

            if (record != null)
            {
                // Concatenate FirstName and LastName for FullName
                var fullName = string.Join(" ", new[] { record.FirstName?.Trim(), record.LastName?.Trim() }.Where(s => !string.IsNullOrWhiteSpace(s)));

                // Retrieve the campus name if CampusId is not null
                var campusName = record.CampusId != null
                    ? db.Campuses.FirstOrDefault(c => c.Id == record.CampusId)?.Display
                    : "Campus Not Specified";

                // Use a ChurchEventViewModel for consistency with GetEvents structure
                var model = new ChurchEventViewModel
                {
                    Id = record.EventId,
                    ChurchId = record.ChurchId,
                    CalendarColor = record.CalendarColor,
                    Description = record.Description,
                    StartDate = date ?? record.StartDate,
                    EndDate = record.EndDate,
                    EventTimeId = record.EventTimeId, // Assign EventTimeId
                    StartTime = record.AllDay ? (TimeSpan?)null : record.StartTime, // Set StartTime to null if AllDay is true
                    EndTime = record.AllDay ? (TimeSpan?)null : record.EndTime,     // Set EndTime to null if AllDay is true
                    Title = record.CustomEventName ?? record.Type,
                    Type = record.Type,
                    TypeId = record.ChurchEventTypeId,
                    CampusId = record.CampusId,
                    CampusName = campusName,
                    AllDay = record.AllDay,
                    ShowEventAt = record.ShowEventAt,
                    HideEventAt = record.HideEventAt,
                    Complete = !record.AllDay && record.EndTime < DateTime.Now.TimeOfDay // Mark complete if end time has passed and event is not AllDay
                };

                ViewBag.isOwner = SessionVariables.CurrentUser.User.Id == record.EventId;
                ViewBag.OwnerName = fullName;
                ViewBag.IsChurchEvent = true;

                return PartialView("_View", model);
            }
            else
            {
                return PartialView("_View", new ChurchEventViewModel());
            }
        }

        public JsonResult GetEventTypeDetails(string eventTypeId)
        {
            if (string.IsNullOrEmpty(eventTypeId))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var eventType = work.ChurchEventType.Get(eventTypeId);

            if (eventType == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            // Return an object containing the description and calendar color
            var eventTypeDetails = new
            {
                eventType.CalendarColor
            };

            return Json(eventTypeDetails, JsonRequestBehavior.AllowGet);
        }
    }
}