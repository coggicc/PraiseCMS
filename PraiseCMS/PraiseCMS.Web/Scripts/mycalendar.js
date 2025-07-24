"use strict";
// Define the calendar variable in the global scope
var calendar;
var filterApplied = false;
let views = "timeGridWeek,timeGridDay,listWeek";
const colorMap = {
    danger: '#dc3545',
    primary: '#007bff',
    success: '#28a745',
    // Add other mappings as needed
};

var KTCalendarExternalEvents = function () {

    var initExternalEvents = function () {
        $('#kt_calendar_external_events .fc-draggable-handle').each(function () {
            // store data so the calendar knows to render an event upon drop
            // Only attach the necessary data for FullCalendar to handle the drop event
            $(this).data('event', {
                title: $.trim($(this).text()), // Use the element's text as the event title
                stick: true, // Maintain the event when the user navigates
                classNames: [$(this).data('color')],
                // Removed the description field to prevent adding placeholder text
            });
        });
    }

    var initCalendar = function () {
        var todayDate = moment().startOf('day');
        var YM = todayDate.format('YYYY-MM');
        var YESTERDAY = todayDate.clone().subtract(1, 'day').format('YYYY-MM-DD');
        var TODAY = todayDate.format('YYYY-MM-DD');
        var TOMORROW = todayDate.clone().add(1, 'day').format('YYYY-MM-DD');

        // Get the current time in the format required by FullCalendar
        var currentTime = moment().format('YYYY-MM-DDTHH:mm:ss');

        var calendarEl = document.getElementById('kt_calendar');
        var containerEl = document.getElementById('kt_calendar_external_events');

        var Draggable = FullCalendarInteraction.Draggable;

        new Draggable(containerEl, {
            itemSelector: '.fc-draggable-handle',
            eventData: function (eventEl) {
                return $(eventEl).data('event');
            }
        });

        calendar = new FullCalendar.Calendar(calendarEl, {
            plugins: ['interaction', 'dayGrid', 'timeGrid', 'list'],
            isRTL: KTUtil.isRTL(),
            customButtons: {
                myCustomButton: {
                    text: '&nbsp;', // We will dynamically add the icon here
                    iconClass: 'fa-solid fa-maximize', // Initial icon class
                    click: function () {
                        // Toggle sidebar visibility
                        $('#calendar-sidebar').toggle();

                        // Check if the sidebar is visible or hidden and adjust the main content width
                        if ($('#calendar-sidebar').is(':visible')) {
                            $('#calendar-main-content').removeClass('col-lg-12').addClass('col-lg-9');
                            // Update the icon to the maximize state
                            $('.fc-myCustomButton-button i').removeClass('fa-compress-alt').addClass('fa-expand-alt');
                        } else {
                            $('#calendar-main-content').removeClass('col-lg-9').addClass('col-lg-12');
                            // Update the icon to the minimize state
                            $('.fc-myCustomButton-button i').removeClass('fa-expand-alt').addClass('fa-compress-alt');
                        }
                    }
                }
            },
            //tooltip for expand calendar button
            eventPositioned: function () {
                // Ensure the custom button is properly initialized with the tooltip after the calendar has rendered
                $('.fc-myCustomButton-button').attr('title', 'Expand/Collapse');
                $('.fc-myCustomButton-button').tooltip({
                    placement: 'top',
                    trigger: 'hover',
                    container: 'body'
                });
            },
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek myCustomButton'
            },
            height: 800,
            contentHeight: 780,
            aspectRatio: 3,  // see: https://fullcalendar.io/docs/aspectRatio
            nowIndicator: true,
            now: currentTime,
            views: {
                dayGridMonth: {
                    buttonText: 'month',
                    eventLimit: 3
                },
                timeGridWeek: { buttonText: 'week' },
                timeGridDay: { buttonText: 'day' },
                listWeek: { buttonText: 'list' }
            },
            defaultView: 'dayGridMonth',
            defaultDate: TODAY,
            droppable: true,
            editable: true,
            eventLimit: true, // allow "more" link when too many events
            navLinks: true,           
            // Fetch events via AJAX
            events: function (fetchInfo, successCallback, failureCallback) {
                let campusId = $('#CampusFilter').val();
                let eventTypeId = $('#ChurchEventTypeFilter').val();
                let dateRange = fetchInfo.startStr + ',' + fetchInfo.endStr;

                $.ajax({
                    url: "/calendar/getEvents",
                    type: "GET",
                    data: {
                        campusId: campusId,
                        eventTypeId: eventTypeId,
                        isoDateRange: dateRange
                    },
                    success: function (response) {
                        let events = response.map(function (evt) {
                            return {
                                id: evt.Id,
                                title: evt.Title || evt.Type,
                                start: evt.StartDate,
                                end: evt.EndDate,
                                allDay: evt.AllDay,
                                className: "customEvent",
                                editable: evt.Type !== "ChurchEvent" && evt.CreatedBy === $('#login-user-id').val(),
                                campusName: evt.CampusName,
                                extendedProps: {
                                    type: evt.Type,
                                    typeId: evt.TypeId,
                                    campusId: evt.CampusId,
                                    churchId: evt.ChurchId,
                                    eventTimeId: evt.EventTimeId, // Add eventTimeId to props
                                    startTime: evt.StartTime, // Display times in tooltips or side panel
                                    endTime: evt.EndTime,
                                    description: evt.Description || '&nbsp;',
                                    showEventAt: evt.ShowEventAt,
                                    hideEventAt: evt.HideEventAt,
                                    complete: evt.Complete,
                                    CalendarColor: evt.CalendarColor,
                                    description: evt.Description || '&nbsp;',
                                    tooltipContent: generateEventTooltip(evt)
                                }
                            };
                        });

                        successCallback(events);
                    },
                    error: function () {
                        failureCallback();
                    }
                });
            },
            //Purpose: The drop function is triggered when an external draggable element is dropped onto the calendar.
            drop: function (arg) {
                if (arg.draggedEl) {
                    // Extract data from the dragged event element
                    const tempEventData = {
                        type: $(arg.draggedEl).data('type'),
                        typeId: $(arg.draggedEl).data('id'),
                        droppedDate: arg.dateStr,
                        droppedTime: arg.date,
                        title: $(arg.draggedEl).text().trim(),
                        description: $(arg.draggedEl).data('description') || ''
                    };

                    // Get the current view type (month, week, day views, etc.)
                    const currentView = arg.view.type;

                    // Check if it's a day or timeGrid view (where time matters)
                    const isDayView = (currentView === 'timeGridDay' || currentView === 'timeGridWeek');

                    // Format the dropped date for the controller
                    const formattedDroppedDate = formatDateToMMDDYYYY(tempEventData.droppedDate);

                    // Only pass the time if in day or timeGrid view
                    const formattedDroppedTime = isDayView ? tempEventData.droppedTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : null;

                    // Use AJAX to call the _CreateEvent action and load the form into the modal
                    $.ajax({
                        url: '/calendar/_CreateEvent',
                        type: 'GET',
                        data: {
                            type: tempEventData.type,
                            typeId: tempEventData.typeId,
                            startDate: formattedDroppedDate, // Pass the formatted date to the action
                            startTime: formattedDroppedTime // Pass the time if available, otherwise null
                        },
                        success: function (response) {
                            // Load the response (form content) into the ajax-modal's form container
                            $('#ajax-modal .modal-body').html(response);
                            $('#ajax-modal-size').addClass('modal-dialog'); // Ensure correct modal class is applied
                            $('#ajax-modal .modal-title').text('Create Event');

                            $('#Title').val(tempEventData.title);
                            $('#Description').val(tempEventData.description);

                            // Show the modal
                            $('#ajax-modal').modal({
                                backdrop: 'static',
                                keyboard: false
                            }).modal('show');
                        },
                        error: function (xhr, status, error) {
                            $('#ajax-modal .modal-body').html('<p class="text-danger">Error loading form. Please try again.</p>');
                        }
                    });
                }
            },
            // Optional: Prevent default event receive action that occurs after drop
            eventReceive: function (info) {
                // This prevents the temporary placeholder event from being added to the calendar
                info.event.remove();
            },
            eventClick: function (info) {
                // Construct formatted date for URL
                const date = new Date(info.event.start);
                const formattedDate = [
                    date.getFullYear(),
                    ("0" + (date.getMonth() + 1)).slice(-2),
                    ("0" + date.getDate()).slice(-2)
                ].join('-');

                // Get eventTimeId and construct URL
                const eventTimeId = info.event.extendedProps.eventTimeId;
                const baseUrl = $('#viewModal').attr('href').split('?')[0];
                const url = `${baseUrl}?id=${info.event.id}&date=${formattedDate}&eventTimeId=${eventTimeId}`;

                // Update modal link once and open modal
                $('#viewModal').attr('href', url);

                // Open the modal without triggering additional clicks
                $('#viewModal').off('click').click();

                // Reset href after a delay to prevent reusing old parameters
                setTimeout(() => $('#viewModal').attr('href', baseUrl), 500);

                // Optional: Update URL for other components if necessary
                updateUrl(info.event.id);
            },

            //Purpose: The eventDrop function is triggered when an existing event is dragged and dropped to a new position in the calendar.
            //eventDrop: function (info) {
            //},
            eventRender: function (info) {
                var element = $(info.el);
                var calendarColorClass = `fc-event-${info.event.extendedProps.CalendarColor}`;
                var backgroundClass;
                var now = new Date();
                var eventEnd = info.event.end ? info.event.end : info.event.start;

                // Determine the background class
                if (eventEnd < now) {
                    backgroundClass = 'bg-light-o-50';
                } else {
                    backgroundClass = `bg-${info.event.extendedProps.CalendarColor}-o-50`;
                }

                element.find('.fc-content').removeClass().addClass(`fc-content ${backgroundClass}`);
                element.find('.fc-content').addClass(calendarColorClass);

                if (element.hasClass('fc-list-item')) {
                    element.addClass(calendarColorClass);
                }

                // Display description under the title for Day, Week, and List views
                if (element.hasClass('fc-time-grid-event') || element.hasClass('fc-list-item')) {
                    const eventTitle = info.event.title;
                    const campusName = info.event.extendedProps.campusName || ''; // Retrieve campus name directly from evt.CampusName
                    const eventDescription = info.event.extendedProps.description || ''; // Ensure description is not null or undefined

                    // Format the title with campus name if available, only for Day and List views
                    const formattedTitle = campusName ? `${eventTitle} (${campusName})` : eventTitle;

                    // Append the formatted title and description to the appropriate elements
                    element.find('.fc-title, .fc-list-item-title').html(
                        `${formattedTitle}<div class="fc-description text-dark-50">${eventDescription}</div>`
                    );
                }

                // Initialize the tooltip with the renamed property
                initializeTooltip(element, info.event.extendedProps.tooltipContent);
            },

            //Purpose: The eventResize function is triggered when an event's duration is changed by dragging the start or end handles.
            eventResize: function (info) {
                // Build the data object to match the structure expected by the updateEvent() controller
                const updatedEvent = {
                    id: info.event.id,
                    churchId: info.event.extendedProps.churchId,
                    calendarColor: info.event.extendedProps.CalendarColor,
                    description: info.event.extendedProps.description || '',
                    startDate: info.event.start.toISOString(), // Convert start date to ISO string format
                    endDate: info.event.end ? info.event.end.toISOString() : null, // Handle null end dates
                    startTime: info.event.extendedProps.startTime,
                    endTime: info.event.extendedProps.endTime,
                    title: info.event.title,
                    type: info.event.extendedProps.type,
                    typeId: info.event.extendedProps.typeId,
                    campusId: info.event.extendedProps.campusId,
                    allDay: info.event.allDay,
                    showEventAt: info.event.extendedProps.showEventAt,
                    hideEventAt: info.event.extendedProps.hideEventAt,
                    complete: info.event.extendedProps.complete
                };

                // Update the event in the database via AJAX
                $.ajax({
                    url: '/calendar/updateEvent', // URL of your server-side update endpoint
                    type: 'POST',
                    data: JSON.stringify(updatedEvent), // Send updated event data as JSON
                    contentType: 'application/json',
                    success: function (response) {
                        //console.log('Event updated successfully in the database.');
                        // Optionally, you can refresh the calendar events if needed
                        // calendar.refetchEvents(); // Call this if you need to reload events from the server
                    },
                    error: function () {
                        //console.error('Failed to update the event in the database.');
                        // Optionally, revert the event to its original state if the update fails
                        info.revert();
                    }
                });
            },
            datesRender: function () {
                // This will ensure that the icon is properly set when the view changes
                $('.fc-myCustomButton-button').html('<i class="fas fa-expand-alt"></i>');
            },
        });

        calendar.render();
        $('.fc-myCustomButton-button').html('<i class="fas fa-expand-alt"></i>'); // Set initial icon
    }

    return {
        //main function to initiate the module
        init: function () {
            initExternalEvents();
            initCalendar();
        }
    };
}();

