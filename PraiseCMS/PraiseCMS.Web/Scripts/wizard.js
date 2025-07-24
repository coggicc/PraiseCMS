"use strict";

// Class definition
var KTWizard1 = function () {
    // Base elements
    var _wizardEl;
    var _formEl;
    var _wizard;
    var _validations = [];

    // Private functions
    var initWizard = function () {
        // Initialize form wizard
        _wizard = new KTWizard(_wizardEl, {
            startStep: 1, // initial active step number
            clickableSteps: true  // allow step clicking
        });

        // Validation before going to next page
        _wizard.on('beforeNext', function (wizard) {
            // Don't go to the next step yet
            _wizard.stop();

            // Validate form

            var validator = _validations[wizard.getStep() - 1]; // get validator for current step
            validator.validate().then(function (status) {
                if (status === 'Valid') {
                    if (wizard.currentStep === 1) {
                        let status = checkForValidEmailById('Church_Email', 'Js');
                        if (!status) {
                            wizardValidationMessage();
                            return;
                        }
                    }

                    if (wizard.currentStep === 3) {
                        if ($('#collapseOne2').attr('class').includes('show')) {
                            let status = checkForValidEmailById('AdminUserEmail', 'Js');
                            if (!status) {
                                wizardValidationMessage();
                                return;
                            }
                        }
                    }

                    _wizard.goNext();
                    reviewChurchDetail();
                    KTUtil.scrollTop();
                } else {
                    if ($('#collapseOne2').attr('class').includes('show') && status !== 'Valid' && wizard.currentStep === 3) {
                        wizardValidationMessage();
                    }
                    else if (!$('#collapseOne2').attr('class').includes('show') && status !== 'Valid' && wizard.currentStep === 3) {
                        _wizard.goNext();
                        if (wizard.currentStep === 4) {
                            reviewChurchDetail();
                        }
                        KTUtil.scrollTop();
                    }
                    else {
                        wizardValidationMessage();
                    }
                }
            });
        });

        // Change event
        _wizard.on('change', function (wizard) {
            KTUtil.scrollTop();
        });
    };

    var initValidation = function () {
        // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
        // Step 1
        _validations.push(FormValidation.formValidation(
            _formEl,
            {
                fields: {
                    'Church.Name': {
                        validators: {
                            notEmpty: {
                                message: 'Church Name is required'
                            }
                        }
                    },
                    'Church.Phone': {
                        validators: {
                            notEmpty: {
                                message: 'Phone is required'
                            },
                        }
                    },
                    'Church.Email': {
                        validators: {
                            notEmpty: {
                                message: 'Email is required'
                            },
                            email: {
                                message: 'Invalid email'
                            },
                        }
                    },
                    'Church.Denomination': {
                        validators: {
                            notEmpty: {
                                message: 'Denomination is required'
                            },
                        }
                    },
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap: new FormValidation.plugins.Bootstrap()
                }
            }
        ));

        // Step 2
        _validations.push(FormValidation.formValidation(
            _formEl,
            {
                fields: {
                    'Church.PhysicalAddress1': {
                        validators: {
                            notEmpty: {
                                message: 'Address is required'
                            }
                        }
                    },

                    'Church.PhysicalCity': {
                        validators: {
                            notEmpty: {
                                message: 'City is required'
                            }
                        }
                    },
                    'Church.PhysicalState': {
                        validators: {
                            notEmpty: {
                                message: 'State is required'
                            }
                        }
                    },
                    'Church.PhysicalZip': {
                        validators: {
                            notEmpty: {
                                message: 'Zip is required'
                            },
                            digits: {
                                message: 'Invalid zip'
                            }
                        }
                    },
                    //'Church.TimeZone': {
                    //    validators: {
                    //        notEmpty: {
                    //            message: 'Timezone is required'
                    //        }
                    //    }
                    //}
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap: new FormValidation.plugins.Bootstrap()
                }
            }
        ));

        // Step 3
        _validations.push(FormValidation.formValidation(
            _formEl,
            {
                fields: {
                    AdminUserFirstname: {
                        validators: {
                            notEmpty: {
                                message: 'First name is required'
                            }
                        }
                    },
                    AdminUserLastname: {
                        validators: {
                            notEmpty: {
                                message: 'Last name is required'
                            }
                        }
                    },
                    AdminUserEmail: {
                        validators: {
                            notEmpty: {
                                message: 'Email is required'
                            }
                        }
                    },
                    AdminUserPhone: {
                        validators: {
                            notEmpty: {
                                message: 'Phone is required'
                            }
                        }
                    }
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap: new FormValidation.plugins.Bootstrap()
                }
            }
        ));

    };

    return {
        // public functions
        init: function () {
            _wizardEl = KTUtil.getById('kt_wizard_v1');
            _formEl = KTUtil.getById('kt_form');

            initWizard();
            initValidation();
        }
    };
}();

jQuery(document).ready(function () {
    KTWizard1.init();
});

$(document).on('click', '#btn_form_submit', function () {
    $("#alert-container").html('');
    $.ajax({
        type: "POST",
        url: '/Onboarding/ChurchWelcome',
        data: $("#kt_form").serialize(), // serializes the form's elements.
        success: function (result) {
            if (result.Success) {
                const origin = window.location.origin;
                const url = origin.concat('/Home/Index');
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