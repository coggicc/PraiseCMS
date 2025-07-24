"use strict";
var KTWizard3 = function () {
    var e, t, i, a = [];
    return {
        init: function () {
            e = KTUtil.getById("kt_wizard_v3"),
                t = KTUtil.getById("kt_form"),
                (
                    i = new KTWizard(e, {
                        startStep: 1,
                        clickableSteps: true
                    })).on("beforeNext", (function (e) {
                        if (!(e.getStep() > e.getNewStep())) {
                            var nextstep = e.getStep() + 1;
                            var t = a[e.getStep() - 1];
                            return t && t.validate().then((function (t) {
                                "Valid" === t ? (e.goTo(e.getNewStep()), KTUtil.scrollTop())
                                    : Swal.fire({
                                        text: "Uh-oh! It looks like some errors were detected. Please follow the on-screen instructions and try again.",
                                        icon: "error", buttonsStyling: !1, confirmButtonText: "Ok, got it!",
                                        customClass: { confirmButton: "btn btn-primary font-weight-bold" }
                                    }).then((function () {
                                        KTUtil.scrollTop();
                                    }));
                                if (nextstep === document.querySelectorAll('[data-wizard-type="step"]').length) {
                                    GetSummary();
                                }
                            })), !1;
                        }
                    }
                    )),
                i.on("changed", (function (e) {
                    KTUtil.scrollTop();
                })),
                i.on("submit", (function (e) {
                    var i = a[e.getStep() - 1];
                    i && i.validate().then((function (e) {
                        "Valid" === e ? t.submit()
                            : Swal.fire({
                                text: "Uh-oh! It looks like some errors were detected. Please follow the on-screen instructions and try again.",
                                icon: "error", buttonsStyling: !1, confirmButtonText: "Ok, got it!",
                                customClass: { confirmButton: "btn btn-primary font-weight-bold" }
                            }).then((function () {
                                KTUtil.scrollTop();
                            }));
                    }));
                })),

                validationStep(a, t);

            // Additional code for SSN formatting
            $("#ChurchMerchantAccount_RespContactSSN").on("input", function () {
                var ssn = $(this).val().replace(/\D/g, ''); // Remove non-numeric characters
                var formattedSSN = "";

                for (var i = 0; i < ssn.length; i++) {
                    if (i === 3 || i === 5) {
                        formattedSSN += "-";
                    }
                    formattedSSN += ssn[i];
                }

                $(this).val(formattedSSN);
            });
        }
    };
}();
function validateForm(self) {
    if ($(self).val()) {
        $(self).removeClass('is-invalid');
        var msg = $(self).parent('.form-group').children('.fv-plugins-message-container');
        msg.html('');
    }
}
$(document).ready((function () {
    KTWizard3.init();
    if ($('#Account_AccountNumber').val()) {
        $('#ConfirmAccountNumber').val($('#Account_AccountNumber').val());
    }
    if (document.querySelectorAll('[data-wizard-type="step"]').length === 1) {
        GetSummary();
        $('#submit-btn,#prev-btn').hide();
        $('#enable-giving-btn').show();
    }
}));
function validationStep(a, t) {
    a.push(FormValidation.formValidation(t,
        {
            fields: {
                'Church.LegalName': { validators: { notEmpty: { message: "Please enter your church's legal name." } } },
                'Account.BusinessType': { validators: { notEmpty: { message: "Please select a business type." } } },
                'Church.TaxIdNumber': {
                    validators: {
                        notEmpty: { message: "Please enter your church's tax id number." },
                        callback: {
                            message: 'Please enter a valid nine digit Tax ID #.',
                            callback: function (input) {
                                let taxId = t.querySelector('[name="Church.TaxIdNumber"]').value;
                                return (taxId.indexOf('X') === -1) ? true : false;
                            }
                        }
                    }
                },
                'Church.Email': {
                    validators: {
                        notEmpty: { message: 'Please enter an email address.' },
                        emailAddress: { message: 'Please enter a valid email address.' }
                    }
                },
                'Church.Website': {
                    validators:
                    {
                        callback: {
                            message: 'Please enter a valid church Website.',
                            callback: function () {
                                const regexp = /^(http[s]?:\/\/){0,1}(www\.){0,1}[a-zA-Z0-9\.\-]+\.[a-zA-Z]{2,5}[\.]{0,1}/;
                                let val = t.querySelector('[name="Church.Website"]').value;
                                if (!val) {
                                    return true;
                                }
                                else if (regexp.test(val)) {
                                    return true;
                                } else {
                                    return false;
                                }
                            }
                        }
                    }
                }
            },
            plugins: {
                trigger: new FormValidation.plugins.Trigger,
                bootstrap: new FormValidation.plugins.Bootstrap({ eleValidClass: "" })
            }
        }));

    a.push(FormValidation.formValidation(t,
        {
            fields: {
                'Church.PhysicalAddress1': { validators: { notEmpty: { message: "Please enter an address." } } },
                'Church.PhysicalCity': { validators: { notEmpty: { message: "Please enter a city." } } },
                'Church.PhysicalState': { validators: { notEmpty: { message: "Please select a state." } } },
                'Church.PhysicalZip': { validators: { notEmpty: { message: "Please enter a zip code." } } }
            },
            plugins: {
                trigger: new FormValidation.plugins.Trigger,
                bootstrap: new FormValidation.plugins.Bootstrap({ eleValidClass: "" })
            }
        }));

    a.push(FormValidation.formValidation(t,
        {
            fields: {
                'Account.RoutingNumber': { validators: { notEmpty: { message: "Please enter a routing number." } } },
                'Account.AccountNumber': { validators: { notEmpty: { message: "Please enter an account number." } } },
                'ConfirmAccountNumber': { validators: { notEmpty: { message: "Please confirm the account number." } } },
                'Account.BankAccountType': { validators: { notEmpty: { message: "Please select a bank account type." } } }
            },
            plugins: {
                trigger: new FormValidation.plugins.Trigger,
                bootstrap: new FormValidation.plugins.Bootstrap({ eleValidClass: "" })
            }
        }));

    a.push(FormValidation.formValidation(t,
        {
            fields: {
                'Account.RespContactFirstName': { validators: { notEmpty: { message: "Please enter a first name." } } },
                'Account.RespContactLastName': { validators: { notEmpty: { message: "Please enter a last name." } } },
                'Account.RespContactEmail': {
                    validators: {
                        notEmpty: { message: "Please enter an email address." },
                        emailAddress: { message: 'Please enter a valid email address.' }
                    }
                },
                'Account.RespContactPhone': {
                    validators: {
                        notEmpty: { message: "Please enter a phone number." },
                        callback: {
                            message: 'Please enter a valid 10 digit phone number.',
                            callback: function () {
                                const val = t.querySelector('[name="Account.RespContactPhone"]').value;
                                if (val) {
                                    const length = parseInt(val.replace(/\D/g, "").length);
                                    return (length === 10) ? true : false;
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'Account.RespContactSSN': {
                    validators: {
                        notEmpty: { message: "Please enter a Social Security Number." },
                        callback: {
                            message: 'Please enter a valid Social Security Number.',
                            callback: function () {
                                const val = t.querySelector('[name="Account.RespContactSSN"]').value;
                                if (val) {
                                    // Remove non-numeric characters
                                    const cleanedSSN = val.replace(/\D/g, "");

                                    // Check if the cleaned SSN has the correct length
                                    if (cleanedSSN.length === 9) {
                                        // Format the SSN with hyphens
                                        const formattedSSN = cleanedSSN.replace(/(\d{3})(\d{2})(\d{4})/, '$1-$2-$3');

                                        // Update the input field with the formatted SSN
                                        t.querySelector('[name="Account.RespContactSSN"]').value = formattedSSN;
                                        return true;
                                    } else {
                                        return false;
                                    }
                                } else {
                                    return true;
                                }
                            }
                        }
                    }
                },
                'Account.RespContactDOB': {
                    validators: {
                        notEmpty: { message: "Please enter a date of birth." },
                        date: {
                            format: 'MM/DD/YYYY',
                            message: 'Please enter a valid date.'
                        }
                    }
                },
                'Account.RespContactDLN': { validators: { notEmpty: { message: "Please enter a driver's license number." } } },                
                'Account.RespContactCity': { validators: { notEmpty: { message: "Please enter the city." } } },
                'Account.RespContactState': { validators: { notEmpty: { message: "Please select a state." } } },
                'Account.RespContactZip': { validators: { notEmpty: { message: "Please enter a zip." } } },
                'Account.RespContactAddress1': { validators: { notEmpty: { message: "Please enter an address." } } }
            },
            plugins: {
                trigger: new FormValidation.plugins.Trigger,
                bootstrap: new FormValidation.plugins.Bootstrap({ eleValidClass: "" })
            }
        }));
}

$(document).on('click', '#submit-btn, #enable-giving-btn', function () {
    $("#alert-container").html('');
    const form = document.getElementById('kt_form');
    if (form.Church_Website.value) {
        const prefix1 = 'http://';
        const prefix2 = 'https://';
        if (form.Church_Website.value.substr(0, prefix1.length) !== prefix1 && form.Church_Website.value.substr(0, prefix2.length) !== prefix2) {
            form.Church_Website.value = prefix1 + form.Church_Website.value;
        }
    }
    $.ajax({
        type: "POST",
        url: '/Onboarding/CreateMerchantAccount',
        data: $("#kt_form").serialize(), // serializes the form's elements.
        success: function (result) {
            if (result.Success) {
                const origin = window.location.origin;
                const url = origin.concat('/Home/Welcome');
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

function GetSummary() {
    var form = document.getElementById('kt_form');
    $('#summary-legalname').text(form.Church_LegalName.value);
    $('#summary-business-type').text(form.Account_BusinessType.value);
    $('#summary-taxid').text(form.Church_TaxIdNumber.value);
    $('#summary-address1').text(form.Church_PhysicalAddress1.value);
    $('#summary-address2').text(form.Church_PhysicalAddress2.value);
    $('#summary-city').text(form.Church_PhysicalCity.value);
    $('#summary-state').text(form.Church_PhysicalState.value);
    $('#summary-zip').text(form.Church_PhysicalZip.value);
    $('#summary-email').text(form.Church_Email.value);
    $('#summary-website').text(form.Church_Website.value);
    $('#summary-account-number').text(form.Account_AccountNumber.value);
    $('#summary-routing-number').text(form.PaymentAccount_RoutingNumber.value);
    $('#summary-BA-type').text((form.Account_BankAccountType.value === 'S' ? 'Savings' : 'Checking'));
    $('#summary-FI-f-name').text(form.Account_RespContactFirstName.value);
    $('#summary-FI-l-name').text(form.Account_RespContactLastName.value);
    $('#summary-FI-email').text(form.Account_RespContactEmail.value);
    $('#summary-FI-phone').text(form.Account_RespContactPhone.value);
    $('#summary-FI-dob').text(form.Account_RespContactDOB.value);
    $('#summary-FI-ssn').text(form.Account_RespContactSSN.value);
    $('#summary-FI-address1').text(form.Account_RespContactAddress1.value);
    $('#summary-FI-address2').text(form.Account_RespContactAddress2.value);
    $('#summary-FI-city').text(form.Account_RespContactCity.value);
    $('#summary-FI-state').text(form.Account_RespContactState.value);
    $('#summary-FI-zip').text(form.Account_RespContactZip.value);
}