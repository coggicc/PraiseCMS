function initializeScripts() {
    $('[data-toggle="tooltip"]').tooltip({
        trigger: 'hover',
        html: true // This option allows HTML to be rendered inside the tooltip
    });

    $(".select2").select2();

    $(".phone").inputmask("mask", {
        "mask": "(999) 999-9999"
    });

    $(".amount").mask("000,000,000.00", {
        reverse: true
    });

    $('.dateOfBirth').mask("00/00/0000", {
        placeholder: "MM/DD/YYYY"
    });

    $("#expiryDate").mask("99/9999");

    $(".taxid").inputmask({
        "mask": "99-9999999",
        placeholder: "XX-XXXXXXX" // remove underscores from the input mask
    });

    $('.datepicker').datepicker({ format: 'mm/dd/yyyy', orientation: "bottom left", todayBtn: "linked", todayHighlight: true, autoclose: true, });
}

$(document).ready(function () {
    initializeScripts();
});

//Reinitializing after loading content via AJAX
$(document).on('ajaxComplete', function () {
    initializeScripts();
});

$(document).ready(function () {
    /*Added start and stop loading code below on 6/17/2024 to handle "please wait..." when modelstate has an error*/
    // Global AJAX event handlers
    $(document).ajaxStart(function () {
        StartLoading();
    }).ajaxStop(function () {
        StopLoading();
    });

    $('.card-toolbar a').click(function () {
        $(this).find('i.details').toggleClass('fa-plus fa-minus');
    });

    $(document).on("keyup", '.amount', function () {
        var $this = $(this);
        var $rawValue = $this.val().replace(',', '');
        var $amount = parseFloat($rawValue);
        if ($amount > 25000.00) {
            $(".btn_submit, input[type=submit], #next-step").prop('disabled', true);
            $this.addClass("block-form-control");
        } else {
            $(".btn_submit, input[type=submit], #next-step").prop('disabled', false);
            $this.removeClass("block-form-control");
        }
    });

    ClassicEditor.defaultConfig = {
        toolbar: {
            items: [
                'heading',
                '|',
                'bold',
                'italic',
                '|',
                'bulletedList',
                'numberedList',
                '|',
                'insertTable',
                '|',
                'blockQuote',
                '|',
                'undo',
                'redo'
            ]
        },
        image: {
            toolbar: [
                'imageStyle:full',
                'imageStyle:side',
                '|',
                'imageTextAlternative'
            ]
        },
        table: {
            contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
        },
        language: 'en'
    };
});

$(document).on('click', '.ajax-modal', function (e) {
    if ($(this).hasClass('backdropStatic')) {
        $('#ajax-modal').data('bs.modal')._config.keyboard = false;
        $('#ajax-modal').data('bs.modal')._config.backdrop = 'static';
    } else {
        $('#ajax-modal').data('bs.modal')._config.keyboard = true;
        $('#ajax-modal').data('bs.modal')._config.backdrop = true;
    }
});

$('#ajax-modal').on('hidden.bs.modal', function () {
    $('#btn-edit').hide();
    $('#btn-close').show();
});

// Function to generate error HTML
function generateErrorHtml(message) {
    return `
        <div class='alert alert-custom alert-notice alert-light-danger fade show mb-5' role='alert'>
            <div class='alert-icon'><i class='fas fa-exclamation-triangle'></i></div>
            <div class='alert-text'><strong>Something went wrong. Please try again.</strong><br />Error: ${message}</div>
            <div class='alert-close'>
                <button type='button' class='close' data-dismiss='alert' aria-label='Close'>
                    <span aria-hidden='true'><i class='ki ki-close'></i></span>
                </button>
            </div>
        </div>
    `;
}

function Notify(theme, title, message, url = "", target = "") {
    var content = {};
    content.message = message;
    content.title = title;
    content.url = url;
    content.target = target;
    $.notify(content, {
        timer: 1000,
        allow_dismiss: true,
        newest_on_top: true,
        allow_duplicates: false,
        type: theme,
        showProgressbar: true,
        z_index: 99999,
        animate: {
            enter: 'animated fadeInDown',
            exit: 'animated fadeOutUp'
        }
    });
}

function formatCurrency(value) {
    if (value < 0) {
        value = '($' + parseFloat(value).toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,").replace("-", "") + ')';
    } else {
        value = '$' + parseFloat(value).toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");
    }
    return value;
}

function deleteMethod(accountGUID, forChurch = false) {
    Swal.fire({
        title: 'Delete Payment Method',
        text: "Are you sure you want to delete this payment method? This cannot be undone.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#aaa',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            let url = window.location.origin;
            if (forChurch) {
                url = url.concat('/Settings/DeleteAccount?accountID=', accountGUID);
            } else {
                url = url.concat('/MyGiving/DeleteAccount?accountID=', accountGUID);
            }
            window.open(url, "_self");
        }
    });
}

$(document).on('keyup', '.dateOfBirth', function () {
    $('#DateOfBirth').val($(this).val());
});

function calculateProcessingFee(churchId, amount, paymentMethod) {
    if ($('#allow_Processing_fee').val() === "true") {
        $.ajax({
            url: '/MyGiving/CalculateProcessingFee',
            type: 'GET',
            dataType: 'json',
            data: {
                ChurchId: churchId,
                Amount: amount,
                AccountGUID: paymentMethod
            },
            success: function (response) {
                if (response === "$0.00") {
                    $('#processingFee-div').hide();
                } else {
                    $('#processingFee-div').show();
                }
                $('#processingFeeAmount').text(response);
            },
            error: function (response) {
                //console.log(response);
            }
        });
    }
}

function toggleDetails() {
    // For card 1 (Giving by Campus)
    let btnCampusDetails = document.querySelector('[data-target="campusDetails"]');
    $(btnCampusDetails).click(function (e) {
        $('.' + $(this).data('target')).toggle();
    });
    // For card 2 (Giving by Fund)
    let btnFundDetails = document.querySelector('[data-target="fundDetails"]');
    $(btnFundDetails).click(function (e) {
        $('.' + $(this).data('target')).toggle();
    });
}

function cardformat(e) {
    let value = $(e).val();
    var v = value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
    var matches = v.match(/\d{4,16}/g);
    var match = matches && matches[0] || '';
    var parts = [];

    for (let i = 0; i < match.length; i += 4) {
        parts.push(match.substring(i, i + 4));
    }

    if (parts.length) {
        $(e).val(parts.join(' '));
    } else {
        $(e).val(value);
    }
}

