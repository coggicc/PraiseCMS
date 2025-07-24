"use strict";

// Class definition
var KTAppInbox = function () {
    // Private properties
    //var _asideEl;
    var _listEl;
    var _viewEl;
    //var _composeEl;
    //var _replyEl;
    //var _asideOffcanvas;
    var _selectedIds = [];
    var _selectedItems = [];

    // Private methods
    //var _initEditor = function (form, editorId) {
    //    // init editor
    //    var options = {
    //        modules: {
    //            toolbar: {}
    //        },
    //        placeholder: 'Type message...',
    //        theme: 'snow'
    //    };

    //    // Init editor
    //    var editorElement = document.getElementById(editorId);

    //    if (editorElement) {
    //        var editor = new Quill(editorElement, options);
    //    } else {
    //        console.error('Editor element not found:', editorId);
    //    }

    //    // Customize editor
    //    var toolbar = KTUtil.find(form, '.ql-toolbar');
    //    var editor = KTUtil.find(form, '.ql-editor');

    //    if (toolbar) {
    //        KTUtil.addClass(toolbar, 'px-5 border-top-0 border-left-0 border-right-0');
    //    }

    //    if (editor) {
    //        KTUtil.addClass(editor, 'px-8');
    //    }
    //}

    //var _initForm = function (formEl) {
    //    var formEl = KTUtil.getById(formEl);

    //    // Init autocompletes
    //    var toEl = KTUtil.find(formEl, '[name=compose_to]');
    //    var tagifyTo = new Tagify(toEl, {
    //        delimiters: ", ", // add new tags when a comma or a space character is entered
    //        maxTags: 10,
    //        blacklist: ["fuck", "shit", "pussy"],
    //        keepInvalidTags: true, // do not remove invalid tags (but keep them marked as invalid)
    //        whitelist: [{
    //            value: 'Chris Muller',
    //            email: 'chris.muller@wix.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_11.jpg',
    //            class: 'tagify__tag--primary'
    //        }, {
    //            value: 'Nick Bold',
    //            email: 'nick.seo@gmail.com',
    //            initials: 'SS',
    //            initialsState: 'warning',
    //            pic: ''
    //        }, {
    //            value: 'Alon Silko',
    //            email: 'alon@keenthemes.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_6.jpg'
    //        }, {
    //            value: 'Sam Seanic',
    //            email: 'sam.senic@loop.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_8.jpg'
    //        }, {
    //            value: 'Sara Loran',
    //            email: 'sara.loran@tilda.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_9.jpg'
    //        }, {
    //            value: 'Eric Davok',
    //            email: 'davok@mix.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_13.jpg'
    //        }, {
    //            value: 'Sam Seanic',
    //            email: 'sam.senic@loop.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_13.jpg'
    //        }, {
    //            value: 'Lina Nilson',
    //            email: 'lina.nilson@loop.com',
    //            initials: 'LN',
    //            initialsState: 'danger',
    //            pic: './assets/media/users/100_15.jpg'
    //        }],
    //        templates: {
    //            dropdownItem: function (tagData) {
    //                try {
    //                    var html = '';

    //                    html += '<div class="tagify__dropdown__item">';
    //                    html += '   <div class="d-flex align-items-center">';
    //                    html += '       <span class="symbol sumbol-' + (tagData.initialsState ? tagData.initialsState : '') + ' mr-2">';
    //                    html += '           <span class="symbol-label" style="background-image: url(\'' + (tagData.pic ? tagData.pic : '') + '\')">' + (tagData.initials ? tagData.initials : '') + '</span>';
    //                    html += '       </span>';
    //                    html += '       <div class="d-flex flex-column">';
    //                    html += '           <a href="#" class="text-dark-75 text-hover-primary font-weight-bold">' + (tagData.value ? tagData.value : '') + '</a>';
    //                    html += '           <span class="text-muted font-weight-bold">' + (tagData.email ? tagData.email : '') + '</span>';
    //                    html += '       </div>';
    //                    html += '   </div>';
    //                    html += '</div>';

    //                    return html;
    //                } catch (err) { }
    //            }
    //        },
    //        transformTag: function (tagData) {
    //            tagData.class = 'tagify__tag tagify__tag--primary';
    //        },
    //        dropdown: {
    //            classname: "color-blue",
    //            enabled: 1,
    //            maxItems: 5
    //        }
    //    });

    //    var ccEl = KTUtil.find(formEl, '[name=compose_cc]');
    //    var tagifyCc = new Tagify(ccEl, {
    //        delimiters: ", ", // add new tags when a comma or a space character is entered
    //        maxTags: 10,
    //        blacklist: ["fuck", "shit", "pussy"],
    //        keepInvalidTags: true, // do not remove invalid tags (but keep them marked as invalid)
    //        whitelist: [{
    //            value: 'Chris Muller',
    //            email: 'chris.muller@wix.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_11.jpg',
    //            class: 'tagify__tag--primary'
    //        }, {
    //            value: 'Nick Bold',
    //            email: 'nick.seo@gmail.com',
    //            initials: 'SS',
    //            initialsState: 'warning',
    //            pic: ''
    //        }, {
    //            value: 'Alon Silko',
    //            email: 'alon@keenthemes.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_6.jpg'
    //        }, {
    //            value: 'Sam Seanic',
    //            email: 'sam.senic@loop.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_8.jpg'
    //        }, {
    //            value: 'Sara Loran',
    //            email: 'sara.loran@tilda.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_9.jpg'
    //        }, {
    //            value: 'Eric Davok',
    //            email: 'davok@mix.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_13.jpg'
    //        }, {
    //            value: 'Sam Seanic',
    //            email: 'sam.senic@loop.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_13.jpg'
    //        }, {
    //            value: 'Lina Nilson',
    //            email: 'lina.nilson@loop.com',
    //            initials: 'LN',
    //            initialsState: 'danger',
    //            pic: './assets/media/users/100_15.jpg'
    //        }],
    //        templates: {
    //            dropdownItem: function (tagData) {
    //                try {
    //                    var html = '';

    //                    html += '<div class="tagify__dropdown__item">';
    //                    html += '   <div class="d-flex align-items-center">';
    //                    html += '       <span class="symbol sumbol-' + (tagData.initialsState ? tagData.initialsState : '') + ' mr-2" style="background-image: url(\'' + (tagData.pic ? tagData.pic : '') + '\')">';
    //                    html += '           <span class="symbol-label">' + (tagData.initials ? tagData.initials : '') + '</span>';
    //                    html += '       </span>';
    //                    html += '       <div class="d-flex flex-column">';
    //                    html += '           <a href="#" class="text-dark-75 text-hover-primary font-weight-bold">' + (tagData.value ? tagData.value : '') + '</a>';
    //                    html += '           <span class="text-muted font-weight-bold">' + (tagData.email ? tagData.email : '') + '</span>';
    //                    html += '       </div>';
    //                    html += '   </div>';
    //                    html += '</div>';

    //                    return html;
    //                } catch (err) { }
    //            }
    //        },
    //        transformTag: function (tagData) {
    //            tagData.class = 'tagify__tag tagify__tag--primary';
    //        },
    //        dropdown: {
    //            classname: "color-blue",
    //            enabled: 1,
    //            maxItems: 5
    //        }
    //    });

    //    var bccEl = KTUtil.find(formEl, '[name=compose_bcc]');
    //    var tagifyBcc = new Tagify(bccEl, {
    //        delimiters: ", ", // add new tags when a comma or a space character is entered
    //        maxTags: 10,
    //        blacklist: ["fuck", "shit", "pussy"],
    //        keepInvalidTags: true, // do not remove invalid tags (but keep them marked as invalid)
    //        whitelist: [{
    //            value: 'Chris Muller',
    //            email: 'chris.muller@wix.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_11.jpg',
    //            class: 'tagify__tag--primary'
    //        }, {
    //            value: 'Nick Bold',
    //            email: 'nick.seo@gmail.com',
    //            initials: 'SS',
    //            initialsState: 'warning',
    //            pic: ''
    //        }, {
    //            value: 'Alon Silko',
    //            email: 'alon@keenthemes.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_6.jpg'
    //        }, {
    //            value: 'Sam Seanic',
    //            email: 'sam.senic@loop.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_8.jpg'
    //        }, {
    //            value: 'Sara Loran',
    //            email: 'sara.loran@tilda.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_9.jpg'
    //        }, {
    //            value: 'Eric Davok',
    //            email: 'davok@mix.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_13.jpg'
    //        }, {
    //            value: 'Sam Seanic',
    //            email: 'sam.senic@loop.com',
    //            initials: '',
    //            initialsState: '',
    //            pic: './assets/media/users/100_13.jpg'
    //        }, {
    //            value: 'Lina Nilson',
    //            email: 'lina.nilson@loop.com',
    //            initials: 'LN',
    //            initialsState: 'danger',
    //            pic: './assets/media/users/100_15.jpg'
    //        }],
    //        templates: {
    //            dropdownItem: function (tagData) {
    //                try {
    //                    var html = '';

    //                    html += '<div class="tagify__dropdown__item">';
    //                    html += '   <div class="d-flex align-items-center">';
    //                    html += '       <span class="symbol sumbol-' + (tagData.initialsState ? tagData.initialsState : '') + ' mr-2" style="background-image: url(\'' + (tagData.pic ? tagData.pic : '') + '\')">';
    //                    html += '           <span class="symbol-label">' + (tagData.initials ? tagData.initials : '') + '</span>';
    //                    html += '       </span>';
    //                    html += '       <div class="d-flex flex-column">';
    //                    html += '           <a href="#" class="text-dark-75 text-hover-primary font-weight-bold">' + (tagData.value ? tagData.value : '') + '</a>';
    //                    html += '           <span class="text-muted font-weight-bold">' + (tagData.email ? tagData.email : '') + '</span>';
    //                    html += '       </div>';
    //                    html += '   </div>';
    //                    html += '</div>';

    //                    return html;
    //                } catch (err) { }
    //            }
    //        },
    //        transformTag: function (tagData) {
    //            tagData.class = 'tagify__tag tagify__tag--primary';
    //        },
    //        dropdown: {
    //            classname: "color-blue",
    //            enabled: 1,
    //            maxItems: 5
    //        }
    //    });

    //    // CC input show
    //    KTUtil.on(formEl, '[data-inbox="cc-show"]', 'click', function (e) {
    //        var inputEl = KTUtil.find(formEl, '.inbox-to-cc');
    //        KTUtil.removeClass(inputEl, 'd-none');
    //        KTUtil.addClass(inputEl, 'd-flex');
    //        KTUtil.find(formEl, "[name=compose_cc]").focus();
    //    });

    //    // CC input hide
    //    KTUtil.on(formEl, '[data-inbox="cc-hide"]', 'click', function (e) {
    //        var inputEl = KTUtil.find(formEl, '.inbox-to-cc');
    //        KTUtil.removeClass(inputEl, 'd-flex');
    //        KTUtil.addClass(inputEl, 'd-none');
    //    });

    //    // BCC input show
    //    KTUtil.on(formEl, '[data-inbox="bcc-show"]', 'click', function (e) {
    //        var inputEl = KTUtil.find(formEl, '.inbox-to-bcc');
    //        KTUtil.removeClass(inputEl, 'd-none');
    //        KTUtil.addClass(inputEl, 'd-flex');
    //        KTUtil.find(formEl, "[name=compose_bcc]").focus();
    //    });

    //    // BCC input hide
    //    KTUtil.on(formEl, '[data-inbox="bcc-hide"]', 'click', function (e) {
    //        var inputEl = KTUtil.find(formEl, '.inbox-to-bcc');
    //        KTUtil.removeClass(inputEl, 'd-flex');
    //        KTUtil.addClass(inputEl, 'd-none');
    //    });
    //}

    //var _initAttachments = function (elemId) {
    //    var id = "#" + elemId;
    //    var previewNode = $(id + " .dropzone-item");
    //    previewNode.id = "";
    //    var previewTemplate = previewNode.parent('.dropzone-items').html();
    //    previewNode.remove();

    //    var myDropzone = new Dropzone(id, { // Make the whole body a dropzone
    //        url: "https://keenthemes.com/scripts/void.php", // Set the url for your upload script location
    //        parallelUploads: 20,
    //        maxFilesize: 1, // Max filesize in MB
    //        previewTemplate: previewTemplate,
    //        previewsContainer: id + " .dropzone-items", // Define the container to display the previews
    //        clickable: id + "_select" // Define the element that should be used as click trigger to select files.
    //    });

    //    myDropzone.on("addedfile", function (file) {
    //        // Hookup the start button
    //        $(document).find(id + ' .dropzone-item').css('display', '');
    //    });

    //    // Update the total progress bar
    //    myDropzone.on("totaluploadprogress", function (progress) {
    //        document.querySelector(id + " .progress-bar").style.width = progress + "%";
    //    });

    //    myDropzone.on("sending", function (file) {
    //        // Show the total progress bar when upload starts
    //        document.querySelector(id + " .progress-bar").style.opacity = "1";
    //    });

    //    // Hide the total progress bar when nothing's uploading anymore
    //    myDropzone.on("complete", function (progress) {
    //        var thisProgressBar = id + " .dz-complete";
    //        setTimeout(function () {
    //            $(thisProgressBar + " .progress-bar, " + thisProgressBar + " .progress").css('opacity', '0');
    //        }, 300)
    //    });
    //}

    // Function to show/hide button-actions based on checkbox selections
    var _toggleButtonActions = function () {
        var anyChecked = KTUtil.findAll(_listEl, '[data-inbox="message"] .checkbox input:checked').length > 0;
        if (anyChecked) {
            $(".button-actions").show();
        } else {
            $(".button-actions").hide();
        }
    };

    var _setReadOrUnread = function () {
        var notRead = _selectedItems.filter(x => x.read === "False");
        var read = _selectedItems.filter(x => x.read === "True");

        if (read.length > 0 && notRead.length > 0) {
            $('#readIcon').attr('data-original-title', 'Mark as Read');
            $('#readIcon').data('action', 'Read');
        } else if (read.length > 0) {
            $('#readIcon').attr('data-original-title', 'Mark as Unread');
            $('#readIcon').data('action', 'Unread');
        } else {
            $('#readIcon').attr('data-original-title', 'Mark as Read');
            $('#readIcon').data('action', 'Read');
        }
    };

    var _setPrayedOverOrNotPrayedOver = function () {
        var notPrayedOver = _selectedItems.filter(x => x.prayedOver === "False");
        var prayedOver = _selectedItems.filter(x => x.prayedOver === "True");

        if (prayedOver.length > 0 && notPrayedOver.length > 0) {
            $('#prayedOverIcon').attr('data-original-title', 'Mark as Prayed Over');
            $('#prayedOverIcon').data('action', 'PrayedOver');
        } else if (prayedOver.length > 0) {
            $('#prayedOverIcon').attr('data-original-title', 'Mark as Not Prayed Over');
            $('#prayedOverIcon').data('action', 'NotPrayedOver');
        } else {
            $('#prayedOverIcon').attr('data-original-title', 'Mark as Prayed Over');
            $('#prayedOverIcon').data('action', 'PrayedOver');
        }
    };

    var _markRead = function (eve) {
        var action = $(eve).data('action');
        var relativeUrl = window.location.pathname + window.location.search;

        // Capture the state of each checkbox before the update
        var checkboxes = $('#kt_inbox_list').find('input[type="checkbox"]');
        var checkboxStates = {};
        checkboxes.each(function () {
            checkboxStates[this.id] = this.checked;
        });

        // Get the IDs of the currently selected checkboxes
        var selectedCheckboxes = $('#kt_inbox_list').find('input[type="checkbox"]:checked');
        _selectedIds = []; // Reset _selectedIds
        selectedCheckboxes.each(function () {
            _selectedIds.push(this.id);
        });

        $.ajax({
            url: '/PrayerRequests/_MarkRead',
            type: 'POST',
            data: { ids: _selectedIds, url: relativeUrl, action: action },
            success: function (response) {
                $('#kt_inbox_list').html(response);

                // Restore the state of each checkbox
                $('#kt_inbox_list').find('input[type="checkbox"]').each(function () {
                    if (checkboxStates[this.id] !== undefined) {
                        this.checked = checkboxStates[this.id];
                    }
                });

                // Reapply previous checkbox and button states
                _selectedItems = []; // Reset _selectedItems if necessary
                $('#kt_inbox_list').find('input[type="checkbox"]:checked').each(function () {
                    _selectedItems.push({ id: this.id, read: this.dataset.type, prayedOver: this.dataset.prayedOver });
                });

                KTAppInbox.toggleButtonActions(); // Call to check and toggle button visibility
                KTAppInbox.setReadOrUnread(); // Update button title based on selected items
                KTAppInbox.setPrayedOverOrNotPrayedOver(); // Update button title based on selected items
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    };

    var _markDetailsRead = function (eve) {
        var id = $(eve).data('id');
        var action = $(eve).data('action');
        var relativeUrl = window.location.pathname + window.location.search;

        $.ajax({
            url: '/PrayerRequests/_MarkDetailsRead',
            type: 'POST',
            data: { id: id, action: action },
            success: function (response) {
                if (!response.Success) {
                    alert(response.Message);
                } else {
                    // Update the button title and data-action attribute based on the new read status
                    if (response.IsRead) {
                        $(eve).attr('data-original-title', 'Mark as Unread');
                        $(eve).data('action', 'Unread');
                    } else {
                        $(eve).attr('data-original-title', 'Mark as Read');
                        $(eve).data('action', 'Read');
                    }
                    // Optionally, update the button icon or style based on the new read status
                }
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    };

    var _markPrayedOver = function (eve) {
        var action = $(eve).data('action');
        var relativeUrl = window.location.pathname + window.location.search;
        $.ajax({
            url: '/PrayerRequests/_MarkPrayedOver',
            type: 'POST',
            data: { ids: _selectedIds, url: relativeUrl, action: action },
            success: function (response) {
                _selectedIds = [];
                _selectedItems = [];
                $('#kt_inbox_list').html(response);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    };

    var _markDetailsPrayedOver = function (eve) {
        var id = $(eve).data('id');
        var action = $(eve).data('action');

        $.ajax({
            url: '/PrayerRequests/_MarkDetailsPrayedOver',
            type: 'POST',
            data: { id: id, action: action },
            success: function (response) {
                if (!response.Success) {
                    alert(response.Message);
                } else {
                    // Update the button title and data-action attribute based on the new prayed over status
                    if (response.IsPrayedOver) {
                        $(eve).attr('data-original-title', 'Mark as Not Prayed Over');
                        $(eve).data('action', 'NotPrayedOver');                        
                    } else {
                        $(eve).attr('data-original-title', 'Mark as Prayed Over');
                        $(eve).data('action', 'PrayedOver');
                    }
                    // Reinitialize tooltips if necessary
                    $(eve).tooltip('update');
                }
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    };

    // Public methods
    return {
        // Public functions

        toggleButtonActions: _toggleButtonActions,
        setReadOrUnread: _setReadOrUnread,
        setPrayedOverOrNotPrayedOver: _setPrayedOverOrNotPrayedOver,
        markRead: _markRead,
        markPrayedOver: _markPrayedOver,
        markDetailsRead: _markDetailsRead,
        markDetailsPrayedOver: _markDetailsPrayedOver,
        init: function () {
            // Init variables
            //_asideEl = KTUtil.getById('kt_inbox_aside');
            _listEl = KTUtil.getById('kt_inbox_list');
            _viewEl = KTUtil.getById('kt_inbox_view');
            //_composeEl = KTUtil.getById('kt_inbox_compose');
            //_replyEl = KTUtil.getById('kt_inbox_reply');

            // Init handlers
            //KTAppInbox.initAside();
            KTAppInbox.initList();
            KTAppInbox.initView();
            //KTAppInbox.initReply();
            //KTAppInbox.initCompose();

            // Bind event handlers
            KTAppInbox.bindEventHandlers();
        },

        //initAside: function () {
        //    // Mobile offcanvas for mobile mode
        //    _asideOffcanvas = new KTOffcanvas(_asideEl, {
        //        overlay: true,
        //        baseClass: 'offcanvas-mobile',
        //        //closeBy: 'kt_inbox_aside_close',
        //        toggleBy: 'kt_subheader_mobile_toggle'
        //    });

        //    // View list
        //    KTUtil.on(_asideEl, '.list-item[data-action="list"]', 'click', function (e) {
        //        var type = KTUtil.attr(this, 'data-type');
        //        var listItemsEl = KTUtil.find(_listEl, '.kt-inbox__items');
        //        var navItemEl = this.closest('.kt-nav__item');
        //        var navItemActiveEl = KTUtil.find(_asideEl, '.kt-nav__item.kt-nav__item--active');

        //        //// demo loading
        //        //var loading = new KTDialog({
        //        //    'type': 'loader',
        //        //    'placement': 'top center',
        //        //    'message': 'Loading ...'
        //        //});
        //        //loading.show();

        //        setTimeout(function () {
        //            //loading.hide();

        //            KTUtil.css(_listEl, 'display', 'flex'); // show list
        //            KTUtil.css(_viewEl, 'display', 'none'); // hide view

        //            KTUtil.addClass(navItemEl, 'kt-nav__item--active');
        //            KTUtil.removeClass(navItemActiveEl, 'kt-nav__item--active');

        //            KTUtil.attr(listItemsEl, 'data-type', type);
        //        }, 600);
        //    });
        //},

        bindEventHandlers: function () {
            $(document).on('click', '#readIcon', function () {
                KTAppInbox.markRead(this);
            });

            $(document).on('click', '#prayedOverIcon', function () {
                KTAppInbox.markPrayedOver(this);
            });

            $(document).on('click', '#detailsReadIcon', function () {
                KTAppInbox.markDetailsRead(this);
            });

            $(document).on('click', '#detailsPrayedOverIcon', function () {
                KTAppInbox.markDetailsPrayedOver(this);
            });

            $(document).on('click', '.starred-button', function () {
                var id = $(this).data('id');
                KTAppInbox.markStarred(id);
            });

            $(document).on('click', '#followUpRequiredButton', function () {
                KTAppInbox.markFollowUpRequired();
            });

            $(document).on('click', '.sorting-button', function () {
                var sortBy = $(this).data('sort');
                KTAppInbox.sorting(sortBy);
            });

            $(document).on('click', '.search-button', function () {
                KTAppInbox.getPrayerRequestsByKeyword();
            });

            //Enter key pressed on search
            $(document).on('keypress', '#filterKeyword', function (e) {
                if (e.which === 13) { // 13 is the Enter key
                    KTAppInbox.getPrayerRequestsByKeyword();
                }
            });

            $(document).on('click', '.clear-search-button', function () {
                KTAppInbox.clearSearch();
            });
        },

        initList: function () {
            // View message
            KTUtil.on(_listEl, '[data-inbox="message"]', 'click', function (e) {
                var actionsEl = KTUtil.find(this, '[data-inbox="actions"]');

                // skip actions click
                if (e.target === actionsEl || (actionsEl && actionsEl.contains(e.target) === true)) {
                    return false;
                }

                // Demo loading
                //var loading = new KTDialog({
                //    'type': 'loader',
                //    'placement': 'top center',
                //    'message': 'Loading ...'
                //});
                //loading.show();

                var id = $(this).data('id');
                var orderNumber = $(this).data('order-number');
                var totalCount = $(this).data('total-count');
                var previousId = $(this).data('previous-id');
                var nextId = $(this).data('next-id');

                loadDetailView(id, orderNumber, totalCount, previousId, nextId);
            });

            function loadDetailView(id, orderNumber, totalCount, previousId, nextId) {
                $.ajax({
                    url: '/PrayerRequests/DetailPrayerRequest',
                    type: 'GET',
                    data: { id: id, orderNumber: orderNumber, totalCount: totalCount, previousId: previousId, nextId: nextId },
                    success: function (response) {
                        $('#kt_inbox_view').html(response);

                        KTUtil.addClass(_listEl, 'd-none');
                        KTUtil.removeClass(_listEl, 'd-block');

                        KTUtil.addClass(_viewEl, 'd-block');
                        KTUtil.removeClass(_viewEl, 'd-none');

                        // Update buttons with new values
                        $('#previousButton').data('id', previousId);
                        $('#previousButton').data('order-number', orderNumber - 1);
                        $('#previousButton').data('next-id', id);
                        $('#nextButton').data('id', nextId);
                        $('#nextButton').data('order-number', orderNumber + 1);
                        $('#nextButton').data('previous-id', id);

                        attachNavigationHandlers();
                    },
                    error: function () {
                        alert('Failed to load details.');
                    }
                });
            }

            function attachNavigationHandlers() {
                $('#previousButton').on('click', function (e) {
                    e.preventDefault();
                    var id = $(this).data('id');
                    var orderNumber = parseInt($(this).data('order-number')) - 1;
                    var totalCount = parseInt($(this).data('total-count'));
                    var previousId = $(this).data('previous-id');
                    var nextId = $(this).data('next-id');
                    loadDetailView(id, orderNumber, totalCount, previousId, nextId);
                });

                $('#nextButton').on('click', function (e) {
                    e.preventDefault();
                    var id = $(this).data('id');
                    var orderNumber = parseInt($(this).data('order-number')) + 1;
                    var totalCount = parseInt($(this).data('total-count'));
                    var previousId = $(this).data('previous-id');
                    var nextId = $(this).data('next-id');
                    loadDetailView(id, orderNumber, totalCount, previousId, nextId);
                });
            }

            // Group selection
            KTUtil.on(_listEl, '[data-inbox="group-select"] input', 'click', function () {
                var messages = KTUtil.findAll(_listEl, '[data-inbox="message"]');
                _selectedIds = [];
                _selectedItems = [];

                for (var i = 0, j = messages.length; i < j; i++) {
                    var message = messages[i];
                    var checkbox = KTUtil.find(message, '.checkbox input');
                    checkbox.checked = this.checked;

                    if (this.checked) {
                        KTUtil.addClass(message, 'active');
                        _selectedIds.push(checkbox.id);
                        _selectedItems.push({ id: checkbox.id, read: checkbox.dataset.type, prayedOver: checkbox.dataset.prayedOver });
                    } else {
                        KTUtil.removeClass(message, 'active');
                    }
                }

                KTAppInbox.toggleButtonActions(); // Call to check and toggle button visibility
                KTAppInbox.setReadOrUnread(); // Update button title based on selected items
                KTAppInbox.setPrayedOverOrNotPrayedOver(); // Update button title based on selected items
            });

            // Individual selection
            KTUtil.on(_listEl, '[data-inbox="message"] [data-inbox="actions"] .checkbox input', 'click', function () {
                var item = this.closest('[data-inbox="message"]');
                var id = this.id;
                var type = this.dataset.type;
                var prayedOver = this.dataset.prayedOver;

                if (item && this.checked) {
                    KTUtil.addClass(item, 'active');
                    if (!_selectedIds.includes(id)) {
                        _selectedIds.push(id);
                        _selectedItems.push({ id: id, read: type, prayedOver: prayedOver });
                    }
                } else {
                    KTUtil.removeClass(item, 'active');
                    var index = _selectedIds.indexOf(id);
                    if (index !== -1) {
                        _selectedIds.splice(index, 1);
                        _selectedItems.splice(index, 1);
                    }
                }

                KTAppInbox.toggleButtonActions(); // Call to check and toggle button visibility
                KTAppInbox.setReadOrUnread(); // Update button title based on selected items
                KTAppInbox.setPrayedOverOrNotPrayedOver(); // Update button title based on selected items
            });
        },

        initView: function () {
            // Back to listing
            KTUtil.on(_viewEl, '[data-inbox="back"]', 'click', function () {
                //// demo loading
                //var loading = new KTDialog({
                //    'type': 'loader',
                //    'placement': 'top center',
                //    'message': 'Loading ...'
                //});

                //loading.show();

                setTimeout(function () {
                    //loading.hide();

                    KTUtil.addClass(_listEl, 'd-block');
                    KTUtil.removeClass(_listEl, 'd-none');

                    KTUtil.addClass(_viewEl, 'd-none');
                    KTUtil.removeClass(_viewEl, 'd-block');
                }, 700);
            });

            //// Expand/Collapse reply
            //KTUtil.on(_viewEl, '[data-inbox="message"]', 'click', function (e) {
            //    var message = this.closest('[data-inbox="message"]');

            //    var dropdownToggleEl = KTUtil.find(this, '[data-toggle="dropdown"]');
            //    var toolbarEl = KTUtil.find(this, '[data-inbox="toolbar"]');

            //    // skip dropdown toggle click
            //    if (e.target === dropdownToggleEl || (dropdownToggleEl && dropdownToggleEl.contains(e.target) === true)) {
            //        return false;
            //    }

            //    // skip group actions click
            //    if (e.target === toolbarEl || (toolbarEl && toolbarEl.contains(e.target) === true)) {
            //        return false;
            //    }

            //    //if (KTUtil.hasClass(message, 'toggle-on')) {
            //    //    KTUtil.addClass(message, 'toggle-off');
            //    //    KTUtil.removeClass(message, 'toggle-on');
            //    //} else {
            //    //    KTUtil.removeClass(message, 'toggle-off');
            //    //    KTUtil.addClass(message, 'toggle-on');
            //    //}
            //});
        },

        sorting: function (sortBy) {
            var relativeUrl = window.location.pathname + window.location.search;
            if (!relativeUrl.includes('sortType')) {
                let url = relativeUrl.includes('?') ?
                    relativeUrl + "&sortType=" + sortBy :
                    relativeUrl + "?sortType=" + sortBy;
                location.replace(url);
            } else {
                let url = relativeUrl;
                if (url.includes('asc')) {
                    url = url.replace('asc', sortBy);
                } else if (url.includes('desc')) {
                    url = url.replace('desc', sortBy);
                }
                location.replace(url);
            }
        },

        getPrayerRequestsByKeyword: function () {
            var relativeUrl = window.location.pathname + window.location.search;
            if ($('#filterKeyword').val()) {
                if (!relativeUrl.includes('filterKeyword')) {
                    let url = relativeUrl.includes('?') ?
                        relativeUrl + "&filterKeyword=" + $('#filterKeyword').val() :
                        relativeUrl + "?filterKeyword=" + $('#filterKeyword').val();
                    location.replace(url);
                } else {
                    let url = relativeUrl;
                    let startIndex = url.indexOf("filterKeyword=") + 14;
                    let oldKeyword = url.substr(startIndex);
                    if (oldKeyword !== '') {
                        url = url.replace(oldKeyword, $('#filterKeyword').val());
                    } else {
                        url = relativeUrl + $('#filterKeyword').val();
                    }
                    location.replace(url);
                }
            }
        },

        clearSearch: function () {
            var relativeUrl = window.location.pathname + window.location.search;

            if ($('#filterKeyword').val()) {
                $('#filterKeyword').val('');

                if ($('#clearSearchBtn').is(':visible')) {
                    $('#clearSearchBtn').hide();
                }

                $('#filterKeyword').focus();

                // Use URL and URLSearchParams to manipulate the query string
                let url = new URL(window.location.origin + relativeUrl);
                let params = new URLSearchParams(url.search);

                // Remove the filterKeyword parameter
                params.delete('filterKeyword');

                // Construct the new URL without the filterKeyword parameter
                if (params.toString()) {
                    relativeUrl = window.location.pathname + '?' + params.toString();
                } else {
                    relativeUrl = window.location.pathname;
                }

                // Reload the page with the new URL
                location.replace(relativeUrl);
            }
        },

        markStarred: function (id) {
            var relativeUrl = window.location.pathname + window.location.search;
            $.ajax({
                url: '/PrayerRequests/MarkStarred',
                type: 'POST',
                data: { id: id },
                success: function (response) {
                    if (!response.Success) {
                        alert(response.Message);
                    } else {
                        let cell = $('.starred-' + id);

                        // Update the button's classes, tooltip, and icon based on the new status
                        if (response.Starred) {
                            $(cell).removeClass('unmarked').addClass('marked');
                            $(cell).attr('data-original-title', 'Starred');
                            $(cell).html("<i class='fas fa-star text-warning'></i>");
                        } else {
                            $(cell).removeClass('marked').addClass('unmarked');
                            $(cell).attr('data-original-title', 'Not Starred');
                            $(cell).html("<i class='far fa-star text-muted'></i>");
                        }

                        // Optionally, hide or show the item based on the filter
                        if (relativeUrl && relativeUrl.toLowerCase().includes('starred')) {
                            if (response.Starred) {
                                $('#prayer-request-' + id).show();
                            } else {
                                $('#prayer-request-' + id).hide();
                            }
                        }
                    }
                },
                error: function (response) {
                    alert(response.responseText);
                }
            });
        },

        markFollowUpRequired: function () {
            var relativeUrl = window.location.pathname + window.location.search;
            $.ajax({
                url: '/PrayerRequests/markFollowUpRequired',
                type: 'POST',
                data: { ids: _selectedIds, url: relativeUrl },
                success: function (response) {
                    if (!response) {
                        alert(response.responseText);
                    }
                },
                error: function (response) {
                    alert(response.responseText);
                }
            });
        }

        //initReply: function () {
        //    _initEditor(_replyEl, 'kt_inbox_reply_editor');
        //    _initAttachments('kt_inbox_reply_attachments');
        //    _initForm('kt_inbox_reply_form');
        //},

        //initCompose: function () {
        //    _initEditor(_composeEl, 'kt_inbox_compose_editor');
        //    _initAttachments('kt_inbox_compose_attachments');
        //    _initForm('kt_inbox_compose_form');

        //    // Remove reply form
        //    KTUtil.on(_composeEl, '[data-inbox="dismiss"]', 'click', function (e) {
        //        swal.fire({
        //            text: "Are you sure to discard this message ?",
        //            type: "danger",
        //            buttonsStyling: false,
        //            confirmButtonText: "Discard draft",
        //            confirmButtonClass: "btn btn-danger",
        //            showCancelButton: true,
        //            cancelButtonText: "Cancel",
        //            cancelButtonClass: "btn btn-light-primary"
        //        }).then(function (result) {
        //            $(_composeEl).modal('hide');
        //        });
        //    });
        //}
    };
}();

// Class Initialization
jQuery(document).ready(function () {
    KTAppInbox.init();
});