jQuery(document).ready(function () {
    KTCalendarExternalEvents.init();
});

function updateUrl(id) {
    //update url of modal popup for edit event
    var urlEdit = $('#editModal').attr('href');
    $('#editModal').attr("href", urlEdit + 'id=' + id);
    $('#editModal').data("delete-button", "/calendar/deleteevent?id=" + id);
}

$('#viewModal').on('hidden.bs.modal', function () {
    $('#editModal').attr("href", "/calendar/_editEvent?");
});

function resetFilter() {
    // Reset the filter form fields to their default state
    document.getElementById('calendar-filter-form').reset();
    $('.select2').select2();

    if (filterApplied) {
        filterApplied = false;
        // Refresh the existing calendar's events instead of reinitializing the calendar
        calendar.refetchEvents(); // Refresh the calendar to remove filters and show all events
    }

    $('#filter-modal').trigger('click');
}

function applyFilter() {
    const campusId = $('#CampusFilter').val();
    const eventTypeId = $('#ChurchEventTypeFilter').val();

    // Check if no filters are applied
    if (!campusId && !eventTypeId) {
        Notify("warning", "Note:", 'Specify a campus or event type.');
        return false;
    }

    // Store the filter values globally to be used in the calendar's event fetching
    window.selectedCampusId = campusId;
    window.selectedEventTypeId = eventTypeId;

    filterApplied = true;
    $('#filter-modal').trigger('click');

    // Refetch the events in the calendar using the new filters
    calendar.refetchEvents();
}

