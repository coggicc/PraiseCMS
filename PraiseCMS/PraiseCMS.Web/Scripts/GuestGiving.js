"use strict";
var _formValidation;
var _GuestGiving = function () {
    var _guestGiving = function () {
        // Base elements
        let wizardEl = KTUtil.getById('kt_Guest_Giving');
        let _formEl = KTUtil.getById('kt_Guest_Giving_form');
        let wizardObj;
        let _validations = [];

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
                    'Email': {
                        validators: {
                            //notEmpty: { message: "Email is required" },
                            emailAddress: { message: 'Please enter a valid email address' },
                            callback: {
                                message: 'Please enter an email or cell phone number.',
                                callback: function () {
                                    let phone = _formEl.querySelector('[name="Phone"]').value;
                                    if (!phone) {
                                        let email = _formEl.querySelector('[name="Email"]').value;
                                        return email ? true : false;
                                    } else {
                                        return true;
                                    }
                                }
                            }
                        }
                    },
                    'Phone': {
                        validators: {
                            callback: {
                                message: 'Please enter an email or cell phone number.',
                                callback: function () {
                                    let email = _formEl.querySelector('[name="Email"]').value;
                                    if (!email) {
                                        const Phone = _formEl.querySelector('[name="Phone"]').value;
                                        if (Phone) {
                                            const length = parseInt(Phone.replace(/\D/g, "").length);
                                            return (length === 10) ? true : false;
                                        } else {
                                            return false;
                                        }
                                    }
                                    else {
                                        return true;
                                    }
                                }
                            }
                        }
                    },
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap: new FormValidation.plugins.Bootstrap({
                        eleValidClass: '',
                    })
                }
            }
        ));
        // Step 2
        _validations.push(FormValidation.formValidation(
            _formEl,
            {
                fields: {
                    'PaymentCard_CcNumber': { validators: { notEmpty: { message: 'Please enter a card number.' } } },
                    'PaymentCard_CcExpiry': { validators: { notEmpty: { message: 'Please enter the expiration date.' } } },
                    'PaymentCard_CcCvc': { validators: { notEmpty: { message: 'Please enter the card security code.' } } },
                    'FirstName': { validators: { notEmpty: { message: 'Please enter your first name.' } } },
                    'LastName': { validators: { notEmpty: { message: 'Please enter your last name.' } } },
                    'Zip': { validators: { notEmpty: { message: 'Please enter the card zip code.' } } }
                }, plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    // Bootstrap Framework Integration
                    bootstrap: new FormValidation.plugins.Bootstrap({
                        eleValidClass: '',
                    })
                }
            }
        ));
        _formValidation = _validations;

        // Initialize form wizard
        wizardObj = new KTWizard(wizardEl, {
            startStep: 1, // initial active step number
            clickableSteps: false // to make steps clickable this set value true and add data-wizard-clickable="true" in HTML for class="wizard" element
        });

        // Validation before going to next page
        //_wizardObj.on('beforeChange', function (wizard) {
        wizardObj.on('beforeNext', function (wizard) {
            if (wizard.getStep() > wizard.getNewStep()) {
                return; // Skip if stepped back
            }

            // Validate form before change wizard step
            var validator = _validations[wizard.getStep() - 1]; // get validator for currnt step

            if (validator) {
                validator.validate().then(function (status) {
                    if (status === 'Valid') {
                        if (!$('#Phone').val() && !$('#Email').val()) {
                            wizardValidationMessage("Please enter a cell phone number or an email.", "warning");
                            return false;
                        }
                        if ($('#Phone').val() && parseInt($('#Phone').val().replace(/\D/g, "").length) < 10) {
                            wizardValidationMessage("Enter a valid phone number.", "warning");
                            return false;
                        }
                        getDetails();
                        wizard.goTo(wizard.getNewStep());
                        KTUtil.scrollTop();
                    } else {
                        wizardValidationMessage();
                    }
                });
            }
            return false;  // Do not change wizard step, further action will be handled by he validator
        });

        // Change event
        wizardObj.on('change', function (wizard) {
            KTUtil.scrollTop();
        });
    };

    // Public Functions
    return {
        init: function () {
            _guestGiving();
        }
    };
}();

// Class Initialization
jQuery(document).ready(function () {
    _GuestGiving.init();
    if (window.performance && window.performance.navigation.type === window.performance.navigation.TYPE_BACK_FORWARD) {
        location.reload();
    }
    $('#Amount').focus();
});

function validateForm(self) {
    $(self).removeClass('is-invalid');
    var msg = $(self).parent('.form-group').children('.fv-plugins-message-container');
    msg.html('');
}
$('#kt_Guest_Giving_form').on('keyup keypress', function (e) {
    var keyCode = e.keyCode || e.which;
    if (keyCode === 13) {
        e.preventDefault();
        return false;
    }
});

$(document).on('click', '#giving_form_submit', function (event) {
    let validator = _formValidation[1];
    event.preventDefault();
    if (validator) {
        validator.validate().then(function (status) {
            if (status === 'Valid') {
                var form = document.getElementById("kt_Guest_Giving_form");
                if (form) {
                    form.submit();
                    setTimeout(function () {
                        form.reset();
                    });
                }
            } else {
                wizardValidationMessage();
                return false;
            }
        });
    }

});

function getDetails() {
    calculateProcessingFee($('#Church_Id').val(), $('#Amount').val(), "");
    var campus = campuses.filter(x => x.Value === $('#CampusId').val());
    var fund = funds.filter(x => x.Value === $('#FundId').val());
    $('#summary-amount').text("$" + $('#Amount').val());
    if (fund && fund.length) {
        $('#summary-fund').text(fund[0].Text);
    }
    else {
        $('#summary-fund').text("");
    }
    if (campus && campus.length) {
        $('#summary-campus').text(campus[0].Text);
    } else {
        $('#summary-campus').text("");
    }
    if ($('#Phone').val()) {
        $('#summary-phone').text($('#Phone').val());
    } else {
        $('#summary-phone').text("");
    }
    if ($('#Email').val()) {
        $('#summary-email').text($('#Email').val());
    } else {
        $('#summary-email').text("");
    }
    $('#summary-name').text($('#FirstName').val() + " " + $('#LastName').val());
}