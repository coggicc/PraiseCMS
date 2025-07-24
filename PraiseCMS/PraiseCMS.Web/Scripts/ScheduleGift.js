function startDateSelect(eve) {
    $('#ScheduledPayment_RecurringEndDate').val('');
    switch ($('#ScheduledPayment_RecurringFrequency').val()) {
        case "1st and 15th Monthly":
            var date = eve.value.split('/');
            $('.datepickerEnd').datepicker('destroy');
            $('.datepickerEnd').datepicker({
                todayHighlight: true,
                todayBtn: "linked",
                autoclose: true,
                startDate: new Date(date[2], parseInt(date[0]) - 1, date[1]),
                beforeShowDay: enableDate
            });
            break;
        case "Monthly":
            setFixDateForMonth('datepickerEnd');
            break;
        case "Biweekly":
            setFixDateForBiWeek('datepickerEnd');
            break;
        case "Weekly":
            setFixDateForWeek('datepickerEnd');
            break;
    }
}

function frequencyDate_initialize() {
    conditionalDatePicker($('#ScheduledPayment_RecurringFrequency').val());
}

function datepicker(className) {
    $('.' + className).datepicker('destroy');
    $('.' + className).datepicker({
        todayHighlight: true,
        todayBtn: "linked",
        autoclose: true,
        startDate: new Date()
    });
    if (className === 'datepicker') {
        $('.' + className).datepicker('setDate', new Date());
    }
    if (className === 'datepickerEnd') {
        $('.' + className).val('');
    }
}

function setFixDate(className) {
    let date = new Date();
    let setDate;
    if (date.getDate() > 1 && date.getDate() <= 15) {
        setDate = (date.getMonth() + 1) + '/15/' + date.getFullYear();
    } else if (date.getDate() === 1) {
        setDate = (date.getMonth() + 1) + '/01/' + date.getFullYear();
    } else {
        if (date.getMonth() === 11) {
            setDate = ('01/01' + ' / ' + (date.getFullYear() + 1));
        } else {
            setDate = (date.getMonth() + 2) + '/01/' + date.getFullYear();
        }
    }
    $('.' + className).datepicker('destroy');
    $('.' + className).datepicker({
        autoclose: true,
        dateFormat: "mm/dd/yy",
        startDate: new Date(),
        beforeShowDay: enableDate,
    });
    $('.' + className).datepicker('setDate', new Date(setDate));

}

function enableDate(date) {
    if (date.getDate() === 1 || date.getDate() === 15) { return { enabled: true } }
    else { return { enabled: false } }
}

function enableAllDates() {
    return { enabled: true };
}

$(document).on('focus', '.datepicker', function (e) {
    window.setTimeout(function () {
        var days = $(".datepicker-days td.day");
        if ($('#ScheduledPayment_RecurringFrequency').val() === "1st and 15th Monthly") {
            $.each(days, function () {
                var day = $(this);
                if (!day.hasClass("disabled")) {
                    day.addClass("active-day");
                }
            });
        }
    }, 100);
});

$(document).on('focus', '.datepickerEnd', function (e) {
    window.setTimeout(function () {
        var days = $(".datepicker-days td.day");
        if ($('#ScheduledPayment_RecurringFrequency').val() === "1st and 15th Monthly") {
            $.each(days, function () {
                var day = $(this);
                if (!day.hasClass("disabled")) {
                    day.addClass("active-day");
                }
            });
        }
    }, 100);
});

$(document).on('change', '#ScheduledPayment_RecurringFrequency', function () {
    const val = $(this).val();
    conditionalDatePicker(val);
});

function conditionalDatePicker(val) {
    switch (val) {
        case "1st and 15th Monthly":
            setFixDate('datepicker');
            setFixDate('datepickerEnd');
            $('#end-date-note').html('');
            break;
        case "Monthly":
            datepicker('datepicker');
            setFixDateForMonth('datepickerEnd');
            $('#end-date-note').html('Note: Please select an ending date at least one month in the future.');
            break;
        case "Biweekly":
            datepicker('datepicker');
            setFixDateForBiWeek('datepickerEnd');
            $('#end-date-note').html('Note: Please select an ending date at least two weeks in the future.');
            break;
        case "Weekly":
            datepicker('datepicker');
            setFixDateForWeek('datepickerEnd');
            $('#end-date-note').html('Note: Please select an ending date at least one week in the future.');
            break;
    }
}


function setFixDateForMonth(className) {
    let date = $('#ScheduledPayment_RecurringStartDate').val().split('/');
    var newDate = new Date(date[2], parseInt(date[0]) - 1, date[1]);
    newDate = new Date(newDate.setMonth(newDate.getMonth() + 1));
    $('.' + className).datepicker('destroy');
    $('.' + className).datepicker({
        autoclose: true,
        dateFormat: "mm/dd/yy",
        startDate: new Date(newDate.getFullYear(), newDate.getMonth(), newDate.getDate()),
        //beforeShowDay: enableNextMonthDate,
    });
    $('.' + className).val('');
}

function setFixDateForBiWeek(className) {
    const date = $('#ScheduledPayment_RecurringStartDate').val().split('/');
    var newDate = new Date(date[2], parseInt(date[0]) - 1, date[1]);
    newDate = new Date(newDate.setDate(newDate.getDate() + 14));
    $('.' + className).datepicker('destroy');
    $('.' + className).datepicker({
        autoclose: true,
        dateFormat: "mm/dd/yy",
        startDate: new Date(newDate.getFullYear(), newDate.getMonth(), newDate.getDate()),
        //beforeShowDay: enableNextBiWeekDate,
    });
    $('.' + className).val('');
}

function setFixDateForWeek(className) {
    _enableDatesForWeekly = [];
    const date = $('#ScheduledPayment_RecurringStartDate').val().split('/');
    var newDate = new Date(date[2], parseInt(date[0]) - 1, date[1]);
    newDate = new Date(newDate.setDate(newDate.getDate() + 7));
    $('.' + className).datepicker('destroy');
    $('.' + className).datepicker({
        autoclose: true,
        dateFormat: "mm/dd/yy",
        startDate: new Date(newDate.getFullYear(), newDate.getMonth(), newDate.getDate()),
        //beforeShowDay: enableNextBiWeekDate,
    });
    $('.' + className).val('');
}