// Utility function to generate event tooltip content
function generateEventTooltip(evt) {
    // Safely generate the tooltip content with fallback values
    let campusName = evt.CampusName ? `<strong>${evt.CampusName}</strong><br/>` : '';
    let timeRange = evt.AllDay ? "All Day" : `${evt.StartTime} - ${evt.EndTime}`;
    let description = evt.Description ? `<br/>${evt.Description}` : '';

    // Return a properly formatted tooltip content with fallback values
    return `${campusName}${timeRange}${description}`;
}

// Function to initialize tooltips for events
function initializeTooltip(element, content) {
    // Ensure the content is not empty or undefined before initializing the tooltip
    if (content && content.trim() !== '') {
        element.tooltip({
            html: true,
            title: content,
            placement: 'top',
            trigger: 'hover',
            container: 'body'
        });
    }
}

// Helper function to format a Date object into MM/DD/YYYY
function formatDateToMMDDYYYY(dateStr) {
    // Split the date part from the time part (if any)
    const datePart = dateStr.split('T')[0]; // Extract only the date part, 'YYYY-MM-DD'

    // Now split the date part into year, month, and day
    const parts = datePart.split('-'); // Split the 'YYYY-MM-DD' format
    const year = parts[0];
    const month = parts[1];
    const day = parts[2];

    // Return in MM/DD/YYYY format
    return `${month}/${day}/${year}`;
}