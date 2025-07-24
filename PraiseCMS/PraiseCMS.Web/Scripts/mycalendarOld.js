"use strict";
var dateRange = '';
var dataDatesList = [];
let allEvents = [];
let setOfEvents = [];
let idx = 0;
let className = "";
let eventId = '';
let vw = '';
var calendar;
var filterApplied = false;
let views = "timeGridWeek,timeGridDay,listWeek";

var KTCalendarExternalEvents = function () {
    var isInitialLoad = true; // Flag to check if it's the first load
    var initExternalEvents = function () {
        $('#kt_calendar_external_events .fc-draggable-handle').each(function () {
            $(this).data('event', {
                title: $.trim($(this).text()),
                stick: true,
                classNames: [$(this).data('color')],
                description: 'Lorem ipsum dolor eius mod tempor labore'
            });
        });
    };

    var initCalendar = function () {
        let todayDate = moment().startOf('day');
        let TODAY = todayDate.format('YYYY-MM-DD');
        let calendarEl = document.getElementById('kt_calendar');
        let containerEl = document.getElementById('kt_calendar_external_events');
        let margin = 'margin-top: 25%;';

        if (!containerEl) {
            margin = '';
        }

        $(calendarEl).html("<div style='text-align:center;" + margin + "'><i class='mr-5'>Loading Calendar...</i><i class='spinner-border text-primary'></i></div>");

        let Draggable = FullCalendarInteraction.Draggable;

        if (containerEl) {
            new Draggable(containerEl, {
                itemSelector: '.fc-draggable-handle',
                eventData: function (eventEl) {
                    let item = $(eventEl).data('event');
                    item.id = generateUniqueId();
                    item.description = eventEl.dataset.description;
                    eventId = item.id;
                    item.CalendarColor = eventEl.dataset.color;
                    item.className = "fc-event-" + eventEl.dataset.color;
                    return item;
                }
            });
        }

        allEvents = [];
        setOfEvents = [];
        idx = 0;
        className = "";
        let type = "dayGridMonth";
        let title = "";
        let start = new Date().toISOString(); // Use ISO 8601 format for start date

        let eventSession = sessionStorage.getItem("eventSession");
        eventSession = JSON.parse(eventSession);

        if (eventSession !== null && typeof eventSession !== 'undefined') {
            type = eventSession.type;
            title = eventSession.title;
            if (typeof eventSession.start === 'undefined') {
                start = todayDate.format('YYYY-MM-DD');
            } else {
                start = formDateToJSDate(new Date(eventSession.start));
                sessionStorage.clear();
            }
        }

        $.ajax({
            type: "GET",
            url: "/calendar/getEvents?campusId=" + $('#CampusFilter').val() + "&eventTypeId=" + $('#ChurchEventTypeFilter').val() + "&dateRange=" + dateRange,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                let response = data.formattedEvents;
                dataDatesList = data.stringDatesList;

                for (let i = 0; i < response.length; i++) {
                    let value = response[i];
                    setAllEvents(value);
                }

                $(calendarEl).html('');
                calendar = new FullCalendar.Calendar(calendarEl, {
                    plugins: ['interaction', 'dayGrid', 'timeGrid', 'list'],
                    isRTL: KTUtil.isRTL(),
                    header: {
                        left: 'prev,next today',
                        center: 'title',
                        right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
                    },
                    height: 800,
                    contentHeight: 780,
                    aspectRatio: 3,
                    nowIndicator: true,
                    now: TODAY + 'T09:25:00',
                    views: {
                        dayGridMonth: {
                            buttonText: 'Month'
                        },
                        timeGridWeek: {
                            buttonText: 'Week'
                        },
                        timeGridDay: {
                            buttonText: 'Day'
                        },
                        listWeek: {
                            buttonText: 'List'
                        }
                    },
                    defaultView: type,
                    defaultDate: start,
                    droppable: true,
                    editable: true,
                    eventLimit: true,
                    navLinks: true,
                    events: setOfEvents,
                    datesRender: function (info) {
                        if (isInitialLoad) {
                            isInitialLoad = false; // Skip AJAX request on initial load
                            return;
                        }
                        let End = info.view.activeEnd;
                        End.setDate(End.getDate() - 1);
                        let endDate = (End.getMonth() + 1) + '/' + End.getDate() + '/' + End.getFullYear();
                        let startDate = (info.view.activeStart.getMonth() + 1) + '/' + info.view.activeStart.getDate() + '/' + info.view.activeStart.getFullYear();
                        dateRange = startDate + '-' + endDate;

                        if (!dataDatesList.includes(startDate) || !dataDatesList.includes(endDate)) {
                            start = info.view.activeStart;
                            $.ajax({
                                type: "GET",
                                url: "calendar/getEvents?campusId=" + $('#CampusFilter').val() + "&eventTypeId=" + $('#ChurchEventTypeFilter').val() + "&dateRange=" + dateRange,
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (data) {
                                    let response = data.formattedEvents;
                                    $.each(data.stringDatesList, function (index, value) {
                                        dataDatesList.push(value);
                                    });
                                    setOfEvents = [];
                                    for (let i = 0; i < response.length; i++) {
                                        //setAllEvents(response[i]);
                                    }
                                    $.each(setOfEvents, function (index, value) {
                                        //calendar.addEvent(value);
                                    });
                                }
                            });
                        }
                    },
                    eventClick: function (info) {
                        let obj = {
                            type: info.view.type,
                            title: info.view.title,
                            start: info.view.activeStart
                        };

                        if (views.includes(info.view.type)) {
                            obj.start = info.view.activeStart;
                        } else {
                            obj.start = info.view.currentStart;
                        }

                        //sessionStorage.setItem("eventSession", JSON.stringify(obj));
                        //update url of modal popup for edit event
                        var urlView = $('#viewModal').attr('href');

                        var date = new Date(info.event.start),
                            yr = date.getFullYear(),
                            month = date.getMonth() + 1,
                            day = date.getDate(),
                            newDate = yr + '-' + month + '-' + day;

                        let url = urlView + 'id=' + info.event.id + '&date=' + newDate;

                        $('#viewModal').attr("href", url);

                        $('#viewModal').trigger('click');

                        setTimeout(function () {
                            $('#viewModal').attr("href", urlView);
                        });

                        //update url of modal popup for edit event
                        updateUrl(info.event.id);
                    },
                    eventDrop: function (info) {
                        var model = {
                            Id: 0,
                            StartDate: null,
                            EndDate: null,
                            StartTime: null,
                            EndTime: null,
                            CalendarColor: null,
                            Type: 'eventDrop',
                        };

                        if (typeof (info.event.id) !== 'undefined' && info.event.id !== null) {
                            model.Id = info.event.id;
                        }

                        if (typeof (info.event.start) !== 'undefined' && info.event.start !== null) {
                            model.StartDate = getStringDate(info.event.start);
                            model.StartTime = info.event.start.toLocaleTimeString();
                        }

                        if (typeof (info.event.end) !== 'undefined' && info.event.end !== null) {
                            model.EndDate = getStringDate(info.event.end);
                            model.EndTime = info.event.end.toLocaleTimeString();
                        }

                        if (typeof (info.event.calendarcolor) !== 'undefined' && info.event.calendarcolor !== null) {
                            model.CalendarColor = info.event.calendarcolor;
                        }

                        saveUpdateEvent(model);
                    },
                    eventResize: function (info) {
                        let id = null;

                        if (typeof (info.event) !== 'undefined' && typeof (info.event.id) !== 'undefined') {
                            id = info.event.id;
                        }

                        var model = {
                            Id: id,
                            StartDate: '',
                            EndDate: '',
                            StartTime: '',
                            EndTime: '',
                            EventId: id,
                            Type: 'resize'
                        };

                        if (typeof (info.event.start) !== 'undefined' && info.event.start !== null) {
                            model.StartDate = getStringDate(info.event.start);
                            model.StartTime = info.event.start.toLocaleTimeString();
                        }

                        if (typeof (info.event.end) !== 'undefined' && info.event.end !== null) {
                            model.EndDate = getStringDate(info.event.end);
                            model.EndTime = info.event.end.toLocaleTimeString();
                        }

                        saveUpdateEvent(model);
                    },
                    drop: function (arg) {
                        let id = null;
                        if (typeof (arg.draggedEl.dataset) !== 'undefined' && typeof (arg.draggedEl.dataset.id) !== 'undefined') {
                            id = arg.draggedEl.dataset.id;
                        }

                        let description = null;
                        if (typeof (arg.draggedEl.dataset) !== 'undefined' && typeof (arg.draggedEl.dataset.description) !== 'undefined') {
                            description = arg.draggedEl.dataset.description;
                        }

                        let calendarColor = null;
                        if (typeof (arg.draggedEl.dataset) !== 'undefined' && typeof (arg.draggedEl.dataset.color) !== 'undefined') {
                            calendarColor = arg.draggedEl.dataset.color;
                        }

                        var model = {
                            Id: id,
                            Title: arg.draggedEl.textContent,
                            Description: description,
                            StartDate: arg.dateStr,
                            EventId: eventId,
                            CalendarColor: calendarColor,
                            Type: 'drop'
                        };
                        saveUpdateEvent(model);

                        // is the "remove after drop" checkbox checked?
                        if ($('#kt_calendar_external_events_remove').is(':checked')) {
                            // if so, remove the element from the "Draggable Events" list
                            $(arg.draggedEl).remove();
                        }
                    },
                    eventRender: function (info) {
                        var element = $(info.el);

                        // Add custom data attributes
                        element.data('html', true);
                        element.data('delay', {
                            "show": 500,
                            "hide": 0
                        });

                        // Determine calendar color class
                        var calendarColorClass = info.event.extendedProps.Complete ? 'bg-secondary-o-50' : `bg-${info.event.extendedProps.CalendarColor || 'primary'}-o-50`;

                        // Add calendar color class to fc-content
                        element.find('.fc-content').addClass(calendarColorClass);

                        // Handle description tooltip for events with descriptions
                        if (info.event.extendedProps && info.event.extendedProps.description) {
                            if (element.hasClass('fc-day-grid-event')) {
                                element.data('content', info.event.extendedProps.description);
                                element.data('placement', 'top');
                                KTApp.initPopover(element);
                            } else if (element.hasClass('fc-time-grid-event')) {
                                element.find('.fc-title').append('<div class="fc-description">' + info.event.extendedProps.description + '</div>');
                            } else if (element.find('.fc-list-item-title').length !== 0) {
                                element.find('.fc-list-item-title').append('<div class="fc-description">' + info.event.extendedProps.description + '</div>');
                            }
                        }
                    }
                });
                calendar.render();
                StopLoading();
            },
            failure: function (response) {
                StopLoading();
                alert(response.responseText);
            },
            error: function (response) {
                StopLoading();
                alert(response.responseText);
            }
        });
    };

    var generateUniqueId = function () {
        var guid = createGuid();
        guid = guid.toString().replaceAll("-", "");

        var dt = new Date(); // Your date
        var ticks = (dt.getTime() * 10000).toString();

        var id = ticks.substring(ticks.toString().length - 10) + guid;

        return id.substring(0, 30);
    };

    var createGuid = function () {
        function _p8(s) {
            var p = (Math.random().toString(16) + "000000000").substr(2, 8);
            return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
        }
        return _p8() + _p8(true) + _p8(true) + _p8();
    };

    var saveUpdateEvent = function (model) {
        $.ajax({
            url: 'calendar/createEventOnDragAndDrop',
            type: 'POST',
            data: model,
            success: function (response) {
                if (response.Success) {
                    Notify("success", "SUCCESS", response.Message);
                } else {
                    ShowErrorAlert(result.Message);
                }
            },
            error: function () {
                alert(response.responseText);
            }
        });
    };

    var setAllEvents = function (value) {
        let item = {
            title: value.Title,
            description: `<b style="color">${value.Title}</b><br/><em>${value.AllDay ? "All Day" : value.StartTime + " - " + value.EndTime}</em><b>${value.Complete ? " (Over)" : ""}</b><br/>${value.Description || ""}`,
            id: value.Id,
            uniqeId: value.UniqeId,
            Type: value.Type,
            Complete: value.Complete,
            className: "customEvent",
            editable: value.Type !== "ChurchEvent" && value.CreatedBy === $('#login-user-id').val(),
            //CalendarColor: value.CalendarColor // Include CalendarColor in event data
        };

        item.className += ` fc-event-${value.CalendarColor || "primary"}`;

        // Set start and end dates directly if already formatted in controller
        item.start = value.StartDate || undefined;
        item.end = value.EndDate || null;

        //// Avoid duplicate events
        //if (!allEvents.find(x => x.uniqeId === item.uniqeId)) {
        //    allEvents.push(item);
        //    setOfEvents.push(item);
        //}
        allEvents.push(item);
        setOfEvents.push(item);
    };

    return {
        // main function to initiate the module
        init: function () {
            initExternalEvents();
            initCalendar();
        }
    };
}();

