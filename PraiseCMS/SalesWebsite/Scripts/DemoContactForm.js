var formSubmit = false;
'use strict';
$(function ($) {
    $("form").each(function () {
        var $form = $(this);
        var options = {
            // ignore: [], // uncomment this in case you need to validate :hidden inputs ([type=hidden], display:none are considered :hidden)
            errorPlacement: function (error, element) {
                var $parent = element.parent();

                if ($parent.hasClass("input-group")) {
                    error.insertAfter($parent);
                } else if ($parent.hasClass("has-icon")) {
                    error.insertBefore($parent);
                } else if ($parent.hasClass("control")) {
                    error.insertAfter(element.next('.control-label'));
                } else {
                    error.insertAfter(element);
                }
            }
        };

        if ($form.data("validate-on") == "submit") {
            $.extend(options, {
                onfocusout: false,
                onkeyup: false
            });
        }

        // call to validate plugin
        $form.validate(options);
    });

    function validatePhone($form) {
        const val = $form[0].elements['PhoneNumber'].value;
        let length = parseInt(val.replace(/\D/g, "").length);
        if (length === 0) {
            $('#contact_phoneNumber-error').show();
            $('#contact_phoneNumber-error').html('Please enter your phone number.');
            return false;
        }
        else if (length < 10) {
            $('#contact_phoneNumber-error').show();
            $('#contact_phoneNumber-error').html('Please enter a valid 10 digits phone number.');
            return false;
        } else {
            $('#contact_phoneNumber-error').html('');
            $('#contact_phoneNumber-error').hide();
            return true;
        }
    }

    $(document).on('keyup', '#contact_phoneNumber', function () {
        if (formSubmit) {
            validatePhone($(this).parents('form'));
        }
    })

    $("form").submit(function (evt) {
        formSubmit = true;
        evt.preventDefault();
        let $form = $(this);
        let $inputs = $('#' + $(this).attr('id') + ' :input');
        $inputs.each(function () {
            if ($(this).val() && $(this).val() !== null && $(this).val() !== "") {
                $(this).val($(this).val().trim());
            }
        });

        if (!$form.valid()) {
            validatePhone($form);
            return false;
        }
        if (!validatePhone($form)) {
            return false;
        }

        var $submit = $("button[type=submit]", this);
        $submit.html("<span class='mr-4'>Sending...</span><i class='spinner-border'></i>");
        function doAjax(url, data, config) {
            $.ajax({
                type: "POST",
                url: url,
                data: data,
                dataType: 'json',
                success: function (result) {
                    $('.error-message').hide();
                    $('.form-contact').hide();
                    $('.success-message').show();
                },
                error: function (result) {
                    $('.error-message').hide();
                    $('.form-contact').hide();
                    $('.success-message').show();
                }
            });
        }

        function submitAjax($form) {
            doAjax(
                $form.attr('action'),
                $form.serializeArray()
            );
        }

        submitAjax($form);

        return false;
    });
});