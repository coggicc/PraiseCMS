using PraiseCMS.Shared.Methods;
using System;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace PraiseCMS.Shared.Shared
{
    public static class GlobalVariable
    {
        public static bool IsRunning = false;
    }

    public static class SubscriptionPlan
    {
        public static string PlanId = null;
    }

    public static class Constants
    {
        public const string System = "SYSTEM";
        public const string User = "User";
        public const string Person = "person";
        public const string DefaultProfileImage = "~/Content/assets/media/users/blank.png";
        public const string SavedMessage = "Your changes have been saved.";
        public const string CreateExceptionMessage = "Something went wrong while creating the record.";
        public const string UpdateExceptionMessage = "Something went wrong while updating the record.";
        public const string DeleteExceptionMessage = "Something went wrong while deleting the record.";
        public const string DefaultErrorMessage = "Uh-oh! Something went wrong. Error:";
        public const string PhoneExistsMessage = "This phone number has already been registered. Please try another phone number.";
        public const string EmailExistsMessage = "This email has already been registered. Please try another phone number.";
        public const string PhoneAlreadyRegistered = "This phone number has already been registered. Use a new number or try signing in with this number.";
        public const string EmailAlreadyRegistered = "This email has already been registered. Use a new email or try signing in with this email.";
        public const string GiveNowButtonStyling = "<style>.giving_button_praise {color: #FFFFFF;background-color: #3699FF;border-color: #3699FF;display: inline-block;font-weight: normal;text-align: center;user-select: none;border: 1px solid transparent;padding: 0.65rem 1rem;font-size: 1rem;line-height: 1.5;border-radius: 0.42rem;text-transform: none;overflow: visible;margin: 0;font-family: inherit;}</style>";
        public const string DisplayDefaultText = "[No Name Defined]";
        public const string EnableGivingMessage = "<b>Good news!</b> We were able to use some of the data from your settings page. We just need a little more information to finish setting up your giving account. {0}";
        public const string InactiveAccountMessage = "It looks like your account is no longer active. Contact your administrator to reactivate your account.";
        //Default Church Values
        public const string DefaultGivingThankYouText = "<h2>Thank you for your generosity.</h2><p><i>\"Each of you should give what you have decided in your heart to give, not reluctantly or under compulsion, for God loves a cheerful giver.\" 2 Corinthians 9:77 NIV</i></p>";
        public const string DefaultAnnualStatementEmailBody = "<h2>Your {current-year} annual giving statement is available.</h2><p><i>We are so thankful for your generosity!</i></p>";
        public const string DefaultAnnualStatementDisclaimer = "<p>In accordance with IRS guidelines, contributions are shown on the Statement of Contributions if they are received by December 31, or if mailed, the envelope is postmarked no later than December 31.</p><ol><li>A Statement of Contributions is not mailed or emailed for contributions less than $250 unless requested by the contributor. In accordance with IRS guidelines, any single contribution less than $250 may be substantiated by a canceled check.</li><li>Goods or services provided in exchange for a contribution are not reflected on the Statement of Contributions.</li><li>The value of time or services by an individual to the church is not reflected on the Statement of Contributions and generally is not tax deductible.</li></ol>";
        public const string DefaultPrayerRequestReceivedText = "Thank you for sharing your prayer request with us. Our prayer team will lift up your needs in prayer. If you have any further concerns or would like additional support, feel free to reach out to us.";
        public const string DefaultPrayerRequestReceivedFollowUpText = "Thank you for sharing your prayer request with us. A dedicated prayer team member will reach out to you shortly.";
        public const string DefaultPrayerRequestPrayedOverEmailMessage = "Your prayer request has been carefully prayed over.";
        public const string DefaultPrayerRequestPrayedOverTextMessage = "Your prayer request has been carefully prayed over.";

        public static Dictionary<string, string> StateToAbbrev = new Dictionary<string, string>() { { "Alabama", "AL" }, { "Alaska", "AK" }, { "Arizona", "AZ" }, { "Arkansas", "AR" }, { "California", "CA" }, { "Colorado", "CO" }, { "Connecticut", "CT" }, { "Delaware", "DE" }, { "District of Columbia", "DC" }, { "Florida", "FL" }, { "Georgia", "GA" }, { "Hawaii", "HI" }, { "Idaho", "ID" }, { "Illinois", "IL" }, { "Indiana", "IN" }, { "Iowa", "IA" }, { "Kansas", "KS" }, { "Kentucky", "KY" }, { "Louisiana", "LA" }, { "Maine", "ME" }, { "Maryland", "MD" }, { "Massachusetts", "MA" }, { "Michigan", "MI" }, { "Minnesota", "MN" }, { "Mississippi", "MS" }, { "Missouri", "MO" }, { "Montana", "MT" }, { "Nebraska", "NE" }, { "Nevada", "NV" }, { "New Hampshire", "NH" }, { "New Jersey", "NJ" }, { "New Mexico", "NM" }, { "New York", "NY" }, { "North Carolina", "NC" }, { "North Dakota", "ND" }, { "Ohio", "OH" }, { "Oklahoma", "OK" }, { "Oregon", "OR" }, { "Pennsylvania", "PA" }, { "Rhode Island", "RI" }, { "South Carolina", "SC" }, { "South Dakota", "SD" }, { "Tennessee", "TN" }, { "Texas", "TX" }, { "Utah", "UT" }, { "Vermont", "VT" }, { "Virginia", "VA" }, { "Washington", "WA" }, { "West Virginia", "WV" }, { "Wisconsin", "WI" }, { "Wyoming", "WY" } };
        public static Dictionary<string, string> AbbrevToState = new Dictionary<string, string>() { { "AK", "Alaska" }, { "AL", "Alabama" }, { "AR", "Arkansas" }, { "AZ", "Arizona" }, { "CA", "California" }, { "CO", "Colorado" }, { "CT", "Connecticut" }, { "DC", "District of Columbia" }, { "DE", "Delaware" }, { "FL", "Florida" }, { "GA", "Georgia" }, { "HI", "Hawaii" }, { "IA", "Iowa" }, { "ID", "Idaho" }, { "IL", "Illinois" }, { "IN", "Indiana" }, { "KS", "Kansas" }, { "KY", "Kentucky" }, { "LA", "Louisiana" }, { "MA", "Massachusetts" }, { "MD", "Maryland" }, { "ME", "Maine" }, { "MI", "Michigan" }, { "MN", "Minnesota" }, { "MO", "Missouri" }, { "MS", "Mississippi" }, { "MT", "Montana" }, { "NC", "North Carolina" }, { "ND", "North Dakota" }, { "NE", "Nebraska" }, { "NH", "New Hampshire" }, { "NJ", "New Jersey" }, { "NM", "New Mexico" }, { "NV", "Nevada" }, { "NY", "New York" }, { "OH", "Ohio" }, { "OK", "Oklahoma" }, { "OR", "Oregon" }, { "PA", "Pennsylvania" }, { "RI", "Rhode Island" }, { "SC", "South Carolina" }, { "SD", "South Dakota" }, { "TN", "Tennessee" }, { "TX", "Texas" }, { "UT", "Utah" }, { "VA", "Virginia" }, { "VT", "Vermont" }, { "WA", "Washington" }, { "WI", "Wisconsin" }, { "WV", "West Virginia" }, { "WY", "Wyoming" } };
        public static Dictionary<string, string> ButtonColors = new Dictionary<string, string>() { { "danger", "Red" }, { "dark", "Black" }, { "info", "Purple" }, { "primary", "Blue" }, { "success", "Green" }, { "warning", "Yellow" }, { "secondary", "Gray" } };
        public static Dictionary<string, string> Genders = new Dictionary<string, string>() { { "M", "Male" }, { "F", "Female" } };
        public static Dictionary<string, string> GivingFundDescriptions = new Dictionary<string, string> { { GivingFunds.TithesAndOfferings, "Commonly used when donations are received during normal service times." }, { GivingFunds.General, "Commonly used when donations are received with no designation specified." }, { GivingFunds.Missions, "Commonly used for missionary support and outreach programs." } };

        public const string ApiTransactionSuccessCode = "0";
        public const string ApiTransactionApprovalSubResultCode = "00";

        public static class ColorOptions
        {
            public const string Red = "danger";
            public const string Black = "dark";
            public const string Purple = "info";
            public const string Blue = "primary";
            public const string Green = "success";
            public const string Yellow = "warning";
            public const string Gray = "secondary";
        }

        public static Dictionary<int, string> DictionaryTimes
        {
            get
            {
                var times = new List<string>() { "05:00 AM", "06:00 AM", "07:00 AM", "08:00 AM", "09:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "01:00 PM", "02:00 PM", "03:00 PM", "04:00 PM", "05:00 PM", "06:00 PM", "07:00 PM", "08:00 PM", "09:00 PM", };
                var newTimes = new Dictionary<int, string>();
                var count = 0;

                foreach (var time in times)
                {
                    for (var j = 0; j < 60; j += 15)
                    {
                        var minute = j.ToString().PadLeft(2, '0');
                        var newTime = time.Replace(":00", $":{minute}");
                        newTimes.Add(count, newTime);
                        count++;
                    }
                }

                return newTimes;
            }
        }

        public static IEnumerable<string> Times
        {
            get
            {
                var times = new List<string>() { "05:00 AM", "06:00 AM", "07:00 AM", "08:00 AM", "09:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "01:00 PM", "02:00 PM", "03:00 PM", "04:00 PM", "05:00 PM", "06:00 PM", "07:00 PM", "08:00 PM", "09:00 PM", };
                var newTimes = new List<string>();

                foreach (var time in times)
                {
                    for (var j = 0; j < 60; j += 15)
                    {
                        var minute = j.ToString().PadLeft(2, '0');
                        var newTime = time.Replace(":00", $":{minute}");

                        newTimes.Add(newTime);
                    }
                }

                return newTimes;
            }
        }

        public static List<SelectListItem> GetTimeZonesAsDropdown()
        {
            return TimeZones.Items.Select(item => new SelectListItem { Value = item, Text = item }).ToList();
        }

        public static List<SelectListItem> GetAbbrevToStateAsDropdown()
        {
            return AbbrevToState.Select(item => new SelectListItem { Value = item.Key, Text = item.Value }).ToList();
        }

        public static List<SelectListItem> GetTimesDropdown()
        {
            return DictionaryTimes.Select(item => new SelectListItem { Value = item.Value, Text = item.Value }).ToList();
        }

        public static string GenerateStatementPreviewToken(string userId, string churchId, int year)
        {
            return HttpUtility.UrlEncode($"{userId}-{year}-{churchId}".Encrypt());
        }

        public static string GenerateToken(string id)
        {
            return HttpUtility.UrlEncode(id.Encrypt());
        }

        public static string GeneratePassword(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, bool includeSpaces, int lengthOfPassword)
        {
            const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
            const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMERIC_CHARACTERS = "0123456789";
            const string SPECIAL_CHARACTERS = @"!#$%&*@\";
            const string SPACE_CHARACTER = " ";
            const int PASSWORD_LENGTH_MIN = 8;
            const int PASSWORD_LENGTH_MAX = 128;

            if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX)
            {
                return "Password must be at least 8 characters.";
            }

            var characterSet = string.Empty;

            if (includeLowercase)
            {
                characterSet += LOWERCASE_CHARACTERS;
            }

            if (includeUppercase)
            {
                characterSet += UPPERCASE_CHARACTERS;
            }

            if (includeNumeric)
            {
                characterSet += NUMERIC_CHARACTERS;
            }

            if (includeSpecial)
            {
                characterSet += SPECIAL_CHARACTERS;
            }

            if (includeSpaces)
            {
                characterSet += SPACE_CHARACTER;
            }

            var password = new char[lengthOfPassword];
            var characterSetLength = characterSet.Length;

            var random = new Random();

            for (var characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
            {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

                var moreThanTwoIdenticalInARow =
                    characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                    && password[characterPosition] == password[characterPosition - 1]
                    && password[characterPosition - 1] == password[characterPosition - 2];

                if (moreThanTwoIdenticalInARow)
                {
                    characterPosition--;
                }
            }

            return string.Join(null, password);
        }
    }

    public struct SortOrders
    {
        public const string Ascending = "asc";
        public const string Descending = "desc";
    }

    public struct SessionVariableNames
    {
        public const string CurrentUser = "CurrentUser";
        public const string CurrentChurch = "CurrentChurch";
        public const string CurrentCampus = "CurrentCampus";
        public const string CurrentDomain = "CurrentDomain";
        public const string Locations = "Locations";
        public const string Campuses = "Campuses";
        public const string Subscriptions = "Subscriptions";
        public const string PlanPermissions = "PlanPermissions";
        public const string Roles = "Roles";
        public const string AllPermissions = "AllPermissions";
        public const string Modules = "Modules";
        public const string CurrentMerchant = "CurrentMerchant";
        public const string Widgets = "Widgets";
        public const string ReportCategories = "ReportCategories";
    }

    public struct CookieNames
    {
        public const string Authentication = "Praise";
    }

    public struct CookieValues
    {
        public const string Email = "Email";
        public const string UserId = "UserId";
        public const string RememberMe = "RememberMe";
    }

    public struct TimeZones
    {
        public const string Eastern = "Eastern Standard Time";
        public const string Central = "Central Standard Time";
        public const string Mountain = "Mountain Standard Time";
        public const string Pacific = "Pacific Standard Time";

        public static List<string> Items = new List<string> { Eastern, Central, Mountain, Pacific };
    }

    public struct TimeOfTheDay
    {
        public const string EarlyMorning = "Early Morning";
        public const string Morning = "Morning";
        public const string Afternoon = "Afternoon";
        public const string Evening = "Evening";
        public const string Overnight = "Overnight";

        public static List<string> Items = new List<string> { EarlyMorning, Morning, Afternoon, Evening, Overnight };
    }

    public struct EarlyMorningTimes
    {
        public const string Start = "2:00 AM";
        public const string Stop = "5:59 AM";

        public static List<string> Items = new List<string> { Start, Stop };
    }

    public struct MorningTimes
    {
        public const string Start = "6:00 AM";
        public const string Stop = "11:59 AM";

        public static List<string> Items = new List<string> { Start, Stop };
    }

    public struct AfternoonTimes
    {
        public const string Start = "12:00 PM";
        public const string Stop = "4:59 PM";

        public static List<string> Items = new List<string> { Start, Stop };
    }

    public struct EveningTimes
    {
        public const string Start = "5:00 PM";
        public const string Stop = "10:59 PM";

        public static List<string> Items = new List<string> { Start, Stop };
    }

    public struct OvernightTimes
    {
        public const string Start = "11:00 PM";
        public const string Stop = "1:59 AM";

        public static List<string> Items = new List<string> { Start, Stop };
    }

    public struct DaysOfTheWeek
    {
        public const string Sunday = "Sunday";
        public const string Monday = "Monday";
        public const string Tuesday = "Tuesday";
        public const string Wednesday = "Wednesday";
        public const string Thursday = "Thursday";
        public const string Friday = "Friday";
        public const string Saturday = "Saturday";

        public static List<string> Items = new List<string> { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };
    }

    public struct TempDataKeys
    {
        public const string AlertMessage = "AlertMessage";
        public const string AlertMessageType = "AlertMessageType";
        public const string AlertMessageIcon = "AlertMessageIcon";
    }

    public struct AlertMessageTypes
    {
        public const string Default = "alert-light-primary";
        public const string Primary = "alert-light-primary";
        public const string Success = "alert-light-success";
        public const string Info = "alert-light-info";
        public const string Warning = "alert-light-warning";
        public const string Failure = "alert-light-danger";
        public const string Brand = "alert-light-brand";
        public const string Dark = "alert-light-dark";
    }

    public struct AlertMessageIcons
    {
        public const string Default = "fas fa-info-circle";
        public const string Primary = "fas fa-info-circle";
        public const string Success = "fas fa-check-circle";
        public const string Info = "fas fa-info-circle";
        public const string Warning = "fas fa-exclamation-circle";
        public const string Failure = "fas fa-exclamation-triangle";
    }

    public struct PaymentMethodAlertMessages
    {
        public const string CardExists = "This card has already been added. Please add a different payment method.";
        public const string CardAddSuccess = "A new card has been added to your account.";
        public const string CardAddError = "There was a problem adding your card. Please try again later.";
        public const string BankAccountExists = "This bank account has already been added. Please add a different payment method.";
        public const string BankAccountAddSuccess = "A new bank account has been added to your account.";
        public const string BankAccountAddError = "There was a problem adding your bank account. Please try again later.";
        public const string PaymentMethodRemoveSuccess = "Your payment method has been removed.";
    }

    public enum ResultType
    {
        [Description("alert-light-success")]
        Success = 1,

        [Description("alert-light-danger")]
        Failure = 2,

        [Description("alert-light-primary")]
        Empty = 3,

        [Description("alert-light-warning")]
        Warning = 4,

        [Description("alert-light-info")]
        Information = 5,

        [Description("alert-light-danger")]
        ValidationErrors = 6,

        [Description("alert-light-danger")]
        Exception = 7,

        [Description("alert-light-primary")]
        CodeWord = 8,

        [Description("alert-light-primary")]
        Redirect = 9,

        [Description("alert-light-primary")]
        Function = 10,

        [Description("alert-light-primary")]
        Code = 11,

        [Description("alert-light-warning")]
        Duplicate = 12,

        [Description("alert-light-info")]
        AccessRights = 13,

        [Description("alert-light-danger")]
        DeActivated = 14
    }

    public enum ResultIcon
    {
        [Description("fas fa-info-circle")]
        Default = 1,

        [Description("fas fa-info-circle")]
        Primary = 2,

        [Description("fas fa-check-circle")]
        Success = 3,

        [Description("fas fa-info-circle")]
        Info = 4,

        [Description("fas fa-exclamation-circle")]
        Warning = 5,

        [Description("fas fa-exclamation-triangle")]
        Failure = 6
    }

    public struct GraphTypes
    {
        public const string AreaGraph = "Area Graph";
        public const string BarGraph = "Bar Graph";
        public const string DoughnutChart = "Doughnut Chart";
        public const string LineGraph = "Line Graph";
        public const string MultiAxisLineGraph = "Multi Axis Line Graph";
        public const string MixLineBarChart = "Mix Line Bar Chart";
        public const string PieChart = "Pie Chart";

        public static List<string> Items = new List<string> { AreaGraph, BarGraph, DoughnutChart, LineGraph, MultiAxisLineGraph, MixLineBarChart, PieChart };
    }

    public struct ReportCategories
    {
        public const string Custom = "Custom";
        public const string Favorites = "Favorites";
        public const string Attendance = "Attendance";
        public const string Giving = "Giving";
        public const string Salvations = "Salvations";
        public const string PrayerRequests = "Prayer Requests";
        public const string SmallGroups = "Small Groups";
    }

    public struct GoogleMapTypes
    {
        public const string DoNotMap = "DO NOT MAP";
        public const string RoadMap = "ROADMAP";
        public const string Terrain = "TERRAIN";
        public const string Hybrid = "HYBRID";
        public const string Satellite = "SATELLITE";

        public static List<string> Items = new List<string> { RoadMap, Terrain, Hybrid, Satellite, DoNotMap };
    }

    public struct GoogleMapZooms
    {
        public static List<int> Items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
    }

    public struct Roles
    {
        public const string SuperAdmin = "Super Admin";
        public const string Administrator = "Administrator";
        public const string Donor = "Donor";
        public const string Accountant = "Accountant";
        public const string Pastor = "Pastor";
        public const string SeniorPastor = "Senior Pastor";
        public const string PrayerTeam = "Prayer Team";
        public const string Volunteer = "Volunteer";
        public const string VolunteerLeader = "Volunteer Leader";

        public static List<string> Items = new List<string> { SuperAdmin, Administrator, Donor, Accountant, Pastor, SeniorPastor, PrayerTeam, Volunteer, VolunteerLeader };
        public static List<string> LimitedAccessItems = new List<string> { Volunteer, Donor };
    }

    public enum PermissionType
    {
        [Description("Role")]
        Role,
        [Description("User")]
        User,
        [Description("SubscriptionType")]
        Subscription
    }

    public struct AccessCodeMethods
    {
        public const string Text = "Text me an access code";
        public const string Email = "Email me an access code";

        public static List<string> Items = new List<string> { Text, Email };
    }

    public struct PageStatuses
    {
        public const string Draft = "Draft";
        public const string Active = "Active";

        public static List<string> Items = new List<string> { Draft, Active };
    }

    public struct PageTypes
    {
        public const string Content = "Content";
        public const string Link = "Link";

        public static List<string> Items = new List<string> { Content, Link };
    }

    public struct PageShortcodes
    {
        public const string PageContent = "{page-content}";
        public const string PageCss = "{page-css}";
        public const string PageTitle = "{page-title}";
        public const string PageMenuText = "{page-menu-text}";
        public const string PageKeywords = "{page-keywords}";
        public const string PageDescription = "{page-description}";
        public const string PageUrl = "{page-url}";

        public static List<string> Items = new List<string> { PageContent, PageCss, PageTitle, PageMenuText, PageKeywords, PageDescription };
    }

    public struct MessageTypes
    {
        public const string Text = "Text";
        public const string Email = "Email";

        public static List<string> Items = new List<string> { Text, Email };
    }

    public enum ContactMethod
    {
        [Description("All Methods")]
        All = 0,
        [Description("Text Message")]
        Text = 1,
        [Description("Email")]
        Email = 2,
        [Description("System Notification")]
        SystemNotification = 3
    }

    public struct TourStepPlacements
    {
        public const string Top = "top";
        public const string Bottom = "bottom";
        public const string Left = "left";
        public const string Right = "right";
    }

    public struct NewsletterStatuses
    {
        public const string Draft = "Draft";
        public const string Queued = "Queued";
        public const string TempDraft = "Temp Draft";
        public const string Sent = "Sent";

        public static List<string> Items = new List<string> { Draft, Queued, Sent };
    }

    public struct NewsletterMemberActive
    {
        public const string All = "Active & Inactive";
        public const string Active = "Only Active";
        public const string Inactive = "Only Inactive";

        public static List<string> Items = new List<string> { All, Active, Inactive };
    }

    public struct ProgressStatuses
    {
        public const string NotStarted = "Not Started";
        public const string InProgress = "In Progress";
        public const string Complete = "Complete";

        public static List<string> Items = new List<string> { NotStarted, InProgress, Complete };
    }

    public struct Operations
    {
        public const int NoAccess = 0;
        public const int ReadOnly = 1;
        public const int ReadWrite = 2;

        public static Dictionary<int, string> Descriptions = new Dictionary<int, string> { { NoAccess, "No Access" }, { ReadOnly, "Read Only" }, { ReadWrite, "Read Write" } };
    }

    public struct SmsMessageType
    {
        public const string Register = "Register";
        public const string SignIn = "SignIn";
        public const string Gift = "Gift";
        public const string Verify = "Verify";
    }

    public struct ServiceType
    {
        public const string MidWeek = "Mid-Week";
        public const string Weekend = "Weekend";
        public static List<string> Items = new List<string> { MidWeek, Weekend };
    }

    public struct EmailTemplatesNameList
    {
        public const string VerifyEmail = "Verify Email";
        public const string NewUserAccount = "New User Account";
        public const string ChurchRegistration = "Church Registration";
        public const string ChurchRegistrationSuperAdmin = "Church Registration Super Admin";
        public const string ForgotPassword = "Forgot Password";
        public const string ResetPassword = "Reset Password";
        public const string PasswordChanged = "Password Changed";
        public const string PaymentProcessed = "Payment Processed";
        public const string ScheduledGiving = "Scheduled Giving";
        public const string PaymentError = "Payment Error";
        public const string AnnualGivingStatement = "Annual Giving Statement";
        public const string SendInvitation = "Send Invitation";
        public const string VerificationCode = "Verification Code";

        public static List<string> Items = new List<string> {
          AnnualGivingStatement ,ChurchRegistration ,ChurchRegistrationSuperAdmin ,ForgotPassword ,NewUserAccount ,PasswordChanged
        ,PaymentError ,PaymentProcessed ,ResetPassword ,ScheduledGiving ,VerifyEmail, SendInvitation, VerificationCode };
    }

    #region Email Templates    
    public struct EmailTemplates
    {
        public const string Base = @"<!doctype html>
            <html lang='en'>
            <head>
            <meta charset='utf-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>
            <meta name='x-apple-disable-message-reformatting'>
            <meta http-equiv='X-UA-Compatible' content='IE=edge'>
            <title>Forgot Password</title>
            <style type='text/css'>
                a { text-decoration: none; outline: none; }
                @media (max-width: 649px) {
                .o_col-full { max-width: 100% !important; }
                .o_col-half { max-width: 50% !important; }
                .o_hide-lg { display: inline-block !important; font-size: inherit !important; max-height: none !important; line-height: inherit !important; overflow: visible !important; width: auto !important; visibility: visible !important; }
                .o_hide-xs, .o_hide-xs.o_col_i { display: none !important; font-size: 0 !important; max-height: 0 !important; width: 0 !important; line-height: 0 !important; overflow: hidden !important; visibility: hidden !important; height: 0 !important; }
                .o_xs-center { text-align: center !important; }
                .o_xs-left { text-align: left !important; }
                .o_xs-right { text-align: left !important; }
                table.o_xs-left { margin-left: 0 !important; margin-right: auto !important; float: none !important; }
                table.o_xs-right { margin-left: auto !important; margin-right: 0 !important; float: none !important; }
                table.o_xs-center { margin-left: auto !important; margin-right: auto !important; float: none !important; }
                h1.o_heading { font-size: 32px !important; line-height: 41px !important; }
                h2.o_heading { font-size: 26px !important; line-height: 37px !important; }
                h3.o_heading { font-size: 20px !important; line-height: 30px !important; }
                .o_xs-py-md { padding-top: 24px !important; padding-bottom: 24px !important; }
                .o_xs-pt-xs { padding-top: 8px !important; }
                .o_xs-pb-xs { padding-bottom: 8px !important; }
                }
                @media screen {
                @font-face {
                    font-family: 'Roboto';
                    font-style: normal;
                    font-weight: 400;
                    src: local('Roboto'), local('Roboto-Regular'), url(https://fonts.gstatic.com/s/roboto/v18/KFOmCnqEu92Fr1Mu7GxKOzY.woff2) format('woff2');
                    unicode-range: U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF; }
                @font-face {
                    font-family: 'Roboto';
                    font-style: normal;
                    font-weight: 400;
                    src: local('Roboto'), local('Roboto-Regular'), url(https://fonts.gstatic.com/s/roboto/v18/KFOmCnqEu92Fr1Mu4mxK.woff2) format('woff2');
                    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD; }
                @font-face {
                    font-family: 'Roboto';
                    font-style: normal;
                    font-weight: 700;
                    src: local('Roboto Bold'), local('Roboto-Bold'), url(https://fonts.gstatic.com/s/roboto/v18/KFOlCnqEu92Fr1MmWUlfChc4EsA.woff2) format('woff2');
                    unicode-range: U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF; }
                @font-face {
                    font-family: 'Roboto';
                    font-style: normal;
                    font-weight: 700;
                    src: local('Roboto Bold'), local('Roboto-Bold'), url(https://fonts.gstatic.com/s/roboto/v18/KFOlCnqEu92Fr1MmWUlfBBc4.woff2) format('woff2');
                    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD; }
                .o_sans, .o_heading { font-family: 'Roboto', sans-serif !important; }
                .o_heading, strong, b { font-weight: 700 !important; }
                a[x-apple-data-detectors] { color: inherit !important; text-decoration: none !important; }
                }
            </style>
            <!--[if mso]>
            <style>
                table { border-collapse: collapse; }
                .o_col { float: left; }
            </style>
            <xml>
                <o:OfficeDocumentSettings>
                <o:PixelsPerInch>96</o:PixelsPerInch>
                </o:OfficeDocumentSettings>
            </xml>
            <![endif]-->
            </head>
            <body class='o_body o_bg-white' style='width: 100%;margin: 0px;padding: 0px;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;background-color: #ffffff;'>
            <!-- preview-text -->
            <!--<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
                <tbody>
                    <tr>
                        <td class='o_hide' align='center' style='display: none;font-size: 0;max-height: 0;width: 0;line-height: 0;overflow: hidden;mso-hide: all;visibility: hidden;'>Email Summary (Hidden)</td>
                    </tr>
                </tbody>
            </table>-->
            <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
                <tbody>
                    <tr>
                        <td class='o_bg-primary o_px-md o_py-md o_sans o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;background-color: #126de5;padding-left: 24px;padding-right: 24px;padding-top: 24px;padding-bottom: 24px;'>
		                    <p style='margin-top: 0px;margin-bottom: 0px;'><a class='o_text-white' href='https://www.praisecms.com' target='_blank' style='text-decoration: none;outline: none;color: #ffffff;'><img src='{site-logo}' alt='{alt-logo}' style='max-width: 136px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a></p>
	                    </td>
                    </tr>
                </tbody>
            </table>        
            <!-- content -->
            {body_content}
            {unverify_email}
            <!-- content -->                         
            <!-- footer-light-3cols -->
            <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
                <tbody>
                <tr>
                    <td class='o_re o_bg-light o_px o_pb-lg' align='center' style='font-size: 0;vertical-align: top;background-color: #dbe5ea;padding-left: 16px;padding-right: 16px;padding-bottom: 32px;'>
                    <!--[if mso]><table cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td width='200' align='center' valign='top' style='padding:0px 8px;'><![endif]-->
                    <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 200px;'>
                        <div style='font-size: 32px; line-height: 32px; height: 32px;'>&nbsp; </div>
                        <div class='o_px-xs o_sans o_text-xs o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;text-align: center;padding-left: 8px;padding-right: 8px;'>
                            <p style='margin-top: 0px;margin-bottom: 0px;'><a class='o_text-light' href='https://app.praisecms.com/Account/Login' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><strong style='color: #82899a;'>Sign In</strong></a></p>
                        </div>
                    </div>
                    <!--[if mso]></td><td width='200' align='center' valign='top' style='padding:0px 8px;'><![endif]-->
                    <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 250px;'>
                        <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                        <div class='o_px-xs o_sans o_text-xs o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;text-align: center;padding-left: 8px;padding-right: 8px;'>
                            <p style='margin-top: 0px;margin-bottom: 0px;'>
                                {facebook_url}{twitter_url}{instagram_url}{youtube_url}{linkedin_url}
                            </p>
                        </div>
                    </div>
                    <!--[if mso]></td><td width='200' align='center' valign='top' style='padding:0px 8px;'><![endif]-->
                    <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 200px;'>
                        <div style='font-size: 32px; line-height: 32px; height: 32px;'>&nbsp; </div>
                        <div class='o_px-xs o_sans o_text-xs o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;text-align: center;padding-left: 8px;padding-right: 8px;'>
                            <!--<p style='margin-top: 0px;margin-bottom: 0px;'><a class='o_text-light' href='https://praisecms.com/helpcenter' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><strong style='color: #82899a;'>Help Center</strong></a></p>-->
                        </div>
                    </div>
                    <!--[if mso]></td></tr></table><![endif]-->
                    </td>
                </tr>
                <tr>
                    <td class='o_re o_bg-light o_px-md o_pb-lg' align='center' style='font-size: 0;vertical-align: top;background-color: #dbe5ea;padding-left: 24px;padding-right: 24px;padding-bottom: 32px;'>
                        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
                        <div class='o_col-6s o_sans o_text-xs o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;max-width: 584px;color: #82899a;text-align: center;'>
                            <!--<p class='o_mb' style='margin-top: 0px;margin-bottom: 16px;'>
                                <a class='o_text-light' href='https://www.apple.com/app-store/' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/badge_appstore-light.png' width='135' height='56' alt='AppStore' style='max-width: 135px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>
                                <a class='o_text-light' href='https://play.google.com/store/' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/badge_googleplay-light.png' width='135' height='56' alt='Google Play' style='max-width: 135px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>
                            </p>-->
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>{church-name}<br>
                                {church-address}
                            </p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>Powered by <a href='https://www.praisecms.com' target='_blank'>Praise Church Management Solutions</a></p>
                            <p style='margin-top: 0px;margin-bottom: 0px;'>
                                <!-- <a class='o_text-xxs o_text-light o_underline' href='https://example.com/' target='_blank' style='text-decoration: underline;outline: none;font-size: 12px;line-height: 19px;color: #82899a;'>Unsubscribe</a>-->
                            </p>
                        </div>
                        <div class='o_hide-xs' style='font-size: 64px; line-height: 64px; height: 64px;'>&nbsp; </div>
                        <!--[if mso]></td></tr></table><![endif]-->
                    </td>
                </tr>
                </tbody>
            </table>
            </body>
        </html>";

        public const string UnverifyEmail = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
	        <tr>
	            <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
		        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
		            <tbody>
					<tr>
					    <td style='text-align:center;line-height: 30px;'>						 
						<td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color:#ffffff ;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
						<!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
						<div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
							<p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;color: #242b3d;'><b>Verify your email address</b><br>
							Please confirm that we have your email correct by clicking the button below.</p>
							<table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
							<tbody>
								<tr>
								<td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
									<a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Verify Email</a>
								</td>
								</tr>
							</tbody>
							</table>		  
						</div>
						</td>										
						</td>
					</tr>	   
			        </tbody>
		        </table>
	            </td>
	        </tr>
            </tbody>
        </table>";

        public const string VerifyEmail_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
								        <!--<h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Welcome to Praise CMS</h2>-->
								        <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>It looks like your email hasn't been verified yet.</p>   
									        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
										        <tbody>
											        <tr>
												        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
													        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Verify Email</a>
												        </td>
											        </tr>
										        </tbody>
									        </table>		  
								        </div>
							        </td>
						        </tr>	   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string EmailUpdate_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
								        <!--<h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Welcome to Praise CMS</h2>-->
								        <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 16px;'>Your email has been updated.</p>
								        <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>Please use this email to access your account.</p>
									        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
										        <tbody>
											        <tr>
												        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
													        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Sign In</a>
												        </td>
											        </tr>
										        </tbody>
									        </table>		  
								        </div>
							        </td>
						        </tr>	   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string NewUserAccount_body = @" <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
	        <tr>
	            <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
		        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
		            <tbody>
					<tr>
					    <td style='text-align:center;line-height: 30px;'>
						<td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
						<!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
						<div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
							<h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Welcome to Praise CMS</h2>
							<p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>An account has been created for you by {createdBy}.<br/><br/>
							Here is your username to access your account.<br/></p>
							<table style='margin-bottom: 40px;' align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
							<tbody>
								<tr>
								<td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='color: #242b3d; font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 32px;mso-padding-alt: 12px 24px;background-color: #ebf5fa;border-radius: 4px;'>
									<p style='margin:8px;'><span>Username: </span><span style='font-weight:normal;'><a href='mailto:' style='color:initial;cursor:initial;'>{username}</a></span>
						        <br/>							        
								</td>
								</tr>
							</tbody>
							</table>	
							<table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
							<tbody>
								<tr>
								<td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;width: 195px;'>
									<a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Sign In</a>
								</td>
								</tr>
							</tbody>
							</table>		  
						</div>
						</td>										
						</td>
					</tr>	  
		            </tbody>
		        </table>
	            </td>
	        </tr>
            </tbody>
        </table>";

        public const string ChurchRegistration_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Welcome to Praise CMS</h2>
									        <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>{message}</p>   
									        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
										        <tbody>
											        <tr>
												        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
													        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Sign In</a>
												        </td>
											        </tr>
										        </tbody>
									        </table>		  
								        </div>
							        </td>										
						        </tr>	   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string ChurchRegistrationSuperAdmin_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <!--<h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Welcome to Praise CMS</h2>-->
									        <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>{churchName} signed up for Praise CMS on: {created_datetime}.<br>Phone: <a href='tel:1{phone}'>{phone}</a> <br>Email: <a href='mailto:{email}'>{email}</a><br>Address: {church-address} <br>Administrator: {church-admin}</p>             
								        </div>          
							        </td>					
						        </tr>	   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string ForgotPassword_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-ultra_light o_px-md o_py-xl o_xs-py-md' align='center'>
				        <table style='width: 100%; background-color: #242b3d; ' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td align='center'>
								        <div class='o_col-6s o_sans o_text-md o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 15px;margin-bottom: 40px;padding-top:30px;font-size: 19px;line-height: 28px;max-width: 584px;color: white;text-align: center;'>              
									        <table class='o_center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: center;margin-left: auto;margin-right: auto;'>
										        <tbody>
											        <tr>
												        <td class='o_sans o_text o_text-white o_bg-primary o_px o_py o_br-max' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;background-color: #126de5;color: #ffffff;border-radius: 96px;padding-left: 16px;padding-right: 16px;padding-top: 16px;padding-bottom: 16px;'>
													        <img src='{site-url}/Content/assets/image/email_templates/vpn_key-48-white.png' width='48' height='48' alt='' style='max-width: 48px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'>		
												        </td>
											        </tr>
											        <tr>
												        <td style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </td>
											        </tr>
										        </tbody>
									        </table>
									        <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: white;font-size: 30px;line-height: 39px;'>Forgot Your Password?</h2>
									        <p style='margin-top: 0px;margin-bottom: 0px;'>It looks like you lost your password. We're here to help. Click the button below to change your password.</p>
								        </div>
								        <table style='margin: 50px;' align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
									        <tbody>
										        <tr>
											        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
												        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Reset Your Password</a>
											        </td>
										        </tr>
									        </tbody>
								        </table>
							        </td>
						        </tr>
					        </tbody> 				
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string ResetPassword_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-ultra_light o_px-md o_py-xl o_xs-py-md' align='center'>
				        <table style='width: 100%; background-color: #242b3d; ' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td align='center'>
								        <div class='o_col-6s o_sans o_text-md o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 15px;margin-bottom: 40px;padding-top:30px;font-size: 19px;line-height: 28px;max-width: 584px;color: white;text-align: center;'>              
									        <table class='o_center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: center;margin-left: auto;margin-right: auto;'>
										        <tbody>
											        <tr>
												        <td class='o_sans o_text o_text-white o_bg-primary o_px o_py o_br-max' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;background-color: #126de5;color: #ffffff;border-radius: 96px;padding-left: 16px;padding-right: 16px;padding-top: 16px;padding-bottom: 16px;'>
													        <img src='{site-url}/Content/assets/image/email_templates/vpn_key-48-white.png' width='48' height='48' alt='' style='max-width: 48px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'>		
												        </td>
											        </tr>
											        <tr>
												        <td style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </td>
											        </tr>
										        </tbody>
									        </table>
									        <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: white;font-size: 30px;line-height: 39px;'>Password Reset</h2>
									        <p style='margin-top: 0px;margin-bottom: 0px;'>We received a request to reset your password to Praise CMS. Please use the link below to reset it and sign in.</p>
								        </div>
								        <table style='margin: 50px;' align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
									        <tbody>
										        <tr>
											        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
												        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Reset Your Password</a>
											        </td>
										        </tr>
									        </tbody>
								        </table>
							        </td>
						        </tr>
					        </tbody> 				
				        </table> 
				        <!-- <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-white o_px-md o_py' align='center' style='background-color: #ffffff;padding-left: 24px;padding-right: 24px;padding-top: 16px;padding-bottom: 16px;'>
								        <div class='o_col-6s o_sans o_text o_text-secondary o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;max-width: 584px;color: #424651;text-align: center;'>
									        <p style='margin-top: 0px;margin-bottom: 0px;'>Your temporary password is:<br/> <b> {temp_password} </b></p>
								        </div>           
							        </td>
						        </tr>
					        </tbody>
				        </table>-->  
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string PasswordChanged_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' cellspacing='0'  style='width: 100%;' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <table class='o_center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: center;margin-left: auto;margin-right: auto;'>
										        <tbody>
											        <tr>
												        <td class='o_sans o_text o_text-white o_bg-primary o_px o_py o_br-max' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;background-color: #126de5;color: #ffffff;border-radius: 96px;padding-left: 16px;padding-right: 16px;padding-top: 16px;padding-bottom: 16px;'>
													        <img src='{site-url}/Content/assets/image/email_templates/vpn_key-48-white.png' width='48' height='48' alt='' style='max-width: 48px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'>		
												        </td>
											        </tr>
											        <tr>
												        <td style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </td>
											        </tr>
										        </tbody>
									        </table>		
									        <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: white;font-size: 30px;line-height: 39px;'>Password Changed</h2>
									        <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'> Your password was recently changed.  If you believe this was done in error, please <a href='https://www.praisecms.com/contact' target='_blank'>contact us</a>.</p>
									        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
										        <tbody>
											        <tr>
												        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
													        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Sign In</a>
												        </td>
											        </tr>
										        </tbody>
									        </table>		  
								        </div>
							        </td>										
						        </tr>		  
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string PaymentProcessed_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>					     						 
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Hi {user_display},</h2>
									        <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'> A gift was made on {gift_datetime} to {church_display}.</p>
								        </div>
								        <!--[if mso]></td><td width='200' align='right' valign='top' style='padding: 0px 8px;'><![endif]-->
								        <!--[if mso]><table cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td width='400' align='center' valign='top' style='padding: 0px 8px;'><![endif]-->
								        <div class='o_col o_col-4 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 400px;'>
									        <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
									        <div class='o_px-xs o_sans o_text o_text-white o_left o_xs-center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #ffffff;text-align: left;padding-left: 8px;padding-right: 8px;'>
										        <p style='font-size:16px;line-height:24px;/* color:#126de5; */margin-top:0px;margin-bottom:0px;'><strong>{church_display}</strong> <span style='font-size:12px;line-height:19px;'>{campus_display}</span></p>
										        <p style='margin-top: 0px;margin-bottom: 0px;font-size: 12px;'>{fund_display}</p>
										        <p style='margin-top: 0px;margin-bottom: 0px;font-size: 12px;'>{paymentmethod}</p>
										        <p style='margin-top: 0px;margin-bottom: 0px;font-size: 12px;'>TransactionId: {transactionid}</p>
									        </div>
								        </div>
								        <!--[if mso]></td><td width='200' align='right' valign='top' style='padding: 0px 8px;'><![endif]-->
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 200px;'>
									        <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
										        <div class='o_px-xs o_sans o_text o_text-white o_right o_xs-center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #ffffff;text-align: right;padding-left: 8px;padding-right: 8px;'>
										        <h3 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 24px;line-height: 31px;'>{amount}</h3>
									        </div>
								        </div>
								        <!--[if mso]></td></tr></table><![endif]-->
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 40px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
								        <!-- <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>{church_thanks_note}</p> -->
									        <div class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>{church_thanks_note}</div>
								        </div>
								        <!--[if mso]></td><td width='200' align='right' valign='top' style='padding: 0px 8px;'><![endif]-->				
							        </td>										
						        </tr>
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string SubscriptionCancelled_Body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
                <div class='o_col-6s o_sans o_text-md o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
                    <table class='o_center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: center;margin-left: auto;margin-right: auto;'>
                    <tbody>
                        <tr>
                        <td class='o_sans o_text o_text-secondary o_b-primary o_px o_py o_br-max' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #424651;border: 2px solid #126de5;border-radius: 96px;padding-left: 16px;padding-right: 16px;padding-top: 16px;padding-bottom: 16px;background-color: #ffffff;'>
                            <img src='{site-url}/Content/assets/image/email_templates/cancel-48-primary.png' width='48' height='48' alt='' style='max-width: 48px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'>
                        </td>
                        </tr>
                        <tr>
                        <td style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </td>
                        </tr>
                    </tbody>
                    </table>
                    <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: #ffffff;font-size: 30px;line-height: 39px;'>Subscription Canceled</h2>
                    <p style='margin-top: 0px;margin-bottom: 0px;'>The Praise CMS subscription for {church_name} has been canceled. You will continue to have access until your current plan expired. You may renew your subscription at any time.</p>
                </div>
                </td>
            </tr>
            </tbody>
        </table>
        <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white' align='center' style='background-color: #242b3d;'>
                <table class='o_block' width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation' style='max-width: 632px;margin: 0 auto;'>
                    <tbody>
                    <tr>
                        <td class='o_re o_bg-ultra_light o_px o_pb-md' align='center' style='font-size: 0;vertical-align: top;background-color: #ebf5fa;padding-left: 16px;padding-right: 16px;padding-bottom: 24px;'>
                        <div class='o_col o_col-3' style='display: inline-block;vertical-align: top;width: 100%;max-width: 300px;'>
                            <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                            <div class='o_px-xs o_sans o_text o_text-secondary o_left o_xs-center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #424651;text-align: left;padding-left: 8px;padding-right: 8px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>{subscription_type} Account</strong> <span class='o_text-xxs' style='font-size: 12px;line-height: 19px;'>({subscription_period})</span></p>
                            <p class='o_text-xs' style='font-size: 14px;line-height: 21px;margin-top: 0px;margin-bottom: 0px;'>{expiry_date}</p>
                            </div>
                        </div>
                        <div class='o_col o_col-3' style='display: inline-block;vertical-align: top;width: 100%;max-width: 300px;'>
                            <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                            <div class='o_px-xs o_right o_xs-center' style='text-align: right;padding-left: 8px;padding-right: 8px;'>
                            <table class='o_right o_xs-center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: right;margin-left: auto;margin-right: 0;'>
                                <tbody>
                                <tr>
                                    <td class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
                                    <a class='o_text-white' href='{renew_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Renew Subscription</a>
                                    </td>
                                </tr>
                                </tbody>
                            </table>
                        </div>
                        </div>
                        </td>
                    </tr>
                    </tbody>
                </table>
                </td>
            </tr>
            </tbody>
        </table>
        <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_re o_bg-white o_px o_pb-md' align='center' style='font-size: 0;vertical-align: top;background-color: #242b3d;padding-left: 16px;padding-right: 16px;padding-bottom: 24px;'>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-dark o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Basic</strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>$0.00</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Limited Reports</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Limited Giving Funds</p>
						        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Digital Giving (Cards &amp; ACH)</p>
						            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
							            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
								        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
                            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'><span>&nbsp; </span></p>
                        
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-primary o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Premium <small>(Monthly)</small></strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>{monthly_price}</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● All Basic plan Features</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Offline Giving (Cash &amp; Checks)</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Robust Reporting &amp; Analytics</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Prayer Requests</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Unlimited Giving Funds</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Church Family Management</p>
				            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>● Priority Support</p>
                        
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-primary o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Premium <small>(Annually)</small></strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>{annually_price}</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● All Basic plan Features</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Offline Giving (Cash &amp; Checks)</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Robust Reporting &amp; Analytics</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Prayer Requests</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Unlimited Giving Funds</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Church Family Management</p>
				            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>● Priority Support</p>                      
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                </td>
            </tr>
            </tbody>
        </table>
        <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white o_px-md o_py' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 16px;padding-bottom: 16px;'>
                <div class='o_col-6s o_sans o_text o_text-secondary o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;max-width: 584px;color: #ffffff;text-align: center;'>
                    <h4 class='o_heading o_text-dark o_mb-xs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 8px;color: #ffffff;font-size: 18px;line-height: 23px;'>Much and soulfully forward</h4>
                </div>
                </td>
            </tr>
            </tbody>
        </table>";

        public const string subscription_Renewal_Body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 30px;'>
                <div class='o_col-6s o_sans o_text-md o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #82899a;text-align: center;'>
                    <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: white;font-size: 30px;line-height: 39px;'>Subscription Renewal</h2>
                    <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;color: white;'>Your subscription has been renewed.</p>
                </div>
                </td>
            </tr>
            </tbody>
            </table>
            <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
                <tbody>
                <tr>
                    <td class='o_bg-white' align='center' style='background-color: #242b3d;'>
                    <table class='o_block' width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation' style='max-width: 632px;margin: 0 auto;'>
                        <tbody>
                        <tr>
                            <td class='o_re o_bg-ultra_light o_px o_pb-md' align='center' style='font-size: 0;vertical-align: top;background-color: #ebf5fa;padding-left: 16px;padding-right: 16px;padding-bottom: 24px;'>
                            <div class='o_col o_col-3' style='display: inline-block;vertical-align: top;width: 100%;max-width: 300px;'>
                                <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                                <div class='o_px-xs o_sans o_text o_text-secondary o_left o_xs-center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #424651;text-align: left;padding-left: 8px;padding-right: 8px;'>
                                <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>{subscription_type} Account</strong> <span class='o_text-xxs' style='font-size: 15px;line-height: 19px;'>({subscription_period})</span></p>
                                <p class='o_text-xs' style='font-size: 14px;line-height: 21px;margin-top: 0px;margin-bottom: 0px;'>{expiry_date}</p>
                                </div>
                            </div>
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </td>
                </tr>
                </tbody>
            </table>
            <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_re o_bg-white o_px o_pb-md' align='center' style='font-size: 0;vertical-align: top;background-color: #242b3d;padding-left: 16px;padding-right: 16px;padding-bottom: 24px;'>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-dark o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Basic</strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>$0.00</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Limited Reports</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Limited Giving Funds</p>
						        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Digital Giving (Cards &amp; ACH)</p>
						            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
							            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
								        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
                            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'><span>&nbsp; </span></p>
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-primary o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Premium <small>(Monthly)</small></strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>{monthly_price}</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● All Basic plan Features</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Offline Giving (Cash &amp; Checks)</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Robust Reporting &amp; Analytics</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Prayer Requests</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Unlimited Giving Funds</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Church Family Management</p>
				            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>● Priority Support</p>
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-primary o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Premium <small>(Annually)</small></strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>{annually_price}</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● All Basic plan Features</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Offline Giving (Cash &amp; Checks)</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Robust Reporting &amp; Analytics</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Prayer Requests</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Unlimited Giving Funds</p>
					        <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Church Family Management</p>
				            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>● Priority Support</p>
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                </td>
            </tr>
            </tbody>
            </table>
            <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
                <tbody>
                <tr>
                    <td class='o_bg-white o_px-md o_py' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 16px;padding-bottom: 16px;'>
                    <div class='o_col-6s o_sans o_text o_text-secondary o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;max-width: 584px;color: white;text-align: center;'></div>
                    </td>
                </tr>
                </tbody>
        </table>";

        public const string SubscriptionPlanChange_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
                <div class='o_col-6s o_sans o_text-md o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
                    <table class='o_center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: center;margin-left: auto;margin-right: auto;'>
                    <tbody>
                        <tr>
                        <td class='o_sans o_text o_text-secondary o_b-primary o_px o_py o_br-max' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #424651;border: 2px solid #126de5;border-radius: 96px;padding-left: 16px;padding-right: 16px;padding-top: 16px;padding-bottom: 16px;background-color: #ffffff;'>
                            <img src='{site-url}/Content/assets/image/email_templates/flag-48-primary.png' width='48' height='48' alt='' style='max-width: 48px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'>
                        </td>
                        </tr>
                        <tr>
                        <td style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </td>
                        </tr>
                    </tbody>
                    </table>
                    <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: #ffffff;font-size: 30px;line-height: 39px;'>Subscription Updated</h2>
                    <p style='margin-top: 0px;margin-bottom: 0px;'>The Praise subscription for {church_name} was recently changed to the {plan_name} plan. This change will go into effect once the current plan ends.</p>
                </div>
                </td>
            </tr>
            </tbody>
        </table>
        <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white' align='center' style='background-color: #242b3d;'>
                <table class='o_block' width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation' style='max-width: 632px;margin: 0 auto;'>
                    <tbody>
                    <tr>
                        <td class='o_re o_bg-ultra_light o_px o_pb-md' align='center' style='font-size: 0;vertical-align: top;background-color: #ebf5fa;padding-left: 16px;padding-right: 16px;padding-bottom: 24px;'>
                        <div class='o_col o_col-3' style='display: inline-block;vertical-align: top;width: 100%;max-width: 300px;'>
                            <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                            <div class='o_px-xs o_sans o_text o_text-secondary o_left o_xs-center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;color: #424651;text-align: left;padding-left: 8px;padding-right: 8px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>{subscription_type} Account</strong> <span class='o_text-xxs' style='font-size: 12px;line-height: 19px;'>{subscription_period}</span></p>
                            <p class='o_text-xs' style='font-size: 14px;line-height:21px;margin-top:0; margin-bottom: 0;'>{expiry_period}</p>
                            </div>
                        </div>
                        <div class='o_col o_col-3' style='display: inline-block;vertical-align: top;width: 100%;max-width: 300px;'>
                            <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                            <div class='o_px-xs o_right o_xs-center' style='text-align: right;padding-left: 8px;padding-right: 8px;'>
                            <table class='o_right o_xs-center' cellspacing='0' cellpadding='0' border='0' role='presentation' style='text-align: right;margin-left: auto;margin-right: 0;'>
                                <tbody>
                                <tr>
                                    <td class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
                                    <a class='o_text-white' href='{renew_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Review Subscription</a>
                                    </td>
                                </tr>
                                </tbody>
                            </table>
                        </div>
                        </div>
                        </td>
                    </tr>
                    </tbody>
                </table>
                </td>
            </tr>
            </tbody>
        </table>
        <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_bg-white o_px-md o_py' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 40px;padding-bottom: 16px;'>
                <div class='o_col-6s o_sans o_text o_text-secondary o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;max-width: 584px;color: #ffffff;text-align: center;'></div>
                </td>
            </tr>
            </tbody>
        </table>
        <table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
            <tr>
                <td class='o_re o_bg-white o_px o_pb-md' align='center' style='font-size: 0;vertical-align: top;background-color: #242b3d;padding-left: 16px;padding-right: 16px;padding-bottom: 24px;'>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-dark o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Basic</strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>$0.00</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Limited Reports</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Limited Giving Funds</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Digital Giving (Cards &amp; ACH)</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
							<p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
							<p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'><span>&nbsp; </span></p>
                            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'><span>&nbsp; </span></p>
                        
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-primary o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Premium <small>(Annually)</small></strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>{annually_price}</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● All Basic plan Features</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Offline Giving (Cash &amp; Checks)</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Robust Reporting &amp; Analytics</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Prayer Requests</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Unlimited Giving Funds</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Church Family Management</p>
				            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>● Priority Support</p>                      
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
                <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 260px;'>
                    <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
                    <div class='o_px-xs' style='padding-left: 8px;padding-right: 8px;'>
                    <table width='100%' role='presentation' cellspacing='0' cellpadding='0' border='0'>
                        <tbody>
                        <tr>
                            <td class='o_bg-primary o_br o_px o_py-lg o_sans o_text-xs o_text-white' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;background-color: #126de5;color: #ffffff;border-radius: 4px;padding-left: 16px;padding-right: 16px;padding-top: 32px;padding-bottom: 5px;'>
                            <p class='o_mb-xxs' style='margin-top: 0px;margin-bottom: 4px;'><strong>Premium <small>(Monthly)</small></strong></p>
                            <h2 class='o_heading' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 30px;line-height: 39px;'>{monthly_price}</h2>
                            <p class='o_text-xxs o_mb-md' style='font-size: 15px;line-height: 19px;margin-top: 0px;margin-bottom: 24px;'>/month</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● All Basic plan Features</p>
                            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Offline Giving (Cash &amp; Checks)</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Robust Reporting &amp; Analytics</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Prayer Requests</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Unlimited Giving Funds</p>
						    <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>● Church Family Management</p>
				            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>● Priority Support</p>
                        
                            </td>
                        </tr>
                        </tbody>
                    </table>
                    </div>
                </div>                                   
                </td>
            </tr>
            </tbody>
        </table>";

        public const string General_With_Heading = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>					     						 
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 70%;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Hi {user_display},</h2>
									        <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>{message}</p>
								        </div>								     		
							        </td>										
						        </tr>
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string General = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>					     						 
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 70%;color: #ffffff;text-align: center;'>
									        <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>{message}</p>
								        </div>								     		
							        </td>										
						        </tr>
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string ScheduledGiving_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;'>
				        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Hi {user_display},</h2>
									        <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>A scheduled gift was created for {church_display} in the amount of {amount}. The first {frequency} donation will start on {start_date}.<br />{fund_display} | {paymentmethod}</p>
								        </div>
							        </td>										
						        </tr>		   			           
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string Payment_Error_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_text-dark o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;color: white;font-size: 30px;line-height: 39px;'>Payment Error</h2>									        
                                            <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'> There was a problem processing your gift to {church_display}.<br />Error Message: <b>{error_message}</b><br />{fund_display} | {paymentmethod}</p>
								        </div>
							        </td>
						        </tr>		   		   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string AnnualGivingStatement_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>{body}</p>   
									        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
										        <tbody>
											        <tr>
												        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
													        <a class='o_text-white' href='{view_statment_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>View Statement</a>
												        </td>
											        </tr>
										        </tbody>
									        </table>	  
								        </div>
							        </td>										
						        </tr>		   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string PaymentMethod_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>					     						 
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Hi {user_display},</h2>
									        <p class='o_mb-md' style='line-height: 40px;margin-top: 0px;margin-bottom: 24px;'>{message}</p>
								        </div>								         					      		
							        </td>										
						        </tr>
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string Plain = @"<!doctype html>
        <html>
            <head>
            <meta name='viewport' content='width=device-width'>
            <meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>
            <title>Email</title>
            <style media='all' type='text/css'>
            @media only screen and (max-width: 620px) {
                .span-2,
                .span-3 {
                max-width: none !important;
                width: 100% !important;
                }
                .span-2 > table,
                .span-3 > table {
                max-width: 100% !important;
                width: 100% !important;
                }
            }
    
            @media all {
                .btn-primary table td:hover {
                background-color: #34495e !important;
                }
                .btn-primary a:hover {
                background-color: #34495e !important;
                border-color: #34495e !important;
                }
            }
    
            @media all {
                .btn-secondary a:hover {
                border-color: #34495e !important;
                color: #34495e !important;
                }
            }
            @media only screen and (max-width: 620px) {
                h1 {
                font-size: 28px !important;
                margin-bottom: 10px !important;
                }
                h2 {
                font-size: 22px !important;
                margin-bottom: 10px !important;
                }
                h3 {
                font-size: 16px !important;
                margin-bottom: 10px !important;
                }
                .main p,
                .main ul,
                .main ol,
                .main td,
                .main span {
                font-size: 16px !important;
                }
                .wrapper {
                padding: 10px !important;
                }
                .article {
                padding-left: 0 !important;
                padding-right: 0 !important;
                }
                .content {
                padding: 0 !important;
                }
                .container {
                padding: 0 !important;
                width: 100% !important;
                }
                .header {
                margin-bottom: 10px !important;
                }
                .main {
                border-left-width: 0 !important;
                border-radius: 0 !important;
                border-right-width: 0 !important;
                }
                .btn table {
                max-width: 100% !important;
                width: 100% !important;
                }
                .btn a {
                max-width: 100% !important;
                padding: 12px 5px !important;
                width: 100% !important;
                }
                .img-responsive {
                height: auto !important;
                max-width: 100% !important;
                width: auto !important;
                }
                .alert td {
                border-radius: 0 !important;
                padding: 10px !important;
                }
                .receipt {
                width: 100% !important;
                }
                hr {
                Margin-bottom: 10px !important;
                Margin-top: 10px !important;
                }
                .hr tr:first-of-type td,
                .hr tr:last-of-type td {
                height: 10px !important;
                line-height: 10px !important;
                }
            }    
            @media all {
                .ExternalClass {
                width: 100%;
                }
                .ExternalClass,
                .ExternalClass p,
                .ExternalClass span,
                .ExternalClass font,
                .ExternalClass td,
                .ExternalClass div {
                line-height: 100%;
                }
                .apple-link a {
                color: inherit !important;
                font-family: inherit !important;
                font-size: inherit !important;
                font-weight: inherit !important;
                line-height: inherit !important;
                text-decoration: none !important;
                }
            }
            </style>

            <!--[if gte mso 9]>
            <xml>
            <o:OfficeDocumentSettings>
            <o:AllowPNG/>
            <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
        <![endif]-->
            </head>
            <body style='font-family: sans-serif; -webkit-font-smoothing: antialiased; font-size: 14px; line-height: 1.4; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; background-color: #f6f6f6; margin: 0; padding: 0;'>
            <table border='0' cellpadding='0' cellspacing='0' class='body' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #f6f6f6;' width='100%' bgcolor='#f6f6f6'>
                <tr>
                <td style='font-family: sans-serif; font-size: 14px; vertical-align: top;' valign='top'>&nbsp;</td>
                <td class='container' style='font-family: sans-serif; font-size: 14px; vertical-align: top; Margin: 0 auto !important; max-width: 580px; padding: 10px; width: 580px;' width='580' valign='top'>
                    <div class='content' style='box-sizing: border-box; display: block; Margin: 0 auto; max-width: 580px; padding: 10px;'>

                    <!-- START HEADER -->
                    <div class='header' style='margin-bottom: 20px; Margin-top: 10px; width: 100%;'>
                        <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; min-width: 100%;' width='100%'>
                        <tr>
                            <td class='align-center' style='font-family: sans-serif; font-size: 14px; vertical-align: top; text-align: center;' valign='top' align='center'>
                            <a href='{site-url}' target='_blank' style='color: #3498db; text-decoration: underline;'><img src='{site-logo}' alt='' align='center' style='border: none; -ms-interpolation-mode: bicubic; max-width: 100%;'></a>
                            </td>
                        </tr>
                        </table>
                    </div>
                    <!-- END HEADER -->
                    <table class='main' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background: #fff; border-radius: 3px;' width='100%'>

                        <!-- START MAIN CONTENT AREA -->
                        <tr>
                        <td class='wrapper' style='font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;' valign='top'>
                            <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;' width='100%'>
                            <tr>
                                <td style='font-family: sans-serif; font-size: 14px; vertical-align: top;' valign='top'>
                                <p style='font-family: sans-serif; font-size: 14px; font-weight: normal; Margin: 0; Margin-bottom: 15px;'>
                                    {text}
                                </p>
                                </td>
                            </tr>
                            </table>
                        </td>
                        </tr>

                        <!-- END MAIN CONTENT AREA -->
                        </table>
                    <!-- START FOOTER -->
                    <div class='footer' style='clear: both; padding-top: 10px; text-align: center; width: 100%;'>
                        <table border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;' width='100%'>
                        <tr>
                            <td class='content-block' style='font-family: sans-serif; vertical-align: top; padding-top: 10px; padding-bottom: 10px; font-size: 12px; color: #999999; text-align: center;' valign='top' align='center'>
                            <span class='apple-link' style='color: #999999; font-size: 12px; text-align: center;'>{site-description}</span>
                            <br />Powered by <a href='{site-url}' style='color: #999999; font-size: 12px; text-align: center; text-decoration: none;'>{site-name}</a>
                            </td>
                        </tr>
                        </table>
                    </div>
                    <!-- END FOOTER -->
                <!-- END CENTERED WHITE CONTAINER --></div>
                </td>
                <td style='font-family: sans-serif; font-size: 14px; vertical-align: top;' valign='top'>&nbsp;</td>
                </tr>
            </table>
            </body>
        </html>";

        public const string CallToActionButton = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
        <tbody>
	        <tr>
	            <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;padding-left: 24px;padding-right: 24px;padding-top: 8px;padding-bottom: 8px;'>
		        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
		            <tbody>
			        <tr>
			            <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
				        <a class='o_text-white' href='{CallToActionButton_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>{CallToActionButton_text}</a>
			            </td>
			        </tr>
		            </tbody>
		        </table>
	            </td>
	        </tr>
            </tbody>
        </table>";

        public const string HeaderText = "<h1 style='color: #222222; font-family: sans-serif; font-weight: 300; line-height: 1.4; margin: 0; Margin-bottom: 30px; font-size: 35px; text-align: center; text-transform: capitalize;'>{text}</h1>";

        public const string Header = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
	        <tr>
	            <td class='o_bg-primary o_px-md o_py-md o_sans o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;background-color: #126de5;padding-left: 24px;padding-right: 24px;padding-top: 24px;padding-bottom: 24px;'>
		        <p style='margin-top: 0px;margin-bottom: 0px;'><a class='o_text-white' href='https://praisecms.com' target='_blank' style='text-decoration: none;outline: none;color: #ffffff;'><img src='{site-logo}' width='136' height='36' alt='SimpleApp' style='max-width: 136px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a></p>
	            </td>
	        </tr>
            </tbody>
        </table>";

        public const string Footer = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
            <tbody>
	        <tr>
	            <td class='o_re o_bg-light o_px o_pb-lg' align='center' style='font-size: 0;vertical-align: top;background-color: #dbe5ea;padding-left: 16px;padding-right: 16px;padding-bottom: 32px;'>
		        <!--[if mso]><table cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td width='200' align='center' valign='top' style='padding:0px 8px;'><![endif]-->
		        <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 200px;'>
		            <div style='font-size: 32px; line-height: 32px; height: 32px;'>&nbsp; </div>
		            <div class='o_px-xs o_sans o_text-xs o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;text-align: center;padding-left: 8px;padding-right: 8px;'>
			        <p style='margin-top: 0px;margin-bottom: 0px;'><a class='o_text-light' href='https://app.praisecms.com/Account/Login' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><strong style='color: #82899a;'>Sign In</strong></a></p>
		            </div>
		        </div>
		        <!--[if mso]></td><td width='200' align='center' valign='top' style='padding:0px 8px;'><![endif]-->
		        <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 200px;'>
		            <div style='font-size: 24px; line-height: 24px; height: 24px;'>&nbsp; </div>
		            <div class='o_px-xs o_sans o_text-xs o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;text-align: center;padding-left: 8px;padding-right: 8px;'>
			        <p style='margin-top: 0px;margin-bottom: 0px;'>
			            <a class='o_text-light' href='https://facebook.com/praisecms' style='text-decoration: none;outline: none;color: #82899a;'><img src='{facebook-logo}' width='36' height='36' alt='fb' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>
			            <a class='o_text-light' href='https://twitter.com/praisecms' style='text-decoration: none;outline: none;color: #82899a;'><img src='{twitter-logo}' width='36' height='36' alt='tw' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>
			            <a class='o_text-light' href='https://instagram.com/praisecms' style='text-decoration: none;outline: none;color: #82899a;'><img src='{instagram-logo}' width='36' height='36' alt='ig' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>
			            <!--<a class='o_text-light' href='https://youtube.com' style='text-decoration: none;outline: none;color: #82899a;'><img src='{youtube-logo}' width='36' height='36' alt='sc' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>-->
                        <a class='o_text-light' href='https://linkedin.com/company/praise-church-management-solutions' style='text-decoration: none;outline: none;color: #82899a;'><img src='{linkedin-logo}' width='36' height='36' alt='ig' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>
			        </p>
		            </div>
		        </div>
		        <!--[if mso]></td><td width='200' align='center' valign='top' style='padding:0px 8px;'><![endif]-->
		        <div class='o_col o_col-2 o_col-full' style='display: inline-block;vertical-align: top;width: 100%;max-width: 200px;'>
		            <div style='font-size: 32px; line-height: 32px; height: 32px;'>&nbsp; </div>
		            <div class='o_px-xs o_sans o_text-xs o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;text-align: center;padding-left: 8px;padding-right: 8px;'>
                    <!--<p style='margin-top: 0px;margin-bottom: 0px;'><a class='o_text-light' href='https://praisecms.com/helpcenter' style='text-decoration: none;outline: none;color: #82899a;'><strong style='color: #82899a;'>Help Center</strong></a></p>-->
		            </div>
		        </div>
		        <!--[if mso]></td></tr></table><![endif]-->
	            </td>
	        </tr>
	        <tr>
	            <td class='o_re o_bg-light o_px-md o_pb-lg' align='center' style='font-size: 0;vertical-align: top;background-color: #dbe5ea;padding-left: 24px;padding-right: 24px;padding-bottom: 32px;'>
		        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
		        <div class='o_col-6s o_sans o_text-xs o_text-light o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 14px;line-height: 21px;max-width: 584px;color: #82899a;text-align: center;'>
		            <!--<p class='o_mb' style='margin-top: 0px;margin-bottom: 16px;'>
			        <a class='o_text-light' href='https://example.com/' style='text-decoration: none;outline: none;color: #82899a;'><img src='images/badge_appstore-light.png' width='135' height='56' alt='AppStore' style='max-width: 135px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>
			        <a class='o_text-light' href='https://example.com/' style='text-decoration: none;outline: none;color: #82899a;'><img src='images/badge_googleplay-light.png' width='135' height='56' alt='Google Play' style='max-width: 135px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>
		            </p>-->
		            <p class='o_mb-xs' style='margin-top: 0px;margin-bottom: 8px;'>©{current-year} Praise, LLC<br>
			        {praise-address}
		            </p>
		            <p style='margin-top: 0px;margin-bottom: 0px;'>
			        <a class='o_text-xxs o_text-light o_underline' href='https://example.com/' style='text-decoration: underline;outline: none;font-size: 12px;line-height: 19px;color: #82899a;'>Unsubscribe</a>
		            </p>
		        </div>
		        <div class='o_hide-xs' style='font-size: 64px; line-height: 64px; height: 64px;'>&nbsp; </div>
		        <!--[if mso]></td></tr></table><![endif]-->
	            </td>
	        </tr>
            </tbody>
        </table>";

        public const string VerificationCode_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Verification Code</h2>									       
                                            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>Here is your one-time code: {verification-code}.<br/>Please use this to access your account.</p>        
								        </div>
							        </td>										
						        </tr>	   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";

        public const string InvitationEmail_body = @"<table width='100%' cellspacing='0' cellpadding='0' border='0' role='presentation'>
	        <tbody>
		        <tr>
			        <td class='o_bg-white o_px-md o_py-xs' align='center' style='background-color: #ffffff;'>
				        <table align='center' style='width: 100%;' cellspacing='0' cellpadding='0' border='0' role='presentation'>
					        <tbody>
						        <tr>
							        <td class='o_bg-dark o_px-md o_py-xl o_xs-py-md' align='center' style='background-color: #242b3d;padding-left: 24px;padding-right: 24px;padding-top: 64px;padding-bottom: 64px;'>
								        <!--[if mso]><table width='584' cellspacing='0' cellpadding='0' border='0' role='presentation'><tbody><tr><td align='center'><![endif]-->
								        <div class='o_col-6s o_sans o_text-md o_text-white o_center' style='font-family: Helvetica, Arial, sans-serif;margin-top: 0px;margin-bottom: 0px;font-size: 19px;line-height: 28px;max-width: 584px;color: #ffffff;text-align: center;'>
									        <h2 class='o_heading o_mb-xxs' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 4px;font-size: 30px;line-height: 39px;'>Great News!</h2>
                                            <p class='o_mb-md' style='margin-top: 0px;margin-bottom: 24px;'>{message}</p>   
									        <table align='center' cellspacing='0' cellpadding='0' border='0' role='presentation'>
										        <tbody>
											        <tr>
												        <td width='300' class='o_btn o_bg-primary o_br o_heading o_text' align='center' style='font-family: Helvetica, Arial, sans-serif;font-weight: bold;margin-top: 0px;margin-bottom: 0px;font-size: 16px;line-height: 24px;mso-padding-alt: 12px 24px;background-color: #126de5;border-radius: 4px;'>
													        <a class='o_text-white' href='{btn_url}' style='text-decoration: none;outline: none;color: #ffffff;display: block;padding: 12px 24px;mso-text-raise: 3px;'>Start Free Trial</a>
												        </td>
											        </tr>
										        </tbody>
									        </table>		  
								        </div>
							        </td>										
						        </tr>	   
					        </tbody>
				        </table>
			        </td>
		        </tr>
	        </tbody>
        </table>";
    }

    //Example Code : Needs to be changed for this application
    //public struct EmailTemplateCategories
    //{
    //    public const string ChurchRegistration = "Church Registration";
    //    public const string NewUserAccount = "New User Account";
    //    public const string ChurchRegistrationSuperAdmin = "Church Registration Super Admin";
    //    public const string ForgotPassword = "Forget Password";
    //    public const string PasswordReset = "Password Reset";
    //    public const string PasswordChanged = "Password Changed";
    //    public const string EmailVerification = "Email Verification";
    //    public const string PaymentProcessed = "Payment Processed";
    //    public const string ScheduledGiving = "Scheduled Giving";
    //    public const string PaymentError = "Payment Error";
    //    public const string AnnualGivingStatement = "Annual Giving Statement";
    //    public const string PrayerRequest = "Prayer Request";
    //    public const string TaskAssigned = "Task Assigned";

    //    public static List<string> Items = new List<string> { ChurchRegistration, NewUserAccount, ChurchRegistrationSuperAdmin, ForgotPassword, PasswordReset, PasswordChanged, EmailVerification, PaymentProcessed, ScheduledGiving, PaymentError, AnnualGivingStatement, PrayerRequest, TaskAssigned };

    //    //Add the email template HTML here.
    //    //NOTE: Some will use the same HTML so we can remove the need for separate template code, but use the different email template categories
    //    public static Dictionary<string, string> Templates = new Dictionary<string, string>
    //    {
    //        { ChurchRegistration, "<div id=''></div>" },
    //        { NewUserAccount, "<div id=''></div>" },
    //        { ChurchRegistrationSuperAdmin, "<div id=''></div>" },
    //        { ForgotPassword, "<div id=''></div>" },
    //        { PasswordReset, "<div id=''></div>" },
    //        { PasswordChanged, "<div id=''></div>" },
    //        { EmailVerification, "<div id=''></div>" },
    //        { PaymentProcessed, "<div id=''></div>" },
    //        { ScheduledGiving, "<div id=''></div>" },
    //        { PaymentError, "<div id=''></div>" },
    //        { AnnualGivingStatement, "<div id=''></div>" },
    //        { PrayerRequest, "<div id=''></div>" },
    //        { TaskAssigned, "<div id=''></div>" }
    //    };
    //}
    #endregion

    public struct EmailTemplateTypes
    {
        public const string System = "SYSTEM";
        public const string Church = "Church";
    }

    public struct NotificationTypes
    {
        public const string Default = "SYSTEM";
        public const string Church = "Church";
        public const string Denomination = "Denomination";
        public const string Email = "Email";
        public const string Event = "Event";
        public const string Location = "Location";
        public const string Payment = "Payment";
        public const string Service = "Service";
        public const string SmsMessage = "SmsMessage";
        public const string Task = "Task";
        public const string User = "User";

        public static List<string> Items = new List<string> { Default, Church, Denomination, Email, Event, Location, Payment, Service, SmsMessage, Task, User };
    }

    public struct NotificationIcons
    {
        public const string Default = "fa-bell";
        public const string Church = "fa-church";
        public const string Denomination = "fa-cross";
        public const string Email = "fa-envelope";
        public const string Event = "fa-calendar-alt";
        public const string Location = "fa-map-marker-alt";
        public const string Payment = "fa-dollar-sign";
        public const string Service = "fa-concierge-bell";
        public const string SmsMessage = "fa-comment-dots";
        public const string Task = "fa-tasks";
        public const string User = "fa-user";

        public static List<string> Items = new List<string> { Default, Church, Denomination, Email, Event, Location, Payment, Service, SmsMessage, Task, User };
    }

    public struct ModalSizes
    {
        public const string Default = "";
        public const string Small = "modal-sm";
        public const string Large = "modal-lg";
        public const string XL = "modal-xl";
    }

    public struct CheckInType
    {
        public const string Guest = "Guest";
        public const string Volunteer = "Volunteer";

        public static List<string> Items = new List<string> { Guest, Volunteer };
    }

    public struct EventFrequency
    {
        public const string Weekly = "Weekly";
        public const string Daily = "Daily / VBS";
        public const string None = "None";

        public static List<string> Items = new List<string> { Weekly, Daily, None };
    }

    public struct BankAccountTypes
    {
        public const string Checking = "Checking";
        public const string Savings = "Savings";

        public static List<string> Items = new List<string> { Checking, Savings };
    }

    public struct BankAccountTypeInitials
    {
        public const string Checking = "C";
        public const string Savings = "S";

        public static List<string> Items = new List<string> { Checking, Savings };
    }

    public struct CreditCardTypes
    {
        public const string Visa = "VISA";
        public const string Mastercard = "MSTR";
        public const string Discover = "DISC";
        public const string AmericanExpress = "AMEX";

        public static List<string> Items = new List<string> { AmericanExpress, Discover, Mastercard, Visa };
    }

    public struct PaymentMethodAccountTypes
    {
        public const string User = "User";
        public const string Church = "Church";
    }

    public struct PaymentMethodNickNames
    {
        public const string Visa = "Visa";
        public const string Mastercard = "MasterCard";
        public const string Discover = "Discover";
        public const string AmericanExpress = "American Express";
        public const string Checking = "Checking Account";
        public const string Savings = "Savings Account";
    }

    public struct CreditCardIcons
    {
        public const string Visa = "data:image/png;base64,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";
        public const string Mastercard = "data:image/png;base64,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";
        public const string Discover = "data:image/png;base64,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";
        public const string AmericanExpress = "data:image/png;base64,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";
    }

    public struct MerchantProviders
    {
        public const string Nuvei = "Nuvei";

        public static List<string> Items = new List<string> { Nuvei };
    }

    public struct DigitalPaymentMethods
    {
        public const string Card = "Card";
        public const string ACH = "ACH";

        public static List<string> Items = new List<string> { Card, ACH };
    }

    public struct OfflinePaymentMethods
    {
        public const string Cash = "Cash";
        public const string Check = "Check";

        public static List<string> Items = new List<string> { Cash, Check };
    }

    public struct PaymentMethodTypes
    {
        public const string Digital = "Digital";
        public const string Offline = "Offline";

        public static List<string> Items = new List<string> { Digital, Offline };
    }

    public struct DigitalPaymentTypes
    {
        public const string Online = "Online";
        public const string TextToGive = "TextToGive";

        public static List<string> Items = new List<string> { Online, TextToGive };
    }

    public struct OfflinePaymentTypes
    {
        public const string OfferingPlate = "Offering Plate";
        public const string DropOff = "Drop-Off";
        public const string Mail = "Mail";

        public static List<string> Items = new List<string> { OfferingPlate, DropOff, Mail };
    }

    public struct OfflineGiftAmountTypes
    {
        public const string Donor = "Donor";
        public const string LumpSum = "Lump-Sum";

        public static List<string> Items = new List<string> { Donor, LumpSum };
    }

    //TODO: We need to determine if transaction is a payment, refund, or credit to account
    public struct TransactionType
    {
        public const string Payment = "Payment";
        public const string Refund = "Refund";
        public const string Credit = "Credit";

        public static List<string> Items = new List<string> { Payment, Refund, Credit };
    }

    public struct TransactionTypeShortCode
    {
        public const string Auth = "auth";
        public const string RepeatSale = "repeatsale";
        public const string Refund = "TBD-TODO";
        //public const string Credit = "Credit";

        public static List<string> Items = new List<string> { Auth, Refund };
    }

    public struct PaymentStatus
    {
        public const string Success = "Success";
        public const string Declined = "Declined";
        public const string Refund = "Refund";
        public const string Error = "Error";

        public static List<string> Items = new List<string> { Success, Declined, Refund, Error };
    }

    public struct PaymentOccurrence
    {
        public const string OneTime = "OneTime";
        public const string Recurring = "Recurring";

        public static List<string> Items = new List<string> { OneTime, Recurring };
    }

    public struct UserType
    {
        public const string People = "People";
    }

    public struct PaymentFrequency
    {
        public const string Weekly = "Weekly";
        public const string Biweekly = "Biweekly";
        public const string Monthly = "Monthly";
        public const string FirstAndFifteenthMonthly = "1st and 15th Monthly";

        public static List<string> Items = new List<string> { Weekly, Biweekly, Monthly, FirstAndFifteenthMonthly };
    }

    public struct GiftEndingReasons
    {
        public const string WhenICancelIt = "When I Cancel It";
        public const string OnASpecificDate = "On a Specific Date";
        public const string AfterMaxNumberofGifts = "After Number of Gifts";

        public static List<string> Items = new List<string> { WhenICancelIt, OnASpecificDate, AfterMaxNumberofGifts };
    }

    public struct PrayerRequestStatuses
    {
        public const string Confidential = "Confidential";
        public const string HighPriority = "HighPriority";
        public const string Responded = "Responded";
        public const string FollowUpRequired = "FollowUpRequired";
        public const string PrayedOver = "PrayedOver";
        public const string NotPrayedOver = "NotPrayedOver";
        public const string Read = "Read";
        public const string Unread = "Unread";
        public const string Starred = "Starred";
        public const string NotStarred = "Not Starred";

        public static List<string> Items = new List<string> { Confidential, HighPriority, Responded, FollowUpRequired, PrayedOver, NotPrayedOver, Read, Unread, Starred, NotStarred };
    }

    public struct Priorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";

        public static List<string> Items = new List<string> { Low, Medium, High };
    }

    public struct LogType
    {
        public const string Inbox = "Inbox";
        public const string Error = "Error";

        public static List<string> Items = new List<string> { Inbox, Error };
    }

    public struct TaskStatuses
    {
        public const string Complete = "complete";
        public const string Incomplete = "incomplete";
        public const string PastDue = "pastDue";

        public static List<string> Items = new List<string> { Complete, Incomplete, PastDue };
    }

    public struct LoginProvider
    {
        public const string Application = "Application";
        public const string Facebook = "Facebook";
        public const string Google = "Google";
    }

    public struct SignInResponseStatus
    {
        public const string LoadLoginForm = "LoadLoginForm";
        public const string LoadPassword = "LoadPassword";
        public const string LoadConfirmationCode = "LoadConfirmationCode";
        public const string InvalidAttempt = "InvalidAttempt";
        public const string LoadRegistrationForm = "LoadRegistrationForm";
        public const string LoadRegistrationDetailsForm = "LoadRegistrationDetailsForm";
        public const string SetupPassword = "SetupPassword";
        public const string LoginWithPassword = "LoginWithPassword";
    }

    public struct LoginResponseStatus
    {
        public const string LoadLoginForm = "LoadLoginForm";
        public const string LoadPassword = "LoadPassword";
        public const string InvalidAttempt = "InvalidAttempt";
        public const string SetupPassword = "SetupPassword";
        public const string LoginWithPassword = "LoginWithPassword";
    }

    public struct RegisterVia
    {
        public const string Email = "Email";
        public const string PhoneNumber = "PhoneNumber";
    }

    public struct LoginVia
    {
        public const string Email = "Email";
        public const string Phone = "Phone";
    }

    public struct PresetDateRange
    {
        public const string Week = "Last Week";
        public const string Month = "Last Month";
        public const string Year = "Last Year";

        public static List<string> Items = new List<string> { Week, Month, Year };
    }

    public static class ReportUtilities
    {
        public static List<string> YAxisColumnKeywords()
        {
            return new List<string>() {
                "total",
                "count",
                "counts",
                "sum",
                "avg",
                "average",
                "amount",
                "amounts"
            };
        }

        public static List<string> DataSetKeywords()
        {
            return new List<string>() {
                "fund",
                "funds",
                "campus",
                "name"
            };
        }

        public static List<string> CreatedDateKeywords()
        {
            return new List<string>() {
                "date",
                "daterange",
                "date range",
                "datecreated",
                "createddate",
                "insertedon",
                "createdon"
            };
        }

        public static List<string> CampusKeywords()
        {
            return new List<string>() {
                "location",
                "locationid",
                "campus",
                "campusid",
                "campuses"
            };
        }
    }

    public struct RespondedViaTypes
    {
        public const string Email = "Email";
        public const string Phone = "Phone";
        public const string InPerson = "In-Person";

        public static List<string> Items = new List<string> { Email, Phone, InPerson };
    }

    public struct SermonNoteTypes
    {
        public const string Standard = "Standard";
        public const string Filled = "Filled";
        public const string Blank = "Blank";

        public static List<string> Items = new List<string> { Standard, Filled, Blank };
    }

    public struct MaritalStatuses
    {
        public const string Single = "Single";
        public const string Married = "Married";
        public const string Divorced = "Divorced";
        public const string Separated = "Separated";
        public const string Widowed = "Widowed";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { Single, Married, Divorced, Separated, Widowed, Other };
    }

    public struct EducationTypes
    {
        public const string LessThanHighSchool = "12th grade or less";
        public const string HighSchool = "High School";
        public const string CollegeDegree = "College degree";
        public const string PostGraduateDegree = "Post-gradute degree";
        public const string VocationalTraining = "Vocational training";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { LessThanHighSchool, HighSchool, CollegeDegree, PostGraduateDegree, VocationalTraining, Other };
    }

    public struct EthnicTypes
    {
        public const string Asian = "Asian or Pacific Islander";
        public const string Black = "Black or African American";
        public const string Hispanic = "Hispanic or Latino";
        public const string NativeAmerican = "Native American or Alaska Native";
        public const string White = "White";
        public const string Multiracial = "Multiracial or Biracial";
        public const string Other = "Other";
        public const string PreferNotToSay = "Prefer Not to Say";

        public static List<string> Items = new List<string> { Asian, Black, Hispanic, NativeAmerican, White, Multiracial, Other, PreferNotToSay };
    }

    public struct EmploymentStatuses
    {
        public const string FullTime = "Full-time";
        public const string PartTime = "Part-time";
        public const string Homemaker = "Homemaker";
        public const string Student = "Student";
        public const string Military = "Military";
        public const string Retired = "Retired";
        public const string Unemployed = "Unemployed";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { FullTime, PartTime, Homemaker, Student, Military, Retired, Unemployed, Other };
    }

    public struct MilitaryStatuses
    {
        public const string ActiveDuty = "Active-duty";
        public const string InactiveReserve = "Inactive Reserve";
        public const string None = "None";
        public const string Reserve = "Reserve";
        public const string Retired = "Retired";

        public static List<string> Items = new List<string> { ActiveDuty, InactiveReserve, None, Reserve, Retired };
    }

    public struct HouseholdIncomes
    {
        public const string UnderTwenty = "Under $20,000";
        public const string TwentyFourty = "20,001 - $40,000";
        public const string FourtySixty = "$40,001 - $60,000";
        public const string SixtyEighty = "$60,001 - $80,000";
        public const string EightyOneHundred = "$80,001 - $100,000";
        public const string OneHundredPlus = "$100,001 or over";

        public static List<string> Items = new List<string> { UnderTwenty, TwentyFourty, FourtySixty, SixtyEighty, EightyOneHundred, OneHundredPlus };
    }

    public struct GivingReportType
    {
        public const string LYBUNT = "Last Year But Unfortunately Not This (LYBUNT)";
        public const string SYBUNT = "Some Year But Unfortunately Not This (SYBUNT)";
        public const string DonorType = "Donor Type";
        public const string DigitalPaymentMethodType = "Giving by Digital Payment Method Type";
        public const string PaymentType = "Giving by Payment Type (Digital or Offline)";
        public const string DigitalPaymentType = "Giving by Digital Payment Type";
        public const string ManualPaymentType = "Giving by Manual Payment Type";
        public const string WeekDay = "Giving by Weekday";
        public const string TimeADay = "Giving by Time of Day";
        public const string Custom = "Custom";

        public static List<string> Items = new List<string> { LYBUNT, SYBUNT, DonorType, DigitalPaymentMethodType, PaymentType, DigitalPaymentType, ManualPaymentType, WeekDay, TimeADay };
    }

    public struct ChartColors
    {
        public const string Red = "#f64e60";
        public const string Orange = "#d35400";
        public const string Yellow = "#f1c40f";
        public const string Green = "#1bc5bd";
        public const string Blue = "#3699ff";
        public const string Purple = "#8950fc";
        public const string Black = "#181c32";
        public const string Grey = "#e4e6ef";
        public const string Pink = "#fd79a8";
        public const string Brown = "#906030";

        public static List<string> Items = new List<string> { Red, Orange, Yellow, Green, Blue, Purple, Black, Grey, Pink, Brown };
    }

    public struct GivingFunds
    {
        public const string TithesAndOfferings = "Tithes and Offerings";
        public const string General = "General";
        public const string Building = "Building";
        public const string Missions = "Missions";
        public const string DisasterRelief = "Disaster Relief";
        public const string Youth = "Youth";

        public static List<string> Items = new List<string> { TithesAndOfferings, General, Building, Missions, DisasterRelief, Youth };
    }

    public readonly struct FundDisplaySettings
    {
        public string FundName { get; }
        public string Type { get; }
        public string BgClass { get; }
        public string IconClass { get; }

        public FundDisplaySettings(string fundName, string type, string bgClass, string iconClass)
        {
            FundName = fundName;
            Type = type;
            BgClass = bgClass;
            IconClass = iconClass;
        }
    }

    public static class FundDisplayData
    {
        private const string nameTithes = "tithes";
        private const string nameGeneral = "general";
        private const string nameMissions = "missions";
        private const string backgroundInfo = "bg-info";
        private const string backgroundDark = "bg-dark";
        private const string backgroundWarning = "bg-warning";
        private const string iconChurch = "fa-church";
        private const string iconPiggyBank = "fa-piggy-bank";
        private const string iconGlobeAfrica = "fa-globe-africa";

        public static readonly FundDisplaySettings Tithes = new FundDisplaySettings(nameTithes, GivingFunds.TithesAndOfferings, backgroundInfo, iconChurch);
        public static readonly FundDisplaySettings General = new FundDisplaySettings(nameGeneral, GivingFunds.General, backgroundDark, iconPiggyBank);
        public static readonly FundDisplaySettings Missions = new FundDisplaySettings(nameMissions, GivingFunds.Missions, backgroundWarning, iconGlobeAfrica);

        public static List<FundDisplaySettings> AllFunds = new List<FundDisplaySettings>
        {
            Tithes,
            General,
            Missions
        };
    }

    public struct ChurchEvents
    {
        public const string WorshipService = "Worship Service";
        //public const string WeekdayService = "Weekday Service";
        public const string MarriageConference = "Marriage Conference";
        public const string MenConference = "Men's Conference";
        public const string ServeDay = "Serve Day";
        public const string VacationBibleSchool = "Vacation Bible School";
        public const string VolunteerTraining = "Volunteer Training";
        public const string WomenConference = "Women's Conference";

        public static List<string> Items = new List<string> { WorshipService, MarriageConference, MenConference, ServeDay, VacationBibleSchool, VolunteerTraining, WomenConference };
    }

    public struct CommonServiceAreas
    {
        public const string Administration = "Administration";
        public const string BaptismTeam = "Baptism Team";
        public const string CampusHostTeam = "Campus Host Team";
        public const string CampusSupportFacilitiesTeam = "Campus Support/Facilities Team";
        public const string EventsTeam = "Events Team";
        public const string FirstResponders = "First Responders";
        public const string Greeters = "Greeters";
        public const string KidsTeam = "Kids Team";
        public const string OutreachTeam = "Outreach Team";
        public const string ParkingTeam = "Parking Team";
        public const string PrayerTeam = "Prayer Team";
        public const string PreschoolAndNursery = "Preschool and Nursery";
        public const string Production = "Production";
        public const string ResourceTeam = "Resource Team";
        public const string SmallGroupLeaders = "Small Group Leaders";
        public const string Students = "Students";
        public const string Users = "Users";
        public const string VolunteerCheckInTeam = "Volunteer Check-In Team";
        public const string VolunteerHostTeam = "Volunteer Host Team";

        public static List<string> Items = new List<string> { Administration, BaptismTeam, CampusHostTeam, CampusSupportFacilitiesTeam, EventsTeam,
            FirstResponders, Greeters, KidsTeam, OutreachTeam, ParkingTeam, PrayerTeam, PreschoolAndNursery, Production, ResourceTeam, SmallGroupLeaders,
            Students, Users, VolunteerCheckInTeam, VolunteerHostTeam };
    }

    public struct PraiseFunds
    {
        public const string Subscriptions = "Subscriptions";

        public static List<string> Items = new List<string> { Subscriptions };
    }

    public struct FileExtension
    {
        public const string csv = "csv";
        public const string xlsx = "xlsx";
    }

    public struct ActionName
    {
        public const string FundReport = "FundReport";
        public const string CampusGivingReport = "CampusGivingReport";
    }

    public struct PlanType
    {
        public const string Free = "Free";
        public const string Premium = "Premium";
    }

    public struct PeopleSelectionMode
    {
        public const string Manual = "Manual";
        public const string System = "System";
    }

    public struct BillingType
    {
        public const string Monthly = "Monthly";
        public const string Annually = "Annually";
        public const string Free = "Free";

        public static List<string> Items = new List<string> { Monthly, Annually, Free };
    }

    public struct ControllerName
    {
        public const string Reports = "Reports";
    }

    public struct ReportTypes
    {
        public const string Fixed = "Fixed";
        public const string Custom = "Custom";
    }

    public static class GivingReports
    {
        public static List<SelectListItem> Items()
        {
            const string reportPath = "/reports";

            return new List<SelectListItem>()
            {
                new SelectListItem{ Text = "Giving Summary", Value = $"{reportPath}/givingsummary" },
                new SelectListItem{ Text = "Digital Giving Summary", Value = $"{reportPath}/digitalgivingsummary" },
                new SelectListItem{ Text = "Offline Giving Summary", Value = $"{reportPath}/offlinegivingsummary" },
                new SelectListItem{ Text = "Top Donors", Value = $"{reportPath}/topdonors" },
                new SelectListItem{ Text = "Birthday Report", Value = $"{reportPath}/upcomingbirthdays" },
                new SelectListItem{ Text = "Deceased Report", Value = $"{reportPath}/deceasedreport" },
                new SelectListItem{ Text = "Top Donation", Value = $"{reportPath}/topdonations" },
                new SelectListItem{ Text = "Baptism Report", Value = $"{reportPath}/baptismreport" },
                new SelectListItem{ Text = "Other Url", Value = "Other" },
            };
        }

        public static List<SelectListItem> SystemReports()
        {
            const string reportPath = "/reports";

            return new List<SelectListItem>()
            {
                new SelectListItem{ Text = "Giving Summary", Value = $"{reportPath}/givingsummary" },
                new SelectListItem{ Text = "Digital Giving Summary", Value = $"{reportPath}/digitalgivingsummary" },
                new SelectListItem{ Text = "Offline Giving Summary", Value = $"{reportPath}/offlinegivingsummary" },
                new SelectListItem{ Text = "Top Donors", Value = $"{reportPath}/topdonors" },
                new SelectListItem{ Text = "Top Donors - Past 5 Years", Value = $"{reportPath}/topdonorspastfiveyears" },
                new SelectListItem{ Text = "Top Donations - Past 5 Years", Value = $"{reportPath}/topdonationspastfiveyears" },
                new SelectListItem{ Text = "Upcoming Birthdays", Value = $"{reportPath}/upcomingbirthdays" },
                new SelectListItem{ Text = "Deceased Report", Value = $"{reportPath}/deceasedreport" },
                new SelectListItem{ Text = "Baptism Report", Value = $"{reportPath}/baptismreport" },
            };
        }

        public static List<SelectListItem> ReportTypes()
        {
            return new List<SelectListItem>()
            {
               new SelectListItem{ Text = "Custom", Value = "Custom" },
               new SelectListItem{ Text = "Fixed", Value = "Fixed", Selected = true }
            };
        }
    }

    public struct WidgetPermissionType
    {
        public const string Role = "Role";
        public const string User = "User";
    }

    public struct WidgetLocations
    {
        public const string Top = "Top";
        public const string Main = "Main";
    }

    public struct WidgetSizes
    {
        public const string Quarter = "1/4 Width";
        public const string Third = "1/3 Width";
        public const string Half = "1/2 Width";
        public const string Full = "Full Width";
    }

    public struct SystemWidgets
    {
        public const string Notifications = "Notifications";
        public const string PrayerRequests = "Prayer Requests";
        public const string PrayerRequestsConfidential = "Prayer Requests (Confidential)";
        public const string PrayerRequestsHighPriority = "Prayer Requests (High Priority)";
        public const string MyAttendance = "My Attendance";
        public const string WeeklyAttendance = "Weekly Attendance";
        public const string WeeklyAttendanceChange = "Weekly Attendance Change";
        public const string WeeklyBaptisms = "Weekly Baptisms";
        public const string TodaySchedule = "Today's Schedule";
        public const string ThisWeekSchedule = "This Week's Schedule";
        public const string BaptismsYTD = "Baptisms - YTD";
        public const string OnHOLDUpcomingServices = "On HOLD - Upcoming Services";
        public const string ONHOLDUpcomingEvents = "ON HOLD - Upcoming Events";
        public const string EmployeeBirthdays = "Employee Birthdays";
        public const string WeeklySalvations = "Weekly Salvations";
        public const string MyGiving = "My Giving";
        public const string RecentDeaths = "Recent Deaths";
        public const string SalvationsYTD = "Salvations - YTD";
        public const string TodayGiving = "Today's Giving";
        public const string WeeklyGiving = "Weekly Giving";
        public const string WeeklyGivingComparison = "Weekly Giving Comparison";
        public const string NewDonors = "New Donors";

        public static List<string> Items = new List<string>
        {
            Notifications,
            PrayerRequests,
            PrayerRequestsConfidential,
            PrayerRequestsHighPriority,
            MyAttendance,
            WeeklyAttendance,
            WeeklyAttendanceChange,
            WeeklyBaptisms,
            TodaySchedule,
            ThisWeekSchedule,
            BaptismsYTD,
            OnHOLDUpcomingServices,
            ONHOLDUpcomingEvents,
            EmployeeBirthdays,
            WeeklySalvations,
            MyGiving,
            RecentDeaths,
            SalvationsYTD,
            TodayGiving,
            WeeklyGiving,
            WeeklyGivingComparison,
            NewDonors,
        };
    }

    public struct Languages
    {
        public const string English = "English";
        public const string Spanish = "Spanish";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { English, Spanish, Other };
    }

    public struct WorkWeeks
    {
        public const string SundaySaturday = "Sunday-Saturday";
        public const string MondaySunday = "Monday-Sunday";

        public static List<string> Items = new List<string> { SundaySaturday, MondaySunday };
    }

    public struct FollowUpMethods
    {
        public const string InPerson = "In-Person";
        public const string Phone = "Phone";
        public const string Email = "Email";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { InPerson, Phone, Email, Other };
    }

    public struct FollowUpStatuses
    {
        public const string Incomplete = "Incomplete";
        public const string AttemptedToContact = "Attempted To Contact";
        public const string Completed = "Completed";

        public static List<string> Items = new List<string> { Incomplete, AttemptedToContact, Completed };
    }

    public struct InboxDensity
    {
        public const string Default = "Default";
        public const string Comfortable = "Comfortable";
        public const string Compact = "Compact";

        public static List<string> Items = new List<string> { Default, Comfortable, Compact };
    }

    public struct InboxType
    {
        public const string Default = "Default";
        public const string UnreadFirst = "Unread First";
        public const string StarredFirst = "Starred First";

        public static List<string> Items = new List<string> { Default, UnreadFirst, StarredFirst };
    }

    public struct FamilyRoles
    {
        public const string HeadOfHousehold = "Head of Household";
        public const string Spouse = "Spouse";
        public const string Adult = "Adult";
        public const string Child = "Child";
        public const string Unassigned = "Unassigned";

        public static List<string> Items = new List<string> { HeadOfHousehold, Spouse, Adult, Child, Unassigned };
    }

    public enum LeadStatuses
    {
        [Description("New")]
        New = 0,
        [Description("Attempted To Contact")]
        AttemptedToContact = 1,
        [Description("Bad Timing")]
        BadTiming = 2,
        [Description("In Progress")]
        InProgress = 3,
        [Description("Nurture")]
        Nurture = 4,
        [Description("Qualified")]
        Qualified = 5,
        [Description("Unqualified")]
        Unqualified = 6
    }

    public enum DonorStatus
    {
        [Description("Non-Donor")]
        NonDonor = 0,
        [Description("1st Time Donor")]
        FirstTimeDonor = 1,
        [Description("2nd Time Donor")]
        SecondTimeDonor = 2,
        [Description("Occasional Donor")]
        OccasionalDonor = 3,
        [Description("Regular Donor")]
        RegularDonor = 4,
        [Description("Recurring Donor")]
        RecurringDonor = 5,
    }

    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum GenericEnum)
        {
            var genericEnumType = GenericEnum.GetType();
            var memberInfo = genericEnumType.GetMember(GenericEnum.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Any())
                {
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            return GenericEnum.ToString();
        }

        public static List<SelectListItem> GetEnumValues<t>()
        {
            var model = new List<SelectListItem>();

            foreach (var item in Enum.GetValues(typeof(t)))
            {
                var data = new SelectListItem
                {
                    Value = Convert.ToString((int)item),
                    Text = GetDescription((Enum)item)
                };
                model.Add(data);
            }

            return model;
        }

        public static List<SelectListItem> DayOfWeek(DateTime? selectedDay = null)
        {
            var model = new List<SelectListItem>();
            var today = selectedDay.IsNotNullOrEmpty() ? Convert.ToDateTime(selectedDay).DayOfWeek : DateTime.Now.DayOfWeek;

            foreach (var item in Enum.GetValues(typeof(DayOfWeek)))
            {
                var data = new SelectListItem
                {
                    Value = Convert.ToString((int)item),
                    Text = GetDescription((Enum)item).SubstringIt(1),
                    Selected = today.Equals((Enum)item)
                };
                model.Add(data);
            }

            return model;
        }
    }

    public struct FolderTypes
    {
        public const string Attachment = "Attachment";
        public const string Tag = "Tag";

        public static List<string> Items = new List<string> { Attachment, Tag };
    }

    public struct DefaultFolders
    {
        public const string Kids = "Kids";
        public const string Missions = "Missions";
        public const string MensMinistries = "Men's Ministries";
        public const string SmallGroups = "Small Groups";
        public const string StudentMinistries = "Student Ministries";
        public const string WomensMinistries = "Women's Ministries";

        public static List<string> Items = new List<string> { Kids, Missions, MensMinistries, SmallGroups, StudentMinistries, WomensMinistries };
    }

    public struct AgeGroupDemographics
    {
        public const string FifteenNineteen = "15-19";
        public const string TwentyTwentyFour = "20-24";
        public const string TwentyFiveTwentyNine = "25-29";
        public const string Thirties = "30-39";
        public const string Forties = "40-49";
        public const string Fifties = "50-59";
        public const string Sixties = "60-69";
        public const string Seventies = "70-79";
        public const string Eighties = "80-89";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { FifteenNineteen, TwentyTwentyFour, TwentyFiveTwentyNine, Thirties, Forties, Fifties, Sixties, Seventies, Eighties, Other };
    }

    public struct AgeGroups
    {
        public const string All = "All";
        public const string ElevenFourteen = "11-14";
        public const string FourteenEighteen = "14-18";
        public const string EighteenTwentyTwo = "18-22";
        public const string MiddleSchool = "Middleschool";
        public const string HighSchool = "Highschool";
        public const string College = "College";
        public const string Twenties = "20s";
        public const string Thirties = "30s";
        public const string Forties = "40s";
        public const string Fifties = "50s";
        public const string Sixties = "60s";

        public static List<string> Items = new List<string> { All, ElevenFourteen, FourteenEighteen, EighteenTwentyTwo, MiddleSchool, HighSchool, College, Twenties, Thirties, Forties, Fifties, Sixties };
    }

    public struct ChurchEventFrequency
    {
        public const string DoesNotRepeat = "Does Not Repeat";
        public const string Daily = "Daily";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Yearly = "Yearly";
        public const string EveryWeekday = "Every Weekday(Monday to Friday)";
        public const string Custom = "Custom";

        public static List<string> Items = new List<string> { DoesNotRepeat, Daily, Weekly, Monthly, Yearly, EveryWeekday, Custom };
    }

    public struct EventRepeatFrequency
    {
        public const string Day = "Day";
        public const string Week = "Week";
        public const string Month = "Month";
        public const string Year = "Year";

        public static List<string> Items = new List<string> { Day, Week, Month, Year, };
    }

    public struct EventEnds
    {
        public const string Never = "Never";
        public const string OnSpecificDate = "On a Specific Date";
        public const string AfterEventOccurrences = "After # of Occurrences";

        public static List<string> Items = new List<string> { Never, OnSpecificDate, AfterEventOccurrences };
    }

    public struct MonthlyCustomFrequency
    {
        public const string MonthDay = "Month day";
        public const string WeekDay = "Week day";

        public static List<string> Items = new List<string> { MonthDay, WeekDay };
    }

    public struct EmailStatus
    {
        public const string Queued = "Queued";
        public const string Sent = "Sent";
        public const string Error = "Error";

        public static List<string> Items = new List<string> { Queued, Sent, Error };
    }

    public struct LogSortingType
    {
        public const string Church = "Church";
        public const string Controller = "Controller";
        public const string Type = "Type";

        public static List<SelectListItem> Item()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Value = "Church", Text = "Church" },
                new SelectListItem{ Value = "Controller", Text = "Controller" },
                new SelectListItem{ Value = "Type", Text = "Type" }
            };
        }
    }

    public struct RelationshipTypes
    {
        public const string Family = "Family";
        public const string Friend = "Friend";
        public const string Other = "Other";

        public static List<string> Items = new List<string> { Family, Friend, Other };
    }

    public struct ContactFormEmailTypes
    {
        public const string NewLead = "New Lead";
        public const string ExistingLead = "Existing Lead";
        public const string ExistingUser = "Existing User";

        public static List<string> Items = new List<string> { NewLead, ExistingLead, ExistingUser };
    }

    public struct URLPrefixes
    {
        public const string Http = "http://";
        public const string Https = "https://";

        public static List<string> Items = new List<string> { Http, Https };
    }

    public enum BlogStatuses
    {
        [Description("Draft")]
        Draft,
        [Description("Publish")]
        Publish,
        [Description("Trash")]
        Trash,
        [Description("Future")]
        Future
    }

    public struct ActiveStatuses
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
    }

    public struct APIStatuses
    {
        public const string Success = "Success";
        public const string Error = "Error";

        public static List<string> Items = new List<string> { Success, Error };
    }

    public struct CustomerTypes
    {
        public const string CK = "CK";

        public static List<string> Items = new List<string> { CK };
    }

    public struct CustomerStatuses
    {
        public const string Active = "ACTIVE";
        public const string Inactive = "INACTIVE";

        public static List<string> Items = new List<string> { Active, Inactive };
    }

    public struct CompanyOwnershipTypes
    {
        public const string SoleProp = "Sole Proprietorship";
        public const string Corporation = "Corporation";
        public const string Partnership = "Partnership";
        public const string LLC = "LLC";
        public const string NonProfit = "Non-Profit";
        public const string Government = "Government";

        public static List<string> Items = new List<string> { SoleProp, Corporation, Partnership, LLC, NonProfit, Government };
    }

    public struct PaymentMethodStatuses
    {
        public const string Active = "Active";
        public const string Expired = "Expired";

        public static List<string> Items = new List<string> { Active, Expired };
    }

    public struct TextResponses
    {
        public const string YES = "YES";
        public const string NO = "NO";
        public const string YesAbbrv = "Y";
        public const string NoAbbrv = "N";

        public static List<string> Items = new List<string> { YES, NO, YesAbbrv, NoAbbrv };
    }

    public struct SocialMediaIcons
    {
        public const string Facebook = "facebook-light.png";
        public const string Twitter = "twitter-light.png";
        public const string Instagram = "instagram-light.png";
        public const string YouTube = "youtube-light.png";
        public const string LinkedIn = "linkedin-light.png";
    }

    public struct SubscriptionNotificationMessages
    {
        public const string SubscriptionExpired = "Your Praise CMS subscription has expired. Upgrade your account at any time to access all of Praise CMS' great features or continue with our free plan.";
        public const string FreeTrialExpired = "Your Praise CMS free trial is over, but this doesn't have to mean goodbye! You may upgrade your account at any time to access all of Praise CMS' great features or continue with our free plan.";
        public const string FreeTrialExpiringSoon = "Your Praise CMS free trial will expire in {dayCount} days. To avoid interruption to your service, please verify you have an active payment method on file.";
        public const string FreeTrialStarted = "Your free plan started on {0}. You can upgrade your plan at any time to access all of Praise CMS' great features.";
        public const string FreeTrialStartedAutoRenew = "Your Praise CMS free trial has started. At the end of your trial your plan will automatically convert to our premium plan, billed monthly.";
        public const string FreeTrialStartedAndExpireOn = "Your {0}-day free trial has started and will expire on {1}.";
    }

    public struct SubscriptionNotificationSubjects
    {
        public const string SubscriptionExpired = "Praise CMS subscription expired. Renew today!";
        public const string FreeTrialExpired = "Your Praise CMS free trial has ended. Renew today!";
        public const string FreeTrialExpiringSoon = "Praise CMS free trial ending soon. Subscribe today!";
        public const string FreeTrialStarted = "Your Praise CMS free trial has started.";
    }

    public struct LogStatuses
    {
        public const string Done = "Done";
        public const string Error = "Error";
        public const string Exception = "Exception";
    }
}