function verifyDate(datevalue) {
    if (datevalue !== null || datevalue !== '') {
        //split the date as a tmp var
        var tmp = datevalue.split('/');
        //get the month and year
        var month = tmp[0];
        var year = "20" + tmp[1];

        if ((parseInt(month) < 1 || parseInt(month) > 12)) {
            $('#expiryDate').val('');
            errorCardMessage();
        }
        if (parseInt(year) < new Date().getFullYear()) {
            $('#expiryDate').val('');
            errorCardMessage('Expiration date must be in future.');
        }
        if (parseInt(year) === new Date().getFullYear() && (parseInt(month) < (new Date().getMonth() + 1))) {
            $('#expiryDate').val('');
            errorCardMessage('Expiration date must be in future.');
        }
    }
}

$(document).on('keyup', '#expiryDate', function () {
    //get the date
    var datevalue = $(this).val();

    //only if the date is full like this: 'xx/xx' continue
    if (datevalue.length === 5) {
        verifyDate(datevalue);
    }
});

$(document).on('blur', '#expiryDate', function () {
    //get the date
    var datevalue = $(this).val();
    if (datevalue.length === 5) {
        verifyDate(datevalue);
    } else {
        //clean the message
        $('#expiryDate').val('');
        errorCardMessage('Enter a two digit month and year.');
    }
});

$(document).on("keyup", "#cardNumber", function (e) {
    const cardNumber = e.currentTarget.value;
    let cardType, cardLogoSrc;

    const logoBasePath = "/Content/assets/image/card_Logos/";

    if (/^3[47]/.test(cardNumber)) {
        cardType = 'AMEX';
        cardLogoSrc = "american-Express-logo.png";
    } else if (/^(6011|65|64[4-9]|622)/.test(cardNumber)) {
        cardType = 'DISC';
        cardLogoSrc = "discover-card-icon.jpg";
    } else if (/^(5[1-5]|677189)|^(222[1-9]|2[3-6]\d{2}|27[0-1]\d|2720)/.test(cardNumber)) {
        cardType = 'MSTR';
        cardLogoSrc = "mastercard-logo.png";
    } else if (/^4/.test(cardNumber)) {
        cardType = 'VISA';
        cardLogoSrc = "visa_card_logo.png";
    } else {
        cardType = '';
        cardLogoSrc = "blank_card.png";
    }

    $('#cardType').text(cardType);
    $('#cardType').val(cardType);
    $('#card_logo').attr("src", logoBasePath + cardLogoSrc);

    const isValidCard = cardNumber.length === 19;
    cardValidation(isValidCard || !cardType);
});

function cardValidation(isvalid) {
    if (isvalid) {
        $('#cardNumber').removeClass('is-invalid');
        $('.card-validation').removeClass('invalid-feedback').css('display', 'none').text('');
    } else {
        $('#cardNumber').addClass('is-invalid');
        $('.card-validation').addClass('invalid-feedback').css('display', 'block').text('Uh-oh: Please enter a valid card number.');
    }
}

$("#PaymentAccount_RoutingNumber").on("change paste", function () {
    if ($(this).val().length === 9) {
        $.ajax({
            type: "GET",
            url: "/Settings/verifyroutingnumber",
            contentType: "application/json; charset=utf-8",
            data: {
                'routingNumber': $(this).val()
            },
            success: function () { },
            error: function () { }
        });
    }
    if ($(this).val().length > 9) {
        $(this).addClass('is-invalid');
        $('.routing-feedback').addClass('invalid-feedback');
        $('.routing-feedback').text('Uh-oh: Please enter a valid routing number.');
    }
    if ($(this).val().length < 9) {
        $(this).removeClass('is-valid');
        $('.routing-feedback').removeClass('valid-feedback');
        $('.routing-feedback').text('');
    }
});

function getFormattedDate(date) {
    var year = date.getFullYear();

    var month = (1 + date.getMonth()).toString();
    month = month.length > 1 ? month : '0' + month;

    var day = date.getDate().toString();
    day = day.length > 1 ? day : '0' + day;

    return month + '/' + day + '/' + year;
}

function paging(e, page) {
    StartLoading();
    if ($(e).hasClass('previous')) {
        page = page - 1;
    } else if ($(e).hasClass('next')) {
        page = page + 1;
    }
    let currentUrl = window.location;
    if (!currentUrl.href.includes('page')) {
        if (currentUrl.href.includes('?')) {
            let url = currentUrl.href + "&page=" + page;
            location.replace(url);
        } else {
            let url = currentUrl.href + "?page=" + page;
            location.replace(url);
        }
    } else {
        let url = currentUrl.href;
        let val = 0;
        let items = url.split('page=');
        if (items && items.length > 0) {
            if (items[1].includes('&')) {
                let subItems = items[1].split('&');
                if (subItems && subItems.length > 0) {
                    val = parseInt(subItems[0]);
                    url = url.replace("=" + val, "=" + page);
                }
            } else {
                val = parseInt(items[1]);
                url = url.replace("=" + val, "=" + page);
            }
        }
        location.replace(url);
    }
    StopLoading();
}

function LoadRolesData() {
    $.post("/permissions/ViewRolePartial", null, function (result) {
        $("#view-role-container").html('').html(result);
    });
}

function LoadModuleData() {
    $.post("/permissions/ViewModulePartial", null, function (result) {
        $("#view-module-container").html('').html(result);
    });
}

function errorCardMessage(mag = "") {
    msg = mag !== "" ? mag : 'Invalid Expiration Date.';
    $("#expiryErrorMessage").text(msg);
    $("#expiryErrorMessage").fadeIn().delay(2000).fadeOut();
    $("#expiry").focus();
}

function clearIds() {
    $('#prayer-request-details, #log-details').hide();
    $('#prayer-request-container, #log-container').show();
    selectedIds = [];
}

function systemSearch(val) {
    if (!val) {
        return false;
    }
    $.ajax({
        url: '/MyGiving/CheckCardExpiration',
        type: 'GET',
        dataType: 'json',
        data: {
            value: val
        },
        success: function (result) {
            if (result.IsExpiring) {
                //console.log();
            }
        },
        error: function (result) {
            //console.log(result);
        }
    });
}

