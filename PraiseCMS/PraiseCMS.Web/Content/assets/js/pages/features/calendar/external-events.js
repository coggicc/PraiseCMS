"use strict";

var KTCalendarExternalEvents = function () {

    var initExternalEvents = function () {
        $('#kt_calendar_external_events .fc-draggable-handle').each(function () {
            // store data so the calendar knows to render an event upon drop
            $(this).data('event', {
                title: $.trim($(this).text()), // use the element's text as the event title
                stick: true, // maintain when user navigates (see docs on the renderEvent method)
                classNames: [$(this).data('color')],
                description: 'Lorem ipsum dolor eius mod tempor labore'
            });
        });
    }

    var initCalendar = function () {
        let todayDate = moment().startOf('day');
        //var YM = todayDate.format('YYYY-MM');
        //var YESTERDAY = todayDate.clone().subtract(1, 'day').format('YYYY-MM-DD');
        let TODAY = todayDate.format('YYYY-MM-DD');
        //var TOMORROW = todayDate.clone().add(1, 'day').format('YYYY-MM-DD');

        let calendarEl = document.getElementById('kt_calendar');
        let containerEl = document.getElementById('kt_calendar_external_events');
        let Draggable = FullCalendarInteraction.Draggable;

        new Draggable(containerEl, {
            itemSelector: '.fc-draggable-handle',
            eventData: function (eventEl) {
                return $(eventEl).data('event');
            }
        });

        let allEvents = [];
        let idx = 0;
        let className = "";

        $.ajax({
            type: "GET",
            url: "calendar/getEvents",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                $(response).each(function (index, value) {
                    let item = {};
                    item.title = value.Title;
                    item.description = value.Description;
                    item.id = value.Id;

                    if (idx == 0) { className = "fc-event-primary"; }
                    else if (idx == 1) { className = "fc-event-success"; }
                    else if (idx == 2) { className = "fc-event-danger fc-event-solid-warning"; }
                    else if (idx == 3) { className = "fc-event-light fc-event-solid-primary"; }
                    else if (idx == 4) { className = "fc-event-danger"; }
                    else if (idx == 5) { className = "fc-event-info"; }
                    else if (idx == 6) { className = "fc-event-warning"; }

                    item.className = className;

                    if (value.StartDate !== null) {
                        let stdt = toJavaScriptDate(value.StartDate);
                        item.start = stdt;
                        if (value.StartTime !== null) {
                            item.start = new Date(stdt + ' ' + value.StartTime);
                        }
                    }

                    if (value.EndDate !== null) {
                        let endt = toJavaScriptDate(value.EndDate);
                        item.start = endt;
                        if (value.EndTime !== null) {
                            item.end = new Date(endt + ' ' + value.EndTime);
                        }
                    }

                    allEvents.push(item);
                    idx++;

                    if (idx === 6) {
                        idx = 0;
                    }
                });

                var calendar = new FullCalendar.Calendar(calendarEl, {
                    plugins: ['interaction', 'dayGrid', 'timeGrid', 'list'],

                    isRTL: KTUtil.isRTL(),
                    header: {
                        left: 'prev,next today',
                        center: 'title',
                        right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
                    },

                    height: 800,
                    contentHeight: 780,
                    aspectRatio: 3,  // see: https://fullcalendar.io/docs/aspectRatio

                    nowIndicator: true,
                    now: TODAY + 'T09:25:00', // just for demo

                    views: {
                        dayGridMonth: { buttonText: 'month' },
                        timeGridWeek: { buttonText: 'week' },
                        timeGridDay: { buttonText: 'day' },
                        listWeek: { buttonText: 'list' }
                    },

                    defaultView: 'dayGridMonth',
                    defaultDate: TODAY,

                    droppable: true, // this allows things to be dropped onto the calendar
                    editable: true,
                    eventLimit: true, // allow "more" link when too many events
                    navLinks: true,

                    events: allEvents,

                    //events: [
                    //    {
                    //        title: 'All1 1Day Event',
                    //        start: YM + '-01',
                    //        description: 'Toto lorem ipsum dolor sit incid idunt ut',
                    //        className: "fc-event-danger fc-event-solid-warning"
                    //    },
                    //    {
                    //        title: 'Reporting1',
                    //        start: YM + '-14T13:30:00',
                    //        description: 'Lorem ipsum dolor incid idunt ut labore',
                    //        end: YM + '-14',
                    //        className: "fc-event-success"
                    //    },
                    //    {
                    //        title: 'Company Trip',
                    //        start: YM + '-02',
                    //        description: 'Lorem ipsum dolor sit tempor incid',
                    //        end: YM + '-03',
                    //        className: "fc-event-primary"
                    //    },
                    //    {
                    //        title: 'ICT Expo 2017 - Product Release',
                    //        start: YM + '-03',
                    //        description: 'Lorem ipsum dolor sit tempor inci',
                    //        end: YM + '-05',
                    //        className: "fc-event-light fc-event-solid-primary"
                    //    },
                    //    {
                    //        title: 'Dinner',
                    //        start: YM + '-12',
                    //        description: 'Lorem ipsum dolor sit amet, conse ctetur',
                    //        end: YM + '-10'
                    //    },
                    //    {
                    //        id: 999,
                    //        title: 'Repeating Event',
                    //        start: YM + '-09T16:00:00',
                    //        description: 'Lorem ipsum dolor sit ncididunt ut labore',
                    //        className: "fc-event-danger"
                    //    },
                    //    {
                    //        id: 1000,
                    //        title: 'Repeating Event',
                    //        description: 'Lorem ipsum dolor sit amet, labore',
                    //        start: YM + '-16T16:00:00'
                    //    },
                    //    {
                    //        title: 'Conference',
                    //        start: YESTERDAY,
                    //        end: TOMORROW,
                    //        description: 'Lorem ipsum dolor eius mod tempor labore',
                    //        className: "fc-event-primary"
                    //    },
                    //    {
                    //        title: 'Meeting',
                    //        start: TODAY + 'T10:30:00',
                    //        end: TODAY + 'T12:30:00',
                    //        description: 'Lorem ipsum dolor eiu idunt ut labore'
                    //    },
                    //    {
                    //        title: 'Lunch',
                    //        start: TODAY + 'T12:00:00',
                    //        className: "fc-event-info",
                    //        description: 'Lorem ipsum dolor sit amet, ut labore'
                    //    },
                    //    {
                    //        title: 'Meeting',
                    //        start: TODAY + 'T14:30:00',
                    //        className: "fc-event-warning",
                    //        description: 'Lorem ipsum conse ctetur adipi scing'
                    //    },
                    //    {
                    //        title: 'Happy Hour',
                    //        start: TODAY + 'T17:30:00',
                    //        className: "fc-event-info",
                    //        description: 'Lorem ipsum dolor sit amet, conse ctetur'
                    //    },
                    //    {
                    //        title: 'Dinner',
                    //        start: TOMORROW + 'T05:00:00',
                    //        className: "fc-event-solid-danger fc-event-light",
                    //        description: 'Lorem ipsum dolor sit ctetur adipi scing'
                    //    },
                    //    {
                    //        title: 'Birthday Party',
                    //        start: TOMORROW + 'T07:00:00',
                    //        className: "fc-event-primary",
                    //        description: 'Lorem ipsum dolor sit amet, scing'
                    //    },
                    //    {
                    //        title: 'Click for Google',
                    //        url: 'http://google.com/',
                    //        start: YM + '-28',
                    //        className: "fc-event-solid-info fc-event-light",
                    //        description: 'Lorem ipsum dolor sit amet, labore'
                    //    }
                    //],
                    eventClick: function (info) {
                        $.ajax({
                            url: "calendar/getEventById/" + info.event.id,
                            type: 'GET',
                            dataType: "html",
                            success: function (data) {
                                $('#htmlContent').html(data);
                                $('#editEvent').trigger('click');
                            },
                            failure: function (response) {
                                alert(response.responseText);
                            },
                            error: function (response) {
                                alert(response.responseText);
                            }
                        });
                    },
                    eventDrop: function (info) {
                        // alert(info.event.title + " was dropped on " + info.event.start.toISOString());
                        let id = null;
                        if (typeof (info.event.id) !== 'undefined') {
                            id = info.event.id;
                        }
                        let startDate = null;
                        if (typeof (info.event.start) !== 'undefined') {
                            startDate = getStringDate(info.event.start);
                        }
                        var model = {
                            Id: id,
                            StartDate: startDate,
                        };
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

                        var model = {
                            Id: id,
                            Title: arg.draggedEl.textContent,
                            Description: description,
                            StartDate: arg.dateStr,
                            AllDay: arg.allDay,
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

                        if (info.event.extendedProps && info.event.extendedProps.description) {
                            if (element.hasClass('fc-day-grid-event')) {
                                element.data('content', info.event.extendedProps.description);
                                element.data('placement', 'top');
                                KTApp.initPopover(element);
                            } else if (element.hasClass('fc-time-grid-event')) {
                                element.find('.fc-title').append('<div class="fc-description">' + info.event.extendedProps.description + '</div>');
                            } else if (element.find('.fc-list-item-title').lenght !== 0) {
                                element.find('.fc-list-item-title').append('<div class="fc-description">' + info.event.extendedProps.description + '</div>');
                            }
                        }
                    }
                });

                calendar.render();

            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    }

    var toJavaScriptDate = function (value) {
        let pattern = /Date\(([^)]+)\)/;
        let results = pattern.exec(value);
        let dt = new Date(parseFloat(results[1]));
        let month = (dt.getMonth() + 1);
        let day = dt.getDate();
        if (month > 0 && month < 10) {
            month = '0' + month;
        }
        if (day > 0 && day < 10) {
            day = '0' + day;
        }
        return dt.getFullYear() + "-" + month + "-" + day;
    }
    var saveUpdateEvent = function (model) {
        $.ajax({
            url: 'calendar/createEventOnDragAndDrop',
            type: 'POST',
            data: model,
            success: function (response) {
                if (!response) {
                }
            },
            error: function () {
                alert(response.responseText);
            }
        });
    }

    var getStringDate = function (date) {
        return date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate();
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