$(document).on("click", "#addModal", function () {
    let obj = {
        type: calendar.view.type,
        title: calendar.view.title,
        start: calendar.view.activeStart
    };

    if (views.includes(calendar.view.type)) {
        obj.start = calendar.view.activeStart;
    } else {
        obj.start = calendar.view.currentStart;
    }

    //sessionStorage.setItem("eventSession", JSON.stringify(obj));
});

jQuery(document).ready(function () {
    KTCalendarExternalEvents.init();
});

function updateUrl(id) {
    //update url of modal popup for edit event
    var urlEdit = $('#editModal').attr('href');
    $('#editModal').attr("href", urlEdit + 'id=' + id);
    $('#editModal').data("delete-button", "/calendar/deleteevent?id=" + id);
}

$('#ajax-modal').on('hidden.bs.modal', function () {
    $('#editModal').attr("href", "/calendar/_editEvent?");
});

function resetFilter() {
    document.getElementById('calendar-filter-form').reset();
    $('.select2').select2();
    if (filterApplied) {
        filterApplied = false;
        KTCalendarExternalEvents.init();
    }
    $('#filter-modal').trigger('click');
}

function applyFilter() {
    if (!$('#CampusFilter').val() && !$('#ChurchEventTypeFilter').val()) {
        Notify("warning", "WARNING", 'No Filter Applied!');
        return false;
    }
    filterApplied = true;
    $('#filter-modal').trigger('click');
    KTCalendarExternalEvents.init();
}

