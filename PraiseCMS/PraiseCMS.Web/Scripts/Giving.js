"use strict";
var page = 1;
var newUser = false;
var userSummary;
var showUserInfo = false;
var isValidCode = false;
var isValidUser = false;
var isValidCard = false;
var preventDoubleClick = false;
var isSecurityCodeConfirmed = false;
var goToStart = false;
var _phone;
var _userValidations = [];
// Class Definition
var KTLogin = function () {
    var _handleFormSignup = function () {
        // Base elements
        var wizardEl = KTUtil.getById('kt_login');
        var _formEl = KTUtil.getById('kt_login_signup_form');
        var wizardObj;
        var _validations = [];

        if (!_formEl) {
            return;
        }

        // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
        // Step 1
        _validations.push(FormValidation.formValidation(
            _formEl,
            {
                fields: {
                    'Amount': { validators: { notEmpty: { message: 'Please enter an amount.' } } },
                    'Payment.Frequency': { validators: { notEmpty: { message: 'Select a frequency' } } },
                    'ScheduledPayment.RecurringFrequency': {
                        validators: {
                            callback: {
                                message: 'Please select a recurring type.',
                                callback: function () {
                                    let frequency = _formEl.querySelector('[name="Payment.Frequency"]:checked').value;
                                    if (frequency === 'Recurring') {
                                        let rf = _formEl.querySelector('[name="ScheduledPayment.RecurringFrequency"]').value;
                                        return rf ? true : false;
                                    } else {
                                        return true;
                                    }

                                }
                            }
                        }
                    },
                    'ScheduledPayment.RecurringStartDate': {
                        validators: {
                            callback: {
                                message: 'Please select a starting date.',
                                callback: function () {
                                    let frequency = _formEl.querySelector('[name="Payment.Frequency"]:checked').value;
                                    if (frequency === 'Recurring') {
                                        let sd = _formEl.querySelector('[name="ScheduledPayment.RecurringStartDate"]').value;
                                        return sd ? true : false;
                                    } else {
                                        return true;
                                    }
                                }
                            }
                        }
                    },
                    'ScheduledPayment.GiftEndingReason': {
                        validators: {
                            callback: {
                                message: 'Please select an ending reason.',
                                callback: function () {
                                    let frequency = _formEl.querySelector('[name="Payment.Frequency"]:checked').value;
                                    if (frequency === 'Recurring') {
                                        let reason = _formEl.querySelector('[name="ScheduledPayment.GiftEndingReason"]').value;
                                        return reason ? true : false;
                                    } else {
                                        return true;
                                    }
                                }
                            }
                        }
                    },
                    'ScheduledPayment.RecurringEndDate': {
                        validators: {
                            callback: {
                                message: 'Please select an ending date.',
                                callback: function () {
                                    let frequency = _formEl.querySelector('[name="Payment.Frequency"]:checked').value;
                                    let reason = _formEl.querySelector('[name="ScheduledPayment.GiftEndingReason"]').value;
                                    if (frequency === 'Recurring' && reason === 'On a Specific Date') {
                                        let endingDate = _formEl.querySelector('[name="ScheduledPayment.RecurringEndDate"]').value;
                                        return endingDate ? true : false;
                                    } else {
                                        return true;
                                    }
                                }
                            }
                        }
                    },
                    'ScheduledPayment.MaxGifts': {
                        validators: {
                            callback: {
                                message: 'Please enter the maximum number of gifts.',
                                callback: function () {
                                    let frequency = _formEl.querySelector('[name="Payment.Frequency"]:checked').value;
                                    let reason = _formEl.querySelector('[name="ScheduledPayment.GiftEndingReason"]').value;
                                    if (frequency === 'Recurring' && reason === 'After Number of Gifts') {
                                        let maxGift = _formEl.querySelector('[name="ScheduledPayment.MaxGifts"]').value;
                                        return maxGift ? true : false;
                                    } else {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap: new FormValidation.plugins.Bootstrap({
                        eleValidClass: ''
                    })
                }
            }
        ));

        // Step 2
        if ($('#IsGuest').val() === "False") {
            _validations.push(FormValidation.formValidation(
                _formEl,
                {
                    fields: {
                        'Password': {
                            validators: {
                                notEmpty: { message: 'Please enter a password.' }
                            }
                        },
                        'Email': {
                            validators: {
                                notEmpty: { message: 'Please enter an email address.' },
                                emailAddress: { message: 'Please enter a valid email address.' }
                            }
                        }
                    },
                    plugins: {
                        trigger: new FormValidation.plugins.Trigger(),
                        // Bootstrap Framework Integration
                        bootstrap: new FormValidation.plugins.Bootstrap({
                            eleValidClass: ''
                        })
                    }
                }
            ));
        } else {
            _validations.push(FormValidation.formValidation(
                _formEl,
                {
                    fields: { Phone: { validators: { notEmpty: { message: 'Please enter a phone number.' } } } }, plugins: {
                        trigger: new FormValidation.plugins.Trigger(),
                        // Bootstrap Framework Integration
                        bootstrap: new FormValidation.plugins.Bootstrap({
                            //eleInvalidClass: '',
                            eleValidClass: ''
                        })
                    }
                }
            ));
        }

        // Step 3
        _validations.push(FormValidation.formValidation(
            _formEl,
            {
                fields: {
                    'Payment.PaymentMethod': { validators: { notEmpty: { message: 'Please enter a payment method.' } } }
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap: new FormValidation.plugins.Bootstrap({
                        eleValidClass: ''
                    })
                }
            }
        ));

        // Initialize form wizard
        wizardObj = new KTWizard(wizardEl, {
            startStep: 1, // initial active step number
            clickableSteps: false // to make steps clickable this set value true and add data-wizard-clickable="true" in HTML for class="wizard" element
        });

        // Validation before going to next page
        wizardObj.on('beforeNext', function (wizard) {
            if (preventDoubleClick && wizard.getStep() === 3) {
                return false;
            }
            if (wizard.getStep() > wizard.getNewStep()) {
                return; // Skip if stepped back
            }

            // Validate form before change wizard step
            var validator = _validations[wizard.getStep() - 1]; // get validator for current step

            if (validator) {
                validator.validate().then(function (status) {
                    if (status === 'Valid') {
                        let currentPage = wizard.getStep();
                        if (wizard.getStep() === 1 && goToStart) {
                            goToStart = false;
                            wizard.goTo(currentPage + 2);
                            //togglePhoneBlock();
                            return false;
                        }
                        if (wizard.getStep() === 2 && $('#phoneNumber_div').hasClass('active') && $('#IsGuest').val() === "True") {
                            if (!$('#Phone').val() || parseInt($('#Phone').val().replace(/\D/g, "").length) < 10) {
                                wizardValidationMessage("Please enter a valid phone number.");
                                return false;
                            }
                            if (_phone && isSecurityCodeConfirmed && _phone === $('#Phone').val()) {
                                page = 3;
                                calculateFees();
                                wizard.goTo(page);//go to payment methods page(Step 3), skip confirmation code                             
                                return false;
                            } else {
                                sendSecurityCode();
                            }
                            return;
                        } else if (wizard.getStep() === 2 && $('#securityCode_div').hasClass('active') && $('#IsGuest').val() === "True") {
                            if ($('#VerificationCode').val()) {
                                ConfirmSecurityCode();
                                if (!isValidCode) {
                                    wizardValidationMessage("Invalid Code. The code used is either incorrect or has expired. Please click the Resend Code link to request a new code.");
                                    StopLoading();
                                    return false;
                                }
                            } else {
                                wizardValidationMessage();
                                return false;
                            }
                        } else if (wizard.getStep() === 2 && $('#IsGuest').val() === "False") {
                            Login();
                            if (!isValidUser) {
                                return false;
                            }
                        }
                        else if (wizard.getStep() === 3) {
                            getSummary();
                        }
                        if (wizard.getStep() === 2) {
                            calculateFees();
                        }
                        wizard.goTo(wizard.getNewStep());
                        page = currentPage + 1;
                        if (page === 4) {
                            $("#form_submit_button").focus();
                        }

                        KTUtil.scrollTop();
                    } else {
                        wizardValidationMessage();
                    }
                });
            }

            return false;  // Do not change wizard step, further action will be handled by the validator
        });

        wizardObj.on('beforePrev', function (wizard) {
            if (goToStart) {
                page = 1;
                wizard.goTo(page);
                return false;
            }
            if (wizard.getStep() === 3 && isSecurityCodeConfirmed) {
                wizard.goTo(wizard.getStep() - 1);
                togglePhoneBlock();
                return false;
            }
            else if (wizard.getStep() === 2 && $('#securityCode_div').hasClass('active')) {
                togglePhoneBlock();
                return false;
            } else if (wizard.getStep() === 3 && $('#newPaymentMethod').hasClass('active')) {
                addPaymentMethod();
                return false;
            }
            page = wizard.getStep() - 1;
        });

        // Change event
        wizardObj.on('change', function (wizard) {
            KTUtil.scrollTop();
        });
    };

    // Public Functions
    return {
        init: function () {
            _handleFormSignup();
        }
    };
}();

function userValidation() {
    const form = document.getElementById('kt_login_signup_form');
    _userValidations.push(FormValidation.formValidation(
        form,
        {
            fields: {
                'firstName': { validators: { notEmpty: { message: 'Please enter a first name.' } } },
                'lastName': { validators: { notEmpty: { message: 'Please enter a last name.' } } },
                'Address1': { validators: { notEmpty: { message: 'Please enter an address.' } } },
                'City': { validators: { notEmpty: { message: 'Please enter a city.' } } },
                'State': { validators: { notEmpty: { message: 'Please select a state.' } } },
                'Zip': { validators: { notEmpty: { message: 'Please enter a zip code.' } } },
                'Email': {
                    validators: {
                        notEmpty: { message: 'Please enter your email address.' },
                        emailAddress: { message: 'Please enter a valid email address.' }
                    }
                }
            }, plugins: {
                trigger: new FormValidation.plugins.Trigger,
                bootstrap: new FormValidation.plugins.Bootstrap({ eleValidClass: "" })
            }
        }));
    _userValidations.push(FormValidation.formValidation(
        form,
        {
            fields: {
                'paymentMethod': { validators: { notEmpty: { message: 'Please select a payment method.' } } },
                'cardNumber': {
                    validators: {
                        callback: {
                            message: 'Please enter the card number.',
                            callback: function (input) {
                                let peymentMethod = form.querySelector('[name="paymentMethod"]:checked').value;
                                if (peymentMethod === 'Card') {
                                    return input.value ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'expiryDate': {
                    validators: {
                        callback: {
                            message: 'Please enter the expiration date.',
                            callback: function (input) {
                                let peymentMethod = form.querySelector('[name="paymentMethod"]:checked').value;
                                if (peymentMethod === 'Card') {
                                    return input.value ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'ccName': {
                    validators: {
                        callback: {
                            message: 'Please enter the name as it appears on the card.',
                            callback: function (input) {
                                let peymentMethod = form.querySelector('[name="paymentMethod"]:checked').value;
                                if (peymentMethod === 'Card') {
                                    return input.value ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'accountNumber': {
                    validators: {
                        callback: {
                            message: 'Please enter your account number.',
                            callback: function (input) {
                                let peymentMethod = form.querySelector('[name="paymentMethod"]:checked').value;
                                if (peymentMethod === 'ACH') {
                                    return input.value ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'accountType': {
                    validators: {
                        callback: {
                            message: 'Please select a bank account type.',
                            callback: function (input) {
                                let peymentMethod = form.querySelector('[name="paymentMethod"]:checked').value;
                                if (peymentMethod === 'ACH') {
                                    return input.value ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'PaymentAccount_RoutingNumber': {
                    validators: {
                        callback: {
                            message: 'Please enter your routing number.',
                            callback: function (input) {
                                let peymentMethod = form.querySelector('[name="paymentMethod"]:checked').value;
                                if (peymentMethod === 'ACH') {
                                    return input.value ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }, plugins: {
                trigger: new FormValidation.plugins.Trigger,
                bootstrap: new FormValidation.plugins.Bootstrap({ eleValidClass: "" })
            }
        }));
}

//Enter key
$('#kt_login_signup_form').on('keyup keypress', function (e) {
    var keyCode = e.keyCode || e.which;
    if (keyCode === 13) {
        if (page === 1) {
            $('#next-step').trigger('click');
        } else if (page === 2 && $('#phoneNumber_div').hasClass('active') && $('#IsGuest').val() === "True") {
            $('#next-step').trigger('click');
        } else if (page === 2 && $('#securityCode_div').hasClass('active') && $('#IsGuest').val() === "True") {
            preventDoubleClick = true;
            $('#next-step').trigger('click');
        } else if (page === 2 && $('#IsGuest').val() === "False") {
            preventDoubleClick = true;
            $('#next-step').trigger('click');
        } else if (page === 3 && !$('#newPaymentMethod').hasClass('active')) {
            $('#next-step').click();
        } else if (page === 3 && $('#newPaymentMethod').hasClass('active')) {
            $('#save-paymnet-method').click();
        } else if (page === 4) {
            $('#form_submit_button').click();
        }
        e.preventDefault();
        return false;
    }
});

$(document).on('click', '#form_submit_button', function () {
    $.ajax({
        type: "POST",
        url: '/GivingWorkflow/StartGiving',
        data: $("#kt_login_signup_form").serialize(), // serializes the form's elements.
        success: function (result) {
            if (result.Success) {
                document.getElementById("kt_login_signup_form").reset();
                const origin = window.location.origin;
                const url = origin.concat('/GivingWorkflow/Completed?churchId=', result.Church.Id, '&fundId=', result.FundId, '&guest=', result.Guest, '&campusName=', result.CampusName, '&amount=', result.PaymentAmount, '&processingFee=', result.ProcessingFee, '&paymentOccurrence=', result.PaymentOccurrence);
                window.open(url, "_self");
            } else {
                ShowErrorAlert(result.Message);
            }
        },
        error: function (result) {
            ShowErrorAlert(result.Message);
        }
    });
});

$(document).ready(function () {
    if (window.performance && window.performance.navigation.type === window.performance.navigation.TYPE_BACK_FORWARD) {
        location.reload();
    }

    $('#Amount').focus();

    if ($('#Phone').val()) {
        _phone = $('#Phone').val();
        isSecurityCodeConfirmed = true;
        goToStart = true;
    }
    if ($("input[id='Payment_Frequency']:checked").val() === 'Recurring') {
        $('#recurringInfo').show();
    }
});

// Class Initialization
jQuery(document).ready(function () {
    KTLogin.init();
    userValidation();
});

function togglePhoneBlock() {
    if ($('#securityCode_div').hasClass('active')) {
        $('#securityCode_div').removeClass('active');
        $('#securityCode_div').hide();
        $('#phoneNumber_div').addClass('active');
        $('#phoneNumber_div').show();
        $('#spn_phone').text('');
        $('#VerificationCode').val('');
    }
}

function sendSecurityCode() {
    sendCode();
    if ($('#phoneNumber_div').hasClass('active')) {
        $('#phoneNumber_div').removeClass('active');
        $('#phoneNumber_div').hide();
        $('#securityCode_div').addClass('active');
        $('#securityCode_div').show();
        if ($('#Phone').val()) {
            $('#spn_phone').text($('#Phone').val());
        }
    }
}

$('input:radio[name="paymentMethod"]').change(function () {
    if (this.checked && this.value === "Card") {
        $('#bankInfo').hide();
        $('#cardInfo').show();
    }
    if (this.checked && this.value === "ACH") {
        $('#cardInfo').hide();
        $('#bankInfo').show();
    }
});

$(document).on('change', '#Payment_Frequency', function () {
    if ($(this).val() === 'Recurring') {
        $('#recurringInfo').show();
    } else {
        $('#recurringInfo').hide();
    }
});

function addPaymentMethod() {
    const newPaymentMethod = $('#newPaymentMethod');
    const listPaymentMethods = $('#listPaymentMethods');
    const footerBtn = $('#footerBtn');
    const paymentMethodSubmit = $('#paymentMethodSubmit');

    if (newPaymentMethod.hasClass('active')) {
        newPaymentMethod.removeClass('active').hide();
        listPaymentMethods.show();
        footerBtn.removeClass('hideDiv');
        paymentMethodSubmit.addClass('hideDiv');
    } else {
        resetNewPaymentMethod();
        newPaymentMethod.addClass('active').show();
        listPaymentMethods.hide();
        footerBtn.addClass('hideDiv');
        paymentMethodSubmit.removeClass('hideDiv');
    }
}

function resetNewPaymentMethod() {
    const newPaymentMethod = $('#newPaymentMethod');

    newPaymentMethod.find('input:text').val('');
    newPaymentMethod.find('input[type="radio"]').prop('checked', false);

    $("input[name='paymentMethod']:first, input[name='accountType']:first").prop("checked", true).trigger("change");

    // Clear card number validation
    cardValidation(true);

    // Clear routing number validation
    const routingNumber = $('#PaymentAccount_RoutingNumber');
    routingNumber.removeClass('is-invalid is-valid');
    $('.routing-feedback').removeClass('valid-feedback invalid-feedback').text('');
}

$(document).on('change', '#ScheduledPayment_GiftEndingReason', function () {
    const value = $(this).val();
    $('#maxGift').toggle(value === 'After Number of Gifts');
    $('#endingDate').toggle(value === 'On a Specific Date');
});


function ConfirmSecurityCode() {
    isValidCode = false;
    if ($('#VerificationCode').val()) {
        //async false must be need here in ajax call
        $.ajax({
            url: '/GivingWorkflow/VerifyCode',
            type: 'POST',
            dataType: 'json',
            async: false,
            data: { Phone: $('#Phone').val(), Code: $('#VerificationCode').val() },
            success: function (result) {
                //On success
                if (result.Success) {
                    //console.log(result.Message);
                    setupPaymentMethod(result);
                    if ($('#AccountFound').val() === "false") {
                        addPaymentMethod();
                    }
                    if (result.NewUser) {
                        newUser = true;
                        $('#user-name').show();
                        $('#user-name-heading').show();
                        $('#payment-method-heading').hide();
                    } else {
                        newUser = false;
                        $('#user-name').hide();
                        $('#user-name-heading').hide();
                        $('#payment-method-heading').show();
                    }
                    setTimeout(function () {
                        preventDoubleClick = false;
                    }, 1000);
                    isSecurityCodeConfirmed = isValidCode = true;

                } else {
                    //console.log(result.Message);
                    isValidCode = false;
                }
            },
            error: function (result) {
                //console.log(result.Message);
                isValidCode = false;
            }
        });
    }
}

function sendCode(isResend = false) {
    isSecurityCodeConfirmed = false;
    StartLoading();
    //ajax call for generate and send verification code
    if (!$('#Phone').val() || parseInt($('#Phone').val().replace(/\D/g, "").length) < 10) {
        wizardValidationMessage("Please enter a valid phone number.");
        StopLoading();
        return false;
    }

    let Church = {
        Id: $('#Church_Id').val()
    };
    let Campus = {
        Id: $('#Payment_CampusId').val()
    };
    let model = {
        Phone: $('#Phone').val(),
        Church: Church,
        Campus: Campus
    };
    _phone = model.Phone;
    $.ajax({
        url: '/GivingWorkflow/PhoneVerification',
        type: 'POST',
        dataType: 'json',
        data: model,
        success: function (result) {
            StopLoading();
            //On success
            if (result.Success) {
                if ($('#VerificationCode').val().length > 3) {
                    $('#VerificationCode').addClass('is-valid');
                } else {
                    $('#VerificationCode').removeClass('is-valid');
                }
                //temporary code start
                //console.log("Here is the code: " + result.Key);
                var dummy = document.createElement("input");
                document.body.appendChild(dummy);
                dummy.setAttribute("id", "dummy_id");
                document.getElementById("dummy_id").value = result.Key;
                dummy.select();
                document.execCommand("copy");
                document.body.removeChild(dummy);
                //temporary code end
                if (isResend) {
                    $(".resend-code-msg").text('A new code has been sent to your phone.');
                    $(".resend-code-msg").fadeIn(1000);
                    setTimeout(function () {
                        $(".resend-code-msg").fadeOut(3000);
                        setTimeout(function () {
                            $(".resend-code-msg").text('');
                        }, 3000);
                    }, 2500);
                }
            } else {
                //console.log(result.Message);
            }
        },
        error: function (result) {
            StopLoading();
            //console.log(result);
        }
    });
}

function saveNewPaymentMethod() {
    let paymentCard = {
        CcNumber: $('#cardNumber').val(),
        CcExpiry: $('#expiryDate').val(),
        CcType: $('#cardType').text(),
        NickName: $('#nickName').val(),
        CcName: $('#ccName').val()
    };
    let paymentAccount = {
        AccountNumber: $('#accountNumber').val(),
        RoutingNumber: $('#PaymentAccount_RoutingNumber').val(),
        AccountType: $("input[name='accountType']:checked").val(),
        NickName: $('#nickName').val()
    };
    let user = {
        PhoneNumber: $('#Phone').val(),
        PhoneVerificationCode: $('#VerificationCode').val(),
        Zip: $('#Zip').val(),
        FirstName: $('#firstName').val(),
        LastName: $('#lastName').val(),
        Address1: $('#Address1').val(),
        Address2: $('#Address2').val(),
        City: $('#City').val(),
        State: $('#State').val(),
        Email: $('#Email').val()
    };
    let model = {
        PaymentMethod: $("input[name='paymentMethod']:checked").val(),
        PaymentCard: paymentCard,
        PaymentAccount: paymentAccount,
        User: user,
        DonorGUID: $('#DonorGUID').val(),
        churchId: $('#Church_Id').val(),
    };

    _userValidations[1].validate();
    if (newUser) {
        _userValidations[0].validate();

        if (!user.FirstName || !user.LastName || !user.Address1 || !user.City || !user.State || !user.Zip || !user.Email) {
            wizardValidationMessage("Please fill out all of the required fields.");
            return false;
        }
        showUserInfo = true;
        userSummary = user;
    }
    if (model.PaymentMethod === "ACH") {
        if (!paymentAccount.AccountNumber || !paymentAccount.RoutingNumber || !paymentAccount.AccountType) {
            wizardValidationMessage();
            return false;
        }
    } else if (model.PaymentMethod === "Card") {
        if (!paymentCard.CcNumber || !paymentCard.CcExpiry || !paymentCard.CcType || !paymentCard.CcName) {
            wizardValidationMessage();
            return false;
        }
        if (!isValidCard || !paymentCard.CcType || (paymentCard.CcType === "AMEX" && paymentCard.CcNumber.length < 18) || (paymentCard.CcType !== "AMEX" && paymentCard.CcNumber.length < 19)) {
            cardValidation(false);
            return false;
        }
    }

    $.ajax({
        url: '/GivingWorkflow/AddNewPaymentMethods',
        type: 'POST',
        dataType: 'json',
        data: model,
        success: function (response) {
            if (response.Success) {
                newUser = false;
                $('#user-name').hide();
                $('#user-name-heading').hide();
                $('#payment-method-heading').show();
                setupPaymentMethod(response);
                if (response.PaymentMethods.length > 1) {
                    Swal.fire({
                        text: "Your new payment method is ready to be used.",
                        icon: "success",
                        buttonsStyling: false,
                        confirmButtonText: "Ok, got it!",
                        customClass: {
                            confirmButton: "btn font-weight-bold btn-light"
                        }
                    }).then(function () {
                        addPaymentMethod();
                    });
                } else {
                    addPaymentMethod();
                }
            } else {
                if (response.DonorId) {
                    $('#DonorGUID').val(response.DonorId);
                }
                if (response.Message === 'Invalid Card Number') {
                    response.Message = "Please enter a valid card number and try again.";
                } else if (response.Message.includes('routing')) {
                    response.Message = "Please enter a valid routing number and try again.";
                } else {
                    $("#alert-container").prepend("");
                    let html = "<div class='alert alert-custom alert-notice alert-light-danger fade show mb-5' role='alert'><div class='alert-icon'><i class='fas fa-exclamation-triangle'></i></div><div class='alert-text'><strong>Something went wrong. Please try again.</strong><br />Error: " + response.Message + "</div><div class='alert-close'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'><i class='ki ki-close'></i></span></button></div></div>";
                    $("#alert-container").prepend(html);
                    response.Message = "Uh-oh! It looks like there was a problem adding your payment method. Please try again.";
                }
                wizardValidationMessage(response.Message, "warning");
            }
        },
        error: function (response) {
            //console.log(result);
        }
    });
}

function calculateFees() {
    if ($('#Payment_PaymentMethod').val()) {
        calculateProcessingFee($('#Church_Id').val(), $('#Amount').val(), $('#Payment_PaymentMethod').val());
    } else {
        $('#processingFee-div').hide();
        $('#processingFeeAmount').text("$0.00");
    }
}

function getSummary() {
    var campus = campuses.filter(x => x.Value === $('#Payment_CampusId').val());
    var fund = funds.filter(x => x.Value === $('#Payment_FundId').val());
    var paymentMethod = accounts.filter(x => x.Value === $('#Payment_PaymentMethod').val());
    let amount = $('#Amount').val();
    $('#summary-frequency').text($("input[name='Payment.Frequency']:checked").val());
    $('#summary-amount').text("$" + amount);
    if (paymentMethod && paymentMethod.length) {
        $('#summary-payment-method').text(paymentMethod[0].Text);
    } else {
        $('#summary-payment-method').text("");
    }
    if (fund && fund.length) {
        $('#summary-fund').text(fund[0].Text);
    } else {
        $('#summary-fund').text("");
    }
    if (campus && campus.length) {
        $('#summary-campus').text(campus[0].Text);
    } else {
        $('#summary-campus').text("");
    }
    if ($("input[name='Payment.Frequency']:checked").val() === 'Recurring') {
        $('#recurring-parameters').show();
        $('#summary-recurring-frequency').text($('#ScheduledPayment_RecurringFrequency').val());
        $('#summary-starting').text($('#ScheduledPayment_RecurringStartDate').val());
        $('#summary-ending-reason').text($('#ScheduledPayment_GiftEndingReason').val());
        if ($('#ScheduledPayment_GiftEndingReason').val() === 'On a Specific Date') {
            $('#summry-endingDate-div').show();
            $('#summry-maxGift-div').hide();
            //set value
            $('#summary-endingDate').text($('#ScheduledPayment_RecurringEndDate').val());
            $('#summary-maxgifts').text('');
        } else if ($('#ScheduledPayment_GiftEndingReason').val() === 'After Number of Gifts') {
            $('#summry-maxGift-div').show();
            $('#summry-endingDate-div').hide();
            //set value
            $('#summary-maxgifts').text($('#ScheduledPayment_MaxGifts').val());
            $('#summary-endingDate').text('');
        } else {
            $('#summry-maxGift-div').hide();
            $('#summry-endingDate-div').hide();
            //set value
            $('#summary-maxgifts').text('');
            $('#summary-endingDate').text('');
        }
    } else {
        $('#recurring-parameters').hide();
    }
    if (showUserInfo) {
        $('#userInfo').show();

        $('#user-fullName').text(userSummary.FirstName + " " + userSummary.LastName);
        $('#user-email').text(userSummary.Email);
        $('#user-address1').text(userSummary.Address1);
        $('#user-address2').text(userSummary.Address2 ? userSummary.Address2 : "");
        $('#user-city').text(userSummary.City);
        $('#user-state').text(userSummary.State);
        $('#user-zip').text(userSummary.Zip);
    }
}

function setupPaymentMethod(response) {
    let _paymentMethods = response.PaymentMethods;
    if (_paymentMethods.length) {
        $('#AccountFound').val(true);
    }
    else {
        $('#AccountFound').val(false);
    }
    let _donorGUID = response.DonorId;
    $('#DonorGUID').val(_donorGUID);

    $('#Payment_PaymentMethod').html('');
    $('#Payment_PaymentMethod').html('<option value="">Select a payment method...</option>');
    if (_paymentMethods.length) {
        for (let i = 0; i < _paymentMethods.length; i++) {
            if (_paymentMethods[i].IsPrimary) {
                $('#Payment_PaymentMethod').append('<option selected="selected"  data-content="<span class=\'primary-card ml-2\'>(Primary)</span>" value="' + _paymentMethods[i].Value + '"><span>' + _paymentMethods[i].Text + '</span></option > ');

            } else {
                $('#Payment_PaymentMethod').append('<option value="' + _paymentMethods[i].Value + '"><span>' + _paymentMethods[i].Text + '</span></option > ');
            }
        }
        let primary = _paymentMethods.filter(x => x.IsPrimary === true);
        $('#Payment_PaymentMethod').val(primary[0].Value); // Set the default values here.
        calculateFees();
        // Not using accounts variable in html so i have commented this.
        accounts = _paymentMethods;
        InitSelect2();
    }
}

$(document).on("keyup", "#VerificationCode", function (e) {
    let str = e.currentTarget.value;
    if (str.length > 3) {
        $('#VerificationCode').addClass('is-valid');
    } else {
        $('#VerificationCode').removeClass('is-valid');
    }
});

function Login() {
    let model = {
        Password: $('#Password').val(),
        Email: $('#Email').val()
    };
    $.ajax({
        url: '/Account/UserLogin',
        type: 'POST',
        dataType: 'json',
        async: false,
        data: model,
        success: function (response) {
            if (response.Success) {
                isValidUser = true;
                setupPaymentMethod(response);
                if ($('#AccountFound').val() === "false") {
                    addPaymentMethod();
                }
                if (response.NewUser) {
                    newUser = true;
                    $('#user-name').show();
                    $('#user-name-heading').show();
                    $('#payment-method-heading').hide();
                } else {
                    newUser = false;
                    $('#user-name').hide();
                    $('#user-name-heading').hide();
                    $('#payment-method-heading').show();
                }
                setTimeout(function () {
                    preventDoubleClick = false;
                }, 1000);
            } else {
                wizardValidationMessage(response.Message, "warning");
                isValidUser = false;
            }
        },
        error: function (response) {
            //console.log(result);
        }
    });
}

function changeDetails() {
    let backBtn = document.querySelector('[data-wizard-type="action-prev"]');
    if (backBtn) {
        goToStart = true;
        $(backBtn).click();
    }
}