function checkForAdminUser() {
    if (!$('#collapseOne2').attr('class').includes('show')) {
        $("#AdminUserFirstname").val('');
        $("#AdminUserLastname").val('');
        $("#AdminUserEmail").val('');
        $("#AdminUserPhone").val('');

        $("#adminEmailDiv").hide();
        $("#adminNameDiv").hide();
        $("#adminPhoneDiv").hide();
        $("#adminNote").hide();
    } else {
        $("#adminEmailDiv").show();
        $("#adminNameDiv").show();
        $("#adminPhoneDiv").show();
        $("#adminNote").show();
    }
}

function validateEmail() {
    const emailRegex = /^(([^<>()\[\]\\.,;:\s@@"]+(\.[^<>()\[\]\\.,;:\s@@"]+)*)|(".+"))@@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if (!emailRegex.test($('.validateEmail').val())) {
        Swal.fire({
            text: "Uh-oh! Please enter a valid email and try again.",
            icon: "error",
            buttonsStyling: false,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn font-weight-bold btn-light"
            }
        }).then(function () {
            return false;
        });
    }
}

function formatDate(d) {
    var month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [month, day, year].join('/');
}

$(document).on('keypress', '.numeric', function (e) {
    var keycode = (e.which) ? e.which : e.keyCode;
    //comparing pressed keycodes

    if (keycode > 31 && (keycode < 48 || keycode > 57)) {
        return false;
    } else return true;
});

$('.numeric').on('paste', function (event) {
    if (event.originalEvent.clipboardData.getData('Text').match(/[^\d]/)) {
        event.preventDefault();
    }
});

function wizardValidationMessage(msg = "", type = "", scroll = true, title = "") {
    Swal.fire({
        title: title,
        text: msg ? msg : "Uh-oh! It looks like we need a little more information.",
        icon: type ? type : "warning",
        buttonsStyling: false,
        confirmButtonText: "Ok, got it!",
        customClass: {
            confirmButton: "btn font-weight-bold btn-light"
        }
    }).then(function () {
        if (scroll) {
            KTUtil.scrollTop();
        }
    });
}

function reviewChurchDetail() {
    checkForAdminUser();

    $('#yChurchName').text('');
    $('#yPhone').text('');
    $('#yEmail').text('');
    $('#yDenomination').text('');
    $('#ycAddress').text('');
    $('#ycApt').text('');
    $('#ycCity').text('');
    $('#ycState').text('');
    $('#ycZip').text('');
    $('#adminEmail').text('');
    $('#adminName').text('');
    $('#adminPhone').text('');

    if ($('#Church_Name').val()) {
        $('#yChurchName').text($('#Church_Name').val());
    }

    if ($('#Church_Phone').val()) {
        $('#yPhone').text($('#Church_Phone').val());
    }

    if ($('#Church_Email').val()) {
        $('#yEmail').text($('#Church_Email').val());
    }

    if ($('#Church_Denomination').val()) {
        $('#yDenomination').text($("#Church_Denomination option:selected").text());
    }

    if ($('#Church_PhysicalAddress1').val()) {
        $('#ycAddress').text($("#Church_PhysicalAddress1").val());
    }

    if ($('#Church_PhysicalAddress2').val()) {
        $('#ycAptDiv').show();
        $('#ycApt').text($("#Church_PhysicalAddress2").val());
    } else {
        $('#ycAptDiv').hide();
    }

    if ($('#Church_PhysicalCity').val()) {
        $('#ycCity').text($("#Church_PhysicalCity").val());
    }

    if ($('#Church_PhysicalState').val()) {
        $('#ycState').text($("#Church_PhysicalState").val());
    }

    if ($('#Church_PhysicalZip').val()) {
        $('#ycZip').text($("#Church_PhysicalZip").val());
    }

    let name;
    if ($("#AdminUserFirstname").val()) {
        name = $("#AdminUserFirstname").val();
    }

    if ($("#AdminUserLastname").val()) {
        if (name) {
            name = name + " " + $("#AdminUserLastname").val();
        } else {
            name = $("#AdminUserLastname").val();
        }
    }

    $('#adminName').text(name);

    if ($("#AdminUserEmail").val()) {
        $('#adminEmail').text($("#AdminUserEmail").val());
    }

    if ($("#AdminUserPhone").val()) {
        $('#adminPhone').text($("#AdminUserPhone").val());
    }
}

function StartLoading() {
    //this is the other loader which is called from _PageLoader.cshtml page when any page is loading,
    //First hide this one and then start our loader so that there would be no duplicate loader popups.
    $('.page-loader .blockui').hide();
    KTApp.blockPage({
        overlayColor: '#000000',
        zIndex: 99999,
        type: 'v2',
        state: 'primary',
        message: 'Please wait...'
    });
    loaderStarted = true;
}

function StopLoading() {
    KTApp.unblockPage();
    loaderStarted = false;
}