//$(document).on("click", ".newEvent", function () {
//    $('#btn-edit').hide();
//    $('#btn-close').show();
//});
//$('#ajax-modal').on('shown.bs.modal', function () {
//    if (!$('#event-div').is(':visible')) {
//        $('#btn-edit').attr('hidden', false);
//    }
//    else {
//        $('#btn-edit').attr('hidden', true);
//    }
//})


/*
Test method to check calendar functionality.
*/
//$(document).ready(function () {
//    // Get today's date and time
//    let today = new Date();
//    let tomorrow = new Date(today);
//    tomorrow.setDate(today.getDate() + 1); // Set to tomorrow

//    // Format dates in ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ)
//    let startDate = today.toISOString().slice(0, 19) + 'Z';
//    let endDate = tomorrow.toISOString().slice(0, 19) + 'Z';

//    // Define the event with dynamic dates
//    var simpleEvent = [{
//        title: 'Test Event',
//        start: startDate, // Today's date and time
//        end: endDate // Tomorrow's date and time
//    }];

//    // Initialize FullCalendar
//    let calendarEl = document.getElementById('kt_calendar');

//    let calendar = new FullCalendar.Calendar(calendarEl, {
//        plugins: ['interaction', 'dayGrid', 'timeGrid', 'list'],
//        initialView: 'dayGridMonth', // Initial view
//        events: simpleEvent // Use the dynamic event data
//    });

//    calendar.render();
//});