$(function () {
    $(document).on("click",
        '[type="submit"], .modal-footer [type="submit"], .modal-footer .btn_submit, .modal-footer .modal-delete',
        function () {
            StartLoading();
        });

    $(".weburl").validateUrl();

    // jquery tabs
    $(document).on("keyup", ".websiteUrl", function () {
        const val = $(this).val();
        if (val) {
            const regexp = /^(http[s]?:\/\/){0,1}(www\.){0,1}[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,5}[\.]{0,1}/;
            if (regexp.test(val)) {
                $('.webiste-url-error').css('display', 'none');
                $(this).addClass('is-valid').removeClass('is-invalid');
            } else {
                $(this).removeClass('is-valid').addClass('is-invalid');
                $('.webiste-url-error').css('display', 'block');
            }
        } else {
            $('.webiste-url-error').css('display', 'none');
            $(this).removeClass('is-invalid is-valid');
        }
    });

    $(document).on("click", ".nav-tabs .nav-icon, .nav-tabs .nav-text", function () {
        $(this).closest(".nav-link").click();
    });

    $(document).on("change", ".btn-secondary", function () {
        if ($('input[name="gift_type"]:checked').attr("id") === "one_time") {
            $("#recurring_container").slideUp();
        } else {
            $("#recurring_container").slideDown();
        }
    });

    $(document).on("click", "#load_mobile_verification", function (e) {
        if ($("#select_funds option:selected").text() === 'Select a fund...') {
            $("#fund_error").css('display', 'block');
            return;
        } else {
            $("#fund_error").css('display', 'none');
        }

        $(this).html("").html('<i class="fa fa-spinner fa-spin"></i> Please wait...')
            .attr("disabled", "disabled");

        var data = {
            Amount: $("#txt_amount").val(),
            PaymentTypeId: $('input[name="gift_type"]:checked').attr("id"),
            FrequencyId: $('input[name="frequency"]:checked').attr("id"),
            RecurringStartDate: $("#txt_starting_date").val(),
            RecurringEndDate: $("#txt_ending_date").val(),
            Fund: $("#select_funds").find("option:selected").text(),
            ChurchId: $("#churchId").val()
        };

        if ($("#is_verified").val() === "1") {
            $.post("/Give/UpdateModel", (data), function (result) {
                $.post("/Give/MakePayment", null, function (result) {
                    $("#kt-login__container").html('').html(result);
                });
            });
        } else {
            $.post("/Give/MobileVerification", (data), function (result) {
                $("#kt-login__container").html('').html(result);
            });
        }
    });

    $(document).on("click", "#phone-submit", function () {
        var $btn = $(this);
        var $btn_validation = $("#phone_validation");
        var $number = $("#phone").val();

        if ($("#phone").val() === "") {
            $btn_validation.html("").html("Please enter a phone number.");
            $btn_validation.show();
            return false;
        }

        $("#verification-code").val('');

        $btn.html("").html('<i class="fa fa-spinner fa-spin"></i> Please wait...')
            .attr("disabled", "disabled");

        $.post("/Give/VerificationPhoneCode", ({
            phone: $number
        }), function (res) {

            $btn.html("").html("Sign In").removeAttr("disabled");

            if (res.result !== "success") {
                $btn_validation.html("").html(res.result);
                $btn_validation.show();
            } else {
                $btn_validation.hide();
                $("#exampleModal").modal({
                    show: true
                });
            }
        });
    });

    $(document).on("keyup", "#verification-code", function (e) {
        var $this = $(this);
        var $btn_validation = $("#code_validation");

        if ($this.val().length === 5) {
            $this.attr("disabled", "disabled").attr("readonly", "readonly").css("background-color", "#ccc");

            $.get("/Give/MakePayment", ({
                code: $this.val()
            }), function (result) {

                if (result.result === "failed") {
                    $this.removeAttr("disabled", "disabled").removeAttr("readonly", "readonly").css("background-color", "#fff");
                    $btn_validation.html("").html("The verification code entered incorrect.");
                    $btn_validation.show();
                } else {
                    window.location.href = '/Give/MakePayment?code=' + $this.val();
                }
            });
        }
    });

    $(document).on("click", "#payment-confirmation", function () {
        $(this).html("").html('<i class="fa fa-spinner fa-spin"></i> Please wait...')
            .attr("disabled", "disabled");

        $.post("/Give/PaymentConfirmation", ({
            phone: $("#phone").val()
        }), function (result) {
            $("#kt-login__container").html('').html(result);
        });
    });

    $(document).on("click", "#payment-verify", function () {
        $(this).html("").html('<i class="fa fa-spinner fa-spin"></i> Please wait...')
            .attr("disabled", "disabled");

        $.post("/Give/PaymentCompleted", null, function (result) {
            $("#kt-login__container").html('').html(result);
        });
    });

    $(document).on("click", "#change_type_amount", function () {
        $.post("/Give/GiftType", null, function (result) {
            $("#kt-login__container").html('').html(result);
        });
    });

    // Dashboard Scripts Here
    $(document).on("click", ".module-toggle", function () {
        $(this).closest("li").find("div").toggle();
    });

    $(document).on("click", ".jq_view_roles", function () {
        var $this = $(this);
        var $uid = $this.attr("data-id");
        var $name = $this.attr("data-name");
        $("#role_heading").text($name);
        $.post("/permissions/UserRoles", ({
            userid: $uid
        }), function (result) {
            $this.closest(".tab-pane").find(".permissions_container").html('').html(result);
        });
    });

    $(document).on("click", ".viewRolePopup", function () {
        var $id = $(this).attr("data-id");
        $.post("/permissions/AddEditRolePopup", ({
            RoleId: $id
        }), function (result) {
            $("#role-container").html('').html(result);
            $("#viewRolePopup").modal("show");
        });
    });

    $(document).on("click", ".addEditRole", function () {
        var $form = $(this).closest("#role-container");
        var $id = $form.find("#id").val();
        var $name = $form.find("#name").val();
        var $description = $form.find("#description").val();

        $.post("/permissions/AddEditRole", ({
            Id: $id,
            Name: $name,
            Description: $description
        }), function (result) {
            if (result.result === "success") {
                LoadRolesData();
                Notify("success", "Added", "Role added");
                $("#viewRolePopup").modal("hide");
            } else if (result.result === "updated") {
                LoadRolesData();
                Notify("brand", "Updated", "Role updated");
                $("#viewRolePopup").modal("hide");
            } else if (result.result === "exist") {
                Notify("warning", "Create failed", "This role already exists.");
            } else if (result.result === "exception") {
                Notify("danger", "Error", "Something went wrong");
            }
        });
    });

    $(document).on("click", ".assign-role", function (event) {
        var $this = $(this);
        var $url = "/permissions/" + $this.attr("data-action");

        if ($this.hasClass("active")) {
            $this.removeClass("active").attr("data-action", "AssignRoleToUser");
        } else {
            $this.addClass("active").attr("data-action", "RemoveRoleFromUser");
        }

        var $data = {
            userId: $this.attr("data-userId"),
            roleId: $this.attr("data-roleId")
        };

        $.post($url, $data, function (result) {
            //if (result.result == "success") {
            //    Notify("success", "Done", "Role has been assigned to the user");
            //}
            //else if (result.result == "removed") {
            //    Notify("warning", "Removed", "User role has been removed");
            //}
            //else if (result.result == "exception") {
            //    ShowErrorAlert(result.Message);
            //}
        });
    });

    $(document).on("click", ".load_permissions", function () {
        var $this = $(this);
        var $id = $this.attr("data-Id");
        var $parentId = $this.attr("data-parentId");
        var $typeId = $this.attr("data-typeId");
        var $type = $this.attr("data-type");

        $.post("/permissions/LoadPermissions", ({
            ModuleId: $id,
            Type: $type,
            TypeId: $typeId,
            ParentModuleId: $parentId
        }), function (result) {
            $this.closest(".tab-pane").find(".permissions_container").html('').html(result);
        });
    });

    $(document).on("click", ".switch_permissions", function () {
        var $this = $(this);
        var $remove = $this.prop("checked");
        var $url = $this.attr("src");
        $url = $url + "&Prop=" + $remove;
        $.post($url, null, function (result) {
            $this.parents('.permissions_container').html('').html(result);
        });
    });

    $(document).on("click", ".addEditModule", function () {
        var $form = $(this).closest("#module-container");
        var $id = $form.find("#id").val();
        var $name = $form.find("#name").val();
        var $parentId = $form.find("#parentId").val();

        $.post("/permissions/AddEditModule", ({
            Id: $id,
            Name: $name,
            ParentId: $parentId
        }), function (result) {
            if (result.result === "success") {
                LoadModuleData();
                Notify("success", "Added", "Module added");
            } else if (result.result === "updated") {
                LoadModuleData();
                Notify("brand", "Updated", "Module updated");
            } else if (result.result === "exist") {
                Notify("warning", "Create failed", "This module already exists.");
            } else if (result.result === "exception") {
                Notify("danger", "Error", "Something went wrong");
            }
        });
    });

    $(document).on("click", ".removeUserPermission", function (event) {
        event.preventDefault();
        var $this = $(this);
        var url = $this.attr("href");
        $.post(url, null, function (result) {
            $this.parents('.permissions_container').html('').html(result);
        });
    });

    $(document).on("click", ".change-campus", function (event) {
        event.preventDefault();
        var url = $(this).attr("href");
        var name = $(this).attr("data-name");
        $(this).closest(".dropdown").find("span").html(name);
        $.post(url, null, function () {
        });
    });

    $(document).on("change", "#kt-checkbox-selectall", function () {
        var $prop = $(this).prop("checked");
        $("input[name=Campuses]").prop("checked", $prop);
    });

    $(document).on("change", ".cbx_campus", function () {
        var $cbx = $(".cbx_campus");
        var checkedAll = true;
        $.each($cbx, function () {
            var $prop = $(this).prop("checked");
            if (!$prop) {
                checkedAll = false;
            }
        });
        $("#kt-checkbox-selectall").prop("checked", checkedAll);
    });

    // Attachments
    $(document).on("click", ".addEditAttachmentPopup", function () {
        $.get("/Attachments/_Create", ({
            type: "type",
            typeid: "123"
        }), function (result) {
            $("#att-container").html('').html(result);
            $("#addEditAttachmentPopup").modal("show");
        });
    });

    $(document).on("click", ".editAttachment", function (event) {
        event.preventDefault();
        var path = $(this).attr("href");
        $.get(path, function (result) {
            $("#att-container").html('').html(result);
            $("#addEditAttachmentPopup").modal("show");
        });
    });

    $(document).on("click", ".addMore", function () {
        $(".addMoreContainer").toggle(250);
    });

    // Notes
    $(document).on("click", ".addEditNotesPopup", function () {
        var $id = $(this).attr("data-id");
        $.get("/Attachments/_CreateNotes", ({
            Id: $id
        }), function (result) {
            $("#notes-container").html('').html(result);
            $("#addEditNotesPopup").modal("show");
        });
    });

    // Prayer Requests
    $(document).on("click", ".jq_load_pr_details, .jq_load_log_details", function (event) {
        event.preventDefault();
        var $url = $(this).attr("href");
        $.get($url, null, function (result) {
            $("#pr_details, #log_details").html("").html(result);
            $('#prayer-request-container, #log-container').hide();
            $('#prayer-request-details, #log-details').show();
            // history.replaceState({}, document.title, location.href);
        });
    });

    $(document).on("click", ".btn_pr_addEdit_model", function () {
        $.get("/PrayerRequests/_AddPrayerRequest", null, function (result) {
            $("#pr_model_body").html("").html(result);
            $("#pr_addEdit_model").modal("show");
        });
    });

    $(document).on("click", ".btn_pr_edit", function (event) {
        event.preventDefault();
        var $url = $(this).attr("href");
        $.get($url, null, function (result) {
            $("#pr_model_body").html("").html(result);
            $("#pr_addEdit_model").modal("show");
        });
    });

    // Tasks
    $(document).on("click", ".jq_load_tasks_details", function (event) {
        event.preventDefault();
        var $url = $(this).attr("href");
        $.get($url, null, function (result) {
            $("#tasks_details").html("").html(result);
            $('#task-details').show();
        });
    });

    // Giving -> Edit Methods
    $(document).on("click", ".btn-delete-method, .delete_prompt", function (event) {
        event.preventDefault();
        var $this = $(this);
        var $url = $this.attr("href");
        if ($this.attr("data-primary") === "True") {
            swal.fire("Uh-oh!", "You must specify another primary account before you can remove this one.", "error");
            return false;
        }

        swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to undo this!",
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, delete it!'
        }).then(function (result) {
            if (result.value) {
                $.post($url, null, function (result) {
                    if (result.response !== undefined) {
                        swal.fire("Stop!", result.response, "error");
                    } else {
                        if ($this.attr("data-row") == "remove") {
                            $this.closest("tr").remove();
                        }
                    }
                });
            }
        });
    });

    // Dashboard Template -> Clone
    $(document).on("click", ".btn-clone", function (event) {
        event.preventDefault();
        var $this = $(this);
        var $url = $this.attr("href");

        swal.fire({
            title: 'Are you sure?',
            text: 'You are about to make a copy of "' + $this.attr("data-name") + '"',
            type: 'info',
            showCancelButton: true,
            confirmButtonText: 'Yes, clone it!'
        }).then(function (result) {
            if (result.value) {
                $.post($url, null, function () { });
            }
        });
    });

    $(document).on("click", ".makeprimary", function (event) {
        event.preventDefault();

        var $this = $(this);
        var $url = $this.attr("href");

        swal.fire({
            title: 'Scheduled Giving',
            text: "Note: Do you want update your scheduled gifts to use this new primary account?",
            type: 'info',
            showCancelButton: true,
            confirmButtonText: 'Yes, Update All',
            cancelButtonText: 'No'
        }).then(function (result) {
            if (result.value) {
                $.get($url + "&updateScheduledPayments=true", null, function () { });
            } else {
                $.get($url + "&updateScheduledPayments=false", null, function () { });
            }
        });
    });

    //$(document).on("keyup", "#VerificationCode", function (e) {
    //    var $this = $(this);
    //    var phone = $("#Phone").val();
    //    if ($this.val().length === 6) {
    //        $this.attr("disabled", "disabled").attr("readonly", "readonly").css("background-color", "#ccc");

    //        $.get("/account/_VerifyPhoneCode", ({ phone: phone, code: $this.val() }), function (result) {
    //            if (result.result === "failed") {
    //                $this.removeAttr("disabled", "disabled").removeAttr("readonly", "readonly").css("background-color", "#fff");
    //                $("#Verification-Code-Message").text('Invalid code entered.');
    //            } else {
    //                var myDiv = $('#SignUp_Details');
    //                $.ajax({
    //                    url: '/Account/_EmailSignUp',
    //                    type: 'GET',
    //                    data: { status: "verified", phoneNumber: result.phoneNumber },
    //                    cache: false,
    //                    context: myDiv,
    //                    success: function (result) {
    //                        this.html(result);
    //                    }
    //                });
    //            }
    //        });
    //    }
    //    else {
    //        $("#Verification-Code-Message").text('Enter 4 digit code.');
    //    }
    //});

    // Add Giving Method
    $(document).on("click", "#submit-payment-method", function (event) {
        event.preventDefault();
        $(".form1").attr("method", "post").attr("action", "/MyGiving/PaymentMethods");

        var PaymentMethod = $('input[name="PaymentMethod"]:checked').val();

        if (PaymentMethod === "Card") {
            if (validateCCForm()) {
                $(".form1").submit();
            } else {
                $("html, body").animate({
                    scrollTop: 0
                }, 250);
            }
        } else if (PaymentMethod === "Check") {
            if (validateBAForm()) {
                $(".form1").submit();
            } else {
                $("html, body").animate({
                    scrollTop: 0
                }, 250);
            }
        }
    });

    // Give Now
    $(document).on("click", "#give-now-button", function (event) {
        event.preventDefault();

        if (validateGivingForm()) {
            $("form").submit();
        } else {
            $("html, body").animate({
                scrollTop: 0
            }, 250);
        }
    });

    function validateGivingForm() {
        $("#alert-container .alert-validation, #alert-container .alert-validation").remove();
        $("#check-info .form-control, #check-info .select2-selection").removeClass("form-control-error");

        var result = true;
        var errors = "";
        var Payment_Amount = $("#Payment_Amount");
        var Payment_FundId = $("#Payment_FundId");
        var Payment_CampusId = $("#Payment_CampusId");
        var Payment_PaymentMethod = $("#Payment_PaymentMethod");

        if (Payment_PaymentMethod.val() === "") {
            errors += "Please select a payment method.";
            Payment_PaymentMethod.addClass("form-control-error");
            result = false;
        }
        if (Payment_CampusId.val() === "") {
            errors += "Please select a campus.";
            Payment_CampusId.addClass("form-control-error");
            result = false;
        }
        if (Payment_FundId.val() === "") {
            errors += "<br>Please select a fund.";
            Payment_FundId.addClass("form-control-error");
            result = false;
        }
        if (Payment_Amount.val() === "") {
            errors += "<br>Please enter an amount.";
            Payment_Amount.addClass("form-control-error");
            result = false;
        }

        if (errors !== "") {
            ShowErrorAlert(errors);
        }

        return result;
    }

    function validateCCForm() {
        $("#alert-container .alert-validation").remove();
        $("#card-info .form-control, #card-info .select2-selection").removeClass("form-control-error");

        var result = true;
        var errors = "";
        var $cc_container = $("#card-info");
        var PaymentCard_CcName = $cc_container.find("#PaymentCard_CcName");
        var PaymentCard_CcNumber = $cc_container.find("#PaymentCard_CcNumber");
        var PaymentCard_CcType = $cc_container.find("#PaymentCard_CcType");
        var PaymentCard_CcExpMonth = $cc_container.find("#PaymentCard_CcExpMonth");
        var PaymentCard_CcExpYear = $cc_container.find("#PaymentCard_CcExpYear");

        if (PaymentCard_CcExpYear.val() === "") {
            errors += "Card expiration year is required";
            PaymentCard_CcExpYear.addClass("form-control-error");
            result = false;
        }
        if (PaymentCard_CcExpMonth.val() === "") {
            errors += "<br>Card expiration month is required";
            PaymentCard_CcExpMonth.addClass("form-control-error");
            result = false;
        }
        if (PaymentCard_CcNumber.val() === "") {
            errors += "<br>Card number is required";
            PaymentCard_CcNumber.addClass("form-control-error");
            result = false;
        }
        if (PaymentCard_CcType.val() === "") {
            errors += "<br>Card type is required";
            $("#card-info .select2-selection").addClass("form-control-error");
            result = false;
        }
        if (PaymentCard_CcName.val() === "") {
            errors += "<br>Card name is required";
            PaymentCard_CcName.addClass("form-control-error");
            result = false;
        }

        if (errors !== "") {
            ShowErrorAlert(errors);
        }

        return result;
    }

    function validateBAForm() {
        $("#alert-container .alert-validation, #alert-container .alert-validation").remove();
        $("#check-info .form-control, #check-info .select2-selection").removeClass("form-control-error");

        var result = true;
        var errors = "";
        var $cc_container = $("#check-info");
        var PaymentAccount_AccountType = $cc_container.find("#PaymentAccount_AccountType");
        var PaymentAccount_RoutingNumber = $cc_container.find("#PaymentAccount_RoutingNumber");
        var PaymentAccount_AccountNumber = $cc_container.find("#PaymentAccount_AccountNumber");

        if (PaymentAccount_AccountNumber.val() === "") {
            errors += "Account number is required";
            PaymentAccount_AccountNumber.addClass("form-control-error");
            result = false;
        }
        if (PaymentAccount_RoutingNumber.val() === "") {
            errors += "<br>Routing number is required";
            PaymentAccount_RoutingNumber.addClass("form-control-error");
            result = false;
        }
        if (PaymentAccount_AccountType.val() === "") {
            errors += "<br>Account type is required";
            $("#check-info .select2-selection").addClass("form-control-error");
            result = false;
        }

        if (errors !== "") {
            ShowErrorAlert(errors);
        }

        return result;
    }

    function ShowErrorAlert(message) {
        var html = '<div class="alert alert-custom alert-notice alert-light-danger fade show mb-5" role="alert">';
        html += '<div class="alert-icon"><i class="fas fa-exclamation-triangle"></i></div>';
        html += '<div class="alert-text">' + message + '</div>';
        html += '<div class="alert-close">';
        html += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
        html += '<span aria-hidden="true"><i class="ki ki-close"></i></span>';
        html += '</button>';
        html += '</div>';
        html += ' </div>';
        $("#alert-container").empty();
        $("#alert-container").prepend(html);
    }

    // Giving Payment
    function ValidateStartGiving() {
        $("#alert-container .alert-validation").remove();
        $("#Payment_Amount, .select2-selection").removeClass("form-control-error");

        var result = true;
        var errors = "";
        var $amount = $("#Amount");
        var $fund = $("#Payment_FundId");

        if ($amount.val() === "") {
            errors += "The amunt field is required <br />";
            $amount.addClass("form-control-error");
            result = false;
        }

        if ($fund.val() === "") {
            errors += "Please select fund";
            $(".select2-selection").addClass("form-control-error");
            result = false;
        }

        if (errors !== "") {
            ShowErrorAlert(errors);
        }

        return result;
    }

    $(document).on("click", ".btn_giving_submit", function () {
        if (ValidateStartGiving()) {
            $("#start-giving-form").submit();
        }
    });

    function validateEmail($email) {
        var emailReg = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/;
        return emailReg.test($email);
    }

    function validatePhone($phone) {

        value = $.trim($phone).replace(/\D/g, '');
        var emailReg = /\(?([0-9]{3})\)?([ .-]?)([0-9]{3})\2([0-9]{4})/;
        return emailReg.test(value);
    }

    function ValidateSignIn() {
        $("#alert-container .alert-validation").remove();
        $("#Phone, #Email, #VerificationCode, #login-form-password, #FirstName, #LastName, #Password, #ConfirmPassword, #VerificationCode, .select2-selection").removeClass("form-control-error");

        var result = true;
        var errors = "";
        var $loginVia = $("#LoginVia").val();
        var $responseStatus = $("#ResponseStatus").val();
        var $state = $("#LoginVia").val();
        //var url = window.location.pathname.split("/");
        //var controller = url[1];
        //var action = url[2];

        if ($responseStatus === "LoadLoginForm" && $loginVia === "Email") {
            let $Email = $("#Email");

            // Trim whitespace from email and password fields directly
            let trimmedEmail = $Email ? ($Email.val() ? $Email.val().trim() : "") : "";
            $Email.val(trimmedEmail);

            if (trimmedEmail === "") {
                errors += "The email field is required. <br />";
                $Email.addClass("form-control-error");
                result = false;
            } else {
                // Validate the trimmed email
                if (!validateEmail(trimmedEmail)) {
                    errors += "Please enter a valid email. <br />";
                    $Email.addClass("form-control-error");
                    result = false;
                }
            }
        } else if ($responseStatus === "LoadLoginForm" && $loginVia === "Phone") {
            let $Phone = $("#Phone");

            if ($Phone.val() === "") {
                errors += "The phone number field is required. <br />";
                $Phone.addClass("form-control-error");
                result = false;
            } else {
                if (!validatePhone($Phone.val())) {
                    errors += "Please enter a valid phone number. <br />";
                    $Phone.addClass("form-control-error");
                    result = false;
                }
            }
        } else if ($responseStatus === "SetupPassword" && $loginVia === "Email") {
            let $Password = $("#Password");
            let $ConfirmPassword = $("#ConfirmPassword");

            // Trim whitespace from password and confirm password fields directly
            let trimmedPassword = $Password ? ($Password.val() ? $Password.val().trim() : "") : "";
            let trimmedConfirmPassword = $ConfirmPassword ? ($ConfirmPassword.val() ? $ConfirmPassword.val().trim() : "") : "";

            if (trimmedPassword === "") {
                errors += "The password field is required. <br />";
                $Password.addClass("form-control-error");
                result = false;
            }

            if (trimmedConfirmPassword === "") {
                errors += "The confirm password field is required. <br />";
                $ConfirmPassword.addClass("form-control-error");
                result = false;
            }

            if (trimmedPassword !== trimmedConfirmPassword) {
                errors += "The password & confirm password values do not match. <br />";
                $Password.addClass("form-control-error");
                $ConfirmPassword.addClass("form-control-error");
                result = false;
            }
        } else if ($responseStatus === "SetupPassword" && $loginVia === "Phone") {
            let $Password = $("#Password");
            let $ConfirmPassword = $("#ConfirmPassword");
            let $Email = $("#Email");
            $Email.val(trimmedEmail);

            if ($Email.val() === "") {
                errors += "The email field is required. <br />";
                $Email.addClass("form-control-error");
                result = false;
            } else {
                // Validate the trimmed email
                let trimmedEmail = $Email ? ($Email.val() ? $Email.val().trim() : "") : "";
                if (!validateEmail(trimmedEmail)) {
                    errors += "Please enter a valid email. <br />";
                    $Email.addClass("form-control-error");
                    result = false;
                }
            }

            // Trim whitespace from password and confirm password fields directly
            let trimmedPassword = $Password ? ($Password.val() ? $Password.val().trim() : "") : "";
            let trimmedConfirmPassword = $ConfirmPassword ? ($ConfirmPassword.val() ? $ConfirmPassword.val().trim() : "") : "";

            if (trimmedPassword === "") {
                errors += "The password field is required. <br />";
                $Password.addClass("form-control-error");
                result = false;
            }

            if (trimmedConfirmPassword === "") {
                errors += "The confirm password field is required. <br />";
                $ConfirmPassword.addClass("form-control-error");
                result = false;
            }

            if (trimmedPassword !== trimmedConfirmPassword) {
                errors += "The password & confirm password values do not match. <br />";
                $Password.addClass("form-control-error");
                $ConfirmPassword.addClass("form-control-error");
                result = false;
            }
        } else if ($responseStatus === "LoadRegistrationForm") {            
            let $Email = $("#Email");
            let $Phone = $("#Phone");
            let $registerVia = $("#RegisterVia").val();

            // Trim whitespace from relevant fields            
            let trimmedEmail = $Email ? ($Email.val() ? $Email.val().trim() : "") : "";            
            let trimmedPhone = $Phone ? ($Phone.val() ? $Phone.val().trim() : "") : "";

            if ($registerVia === "Email") {
                if (trimmedEmail === "") {
                    errors += "The email field is required. <br />";
                    $Email.addClass("form-control-error");
                    result = false;
                }
            }

            if ($registerVia === "PhoneNumber") {
                if (trimmedPhone === "") {
                    errors += "The phone number field is required. <br />";
                    $Phone.addClass("form-control-error");
                    result = false;
                }
            }
        } else if ($responseStatus === "LoadRegistrationDetailsForm") {
            let $FirstName = $("#FirstName");
            let $LastName = $("#LastName");
            let $Email = $("#Email");
            let $Password = $("#Password");
            let $ConfirmPassword = $("#ConfirmPassword");
            let $Phone = $("#Phone");
            let $registerVia = $("#RegisterVia").val();

            // Trim whitespace from relevant fields
            let trimmedFirstName = $FirstName ? ($FirstName.val() ? $FirstName.val().trim() : "") : "";
            let trimmedLastName = $LastName ? ($LastName.val() ? $LastName.val().trim() : "") : "";
            let trimmedEmail = $Email ? ($Email.val() ? $Email.val().trim() : "") : "";
            let trimmedPassword = $Password ? ($Password.val() ? $Password.val().trim() : "") : "";
            let trimmedConfirmPassword = $ConfirmPassword ? ($ConfirmPassword.val() ? $ConfirmPassword.val().trim() : "") : "";
            let trimmedPhone = $Phone ? ($Phone.val() ? $Phone.val().trim() : "") : "";

            if (trimmedFirstName === "") {
                errors += "The first name field is required. <br />";
                $FirstName.addClass("form-control-error");
                result = false;
            }

            if (trimmedLastName === "") {
                errors += "The last name field is required. <br />";
                $LastName.addClass("form-control-error");
                result = false;
            }

            if ($registerVia === "Email") {
                if (trimmedEmail === "") {
                    errors += "The email field is required. <br />";
                    $Email.addClass("form-control-error");
                    result = false;
                }
                if (trimmedPassword === "") {
                    errors += "The password field is required. <br />";
                    $Password.addClass("form-control-error");
                    result = false;
                }
                if (trimmedConfirmPassword === "") {
                    errors += "The confirm password field is required. <br />";
                    $ConfirmPassword.addClass("form-control-error");
                    result = false;
                } else {
                    if (trimmedPassword !== trimmedConfirmPassword) {
                        errors += "Please verify the password fields match.";
                        $Password.addClass("form-control-error");
                        $ConfirmPassword.addClass("form-control-error");
                        result = false;
                    }
                }
            }

            if ($registerVia === "PhoneNumber") {
                if (trimmedPhone === "") {
                    errors += "The phone number field is required. <br />";
                    $Phone.addClass("form-control-error");
                    result = false;
                }
            }
        } else if ($state === "LoadConfirmationCode") {
            var $VerificationCode = $("#VerificationCode");

            // Trim whitespace from verification code field
            let trimmedVerificationCode = $VerificationCode ? ($VerificationCode.val() ? $VerificationCode.val().trim() : "") : "";

            if (trimmedVerificationCode === "") {
                errors += "The verification code is required. <br />";
                $VerificationCode.addClass("form-control-error");
                result = false;
            }
        }

        if (errors !== "") {
            ShowErrorAlert(errors);
        }

        return result;
    }

    // Function to handle form submission and show alerts
    function handleFormSubmission(e) {
        e.preventDefault(); // Prevent default action

        StartLoading();

        if (ValidateSignIn()) {
            $("#login-form").off('submit').submit(); // Unbind previous submit handlers and submit form
        } else {
            StopLoading();
        }
    }

    // Handle button click
    $(document).on("click", ".btn_continue", handleFormSubmission);

    // Handle Enter key press
    $(document).on('keypress', '#login-email input, #login-phone input', function (e) {
        if (e.which === 13) { // Enter key pressed
            handleFormSubmission(e);
        }
    });

    $(document).on('click', '.toggle', function (e) {
        e.preventDefault();
        $(this).parents('.card-header').next('.card-body').slideToggle("500", "linear");
        $(this).parents('.card-header').find('i.toggle-icon').toggleClass('fa-chevron-down fa-chevron-up');
    });

    $('.amount').on('input propertychange paste', function (e) {
        var reg = /^0+/gi;
        if (this.value.match(reg)) {
            this.value = this.value.replace(reg, '');
        }
    });

    $(document).on("change", ".taxid", function () {
        $(".invalid-feedback").hide();
        var taxId = $(".taxid").val();
        if (taxId !== null && taxId !== "") {
            if (taxId.indexOf('X') > -1) {
                $(".invalid-feedback").show();
            } else {
                $(".invalid-feedback").hide();
            }
        }
    });

    $(document).on("change", ".twoFactorAuth", function () {
        var $this = $(this);
        var $prop = $this.prop("checked");
        var $url = $this.attr("data-url");
        $.post($url, ({
            value: $prop
        }), function (resp) {
            if (resp.Success) {
                Notify("success", "SUCCESS", resp.Message);
            } else {
                ShowErrorAlert(result.Message);
            }
        });
    });

    $(document).on("click", ".btn_reset_password", function (e) {
        e.preventDefault();
        var $url = $(this).attr("data-href");
        swal.fire({
            title: 'Are you sure?',
            text: "You are about to reset another user's password. A notification will be sent to the user.",
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, reset it!'
        }).then(function (result) {
            if (result.value) {
                $.post($url, null, function () {
                });
            }
        });
    });
});

function editEvent(self) {
    $('#close-btn').trigger('click');
    StartLoading();
    if ($(self).data('global') === true) {
        let id = $(self).data('id');
        setTimeout(function () {
            $('#editModalGlobal-' + id).trigger('click');
            $('#btn-close').show();
            $('#btn-edit').hide();
        }, 500);
    } else {
        let href = $('#editModal').attr("href");
        StartLoading();
        setTimeout(function () {
            $('#editModal').attr("href", href);
            $('#editModal').trigger('click');
            $('#btn-close').show();
            $('#btn-edit').hide();
        }, 500);
    }
    setTimeout(function () {
        StopLoading();
    }, 600);
}

$(document).on('click', '.seeMore', function (e) {
    StartLoading();
    $('#category').val($(this).data('category'));
    $('#query').val($(this).data('query'));
    $('#searchResultForm').submit();
});

function createDate(hr, mnts, md) {
    if (md === 'PM' && parseInt(hr) !== 12) {
        hr = parseInt(hr) + 12;
    } else if (md === 'AM' && parseInt(hr) === 12) {
        hr = parseInt(hr) - 12;
    }
    return new Date(new Date().setHours(parseInt(hr), parseInt(mnts), 0));
}

function formatAMPM(date) {
    date = new Date(date);
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
}

function closeFilterModal() {
    $('#filter-modal').trigger('click');
}