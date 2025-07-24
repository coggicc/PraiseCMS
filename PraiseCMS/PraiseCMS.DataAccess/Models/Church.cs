using Foolproof;
using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Churches")]
    public class Church : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Please provide a name for the church.")]
        [DisplayName("Name")]
        public string Name { get; set; }

        [StringLength(50)]
        [DisplayName("Legal Name")]
        public string LegalName { get; set; }

        [StringLength(50)]
        [DisplayName("Phone")]
        [Required(ErrorMessage = "Please provide a phone number for the church.")]
        public string Phone { get; set; }

        [StringLength(75)]
        [DisplayName("Email")]
        [Required(ErrorMessage = "Please provide an email address for the church.")]
        public string Email { get; set; }

        [StringLength(75)]
        [DisplayName("Website URL")]
        public string Website { get; set; }

        [StringLength(75)]
        [DisplayName("Tax Id Number")]
        public string TaxIdNumber { get; set; }

        [StringLength(250)]
        [DisplayName("Logo")]
        public string Logo { get; set; }

        [StringLength(100)]
        [DisplayName("Billing Address Line 1")]
        public string BillingAddress1 { get; set; }

        [StringLength(100)]
        [DisplayName("Billing Address Line 2")]
        public string BillingAddress2 { get; set; }

        [StringLength(50)]
        [DisplayName("Billing City")]
        public string BillingCity { get; set; }

        [StringLength(50)]
        [DisplayName("Billing State")]
        public string BillingState { get; set; }

        [StringLength(10)]
        [DisplayName("Billing Zip")]
        public string BillingZip { get; set; }

        [StringLength(100)]
        [DisplayName("Physical Address Line 1")]
        public string PhysicalAddress1 { get; set; }

        [StringLength(100)]
        [DisplayName("Physical Address Line 2")]
        public string PhysicalAddress2 { get; set; }

        [StringLength(50)]
        [DisplayName("Physical City")]
        public string PhysicalCity { get; set; }

        [StringLength(50)]
        [DisplayName("Physical State")]
        public string PhysicalState { get; set; }

        [StringLength(10)]
        [DisplayName("Physical Zip")]
        public string PhysicalZip { get; set; }

        [DisplayName("Billing Same As Physical")]
        public bool BillingSameAsPhysical { get; set; }

        [DisplayName("Country")]
        public string Country { get; set; }

        [DisplayName("Time Zone")]
        public string TimeZone { get; set; }

        [DisplayName("Time Format")]
        public string TimeFormat { get; set; }

        [StringLength(50)]
        [DisplayName("Denomination")]
        public string Denomination { get; set; }

        [StringLength(75)]
        [DisplayName("Facebook")]
        public string FacebookProfile { get; set; }

        [StringLength(75)]
        [DisplayName("Instagram")]
        public string InstagramProfile { get; set; }

        [StringLength(75)]
        [DisplayName("Twitter")]
        public string TwitterProfile { get; set; }

        [StringLength(75)]
        [DisplayName("YouTube")]
        public string YouTubeProfile { get; set; }

        [StringLength(75)]
        [DisplayName("LinkedIn")]
        public string LinkedInProfile { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        [DisplayName("Has Merchant Account")]
        public bool HasMerchantAccount { get; set; }

        [DisplayName("Primary User")]
        public string PrimaryUserId { get; set; }

        [DisplayName("Is Multisite")]
        public bool IsMultiSite { get; set; }

        [StringLength(500)]
        [DisplayName("Service Area Requirements")]
        public string ServiceAreaRequirements { get; set; }

        [StringLength(500)]
        [DisplayName("Note Categories")]
        public string NoteCategories { get; set; }

        [DisplayName("Number")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [DisplayName("Registration Completed")]
        public bool RegistrationCompleted { get; set; }

        [AllowHtml]
        [StringLength(500)]
        [DisplayName("Giving Thank You Text")]
        public string GivingThankYouText { get; set; }

        [AllowHtml]
        [StringLength(500)]
        [DisplayName("Annual giving statement email body")]
        public string AnnualStatementEmailBody { get; set; }

        [DisplayName("Giving Account Setup Completed")]
        public bool GivingAccountSetupCompleted { get; set; }

        [DisplayName("Allow Donor to Cover Processing Fee")]
        public bool AllowDonorCoverProcessingFee { get; set; }

        [DisplayName("Paperless Giving")]
        public bool PaperlessGiving { get; set; } = true;

        public bool ShowWelcomeMessage { get; set; }

        [DisplayName("Is Auto Email")]
        public bool IsAutoEmail { get; set; }

        [DisplayName("Giving Statement Generated Date")]
        public DateTime? StatementGeneratedDate { get; set; }

        [DisplayName("Monthly Subscription Fees")]
        public decimal SubscriptionFee { get; set; }

        [DisplayName("IPAddress")]
        public string IPAddress { get; set; }

        [DisplayName("Latitude")]
        public double? Latitude { get; set; }

        [DisplayName("Longitude")]
        public double? Longitude { get; set; }

        [DisplayName("Church Donor GUID")]
        public string DonorGUID { get; set; }

        [DisplayName("Church Donor ID")]
        public string DonorID { get; set; }

        [DisplayName("Work Week")]
        public string WorkWeek { get; set; }

        [AllowHtml]
        [StringLength(1000)]
        [DisplayName("Annual Statement Disclaimer")]
        public string AnnualStatementDisclaimer { get; set; }

        [EmailAddress]
        [DisplayName("Prayer Request Email")]
        public string PrayerRequestEmail { get; set; }

        [DisplayName("Completed Prayer Request Alert")]
        public bool CompletedPrayerRequestAlert { get; set; }

        [StringLength(500)]
        [DisplayName("Completed Prayer Request Email Message")]
        [RequiredIf("CompletedPrayerRequestAlert", Operator.EqualTo, true, ErrorMessage = "Please specify a completed prayer request email message.")]
        public string CompletedPrayerRequestEmailMessage { get; set; }

        [StringLength(150)]
        [DisplayName("Completed Prayer Request Text Message")]
        [RequiredIf("CompletedPrayerRequestAlert", Operator.EqualTo, true, ErrorMessage = "Please specify a completed prayer request text message.")]
        public string CompletedPrayerRequestTextMessage { get; set; }

        [DisplayName("Auto Notify Prayer Team")]
        public bool AutoNotifyPrayerTeam { get; set; }

        [DisplayName("Text Message Giving Enabled")]
        public bool TextMessageGivingEnabled { get; set; }

        [StringLength(50)]
        [DisplayName("Text Message Giving Phone Number")]
        public string TextMessageGivingPhoneNum { get; set; }

        [StringLength(500)]
        [DisplayName("Prayer Request Received Text")]
        public string PrayerRequestReceivedText { get; set; }

        [StringLength(500)]
        [DisplayName("Prayer Request Received Follow Up Text")]
        public string PrayerRequestReceivedFollowUpText { get; set; }

        [DisplayName("Twilio Phone Number")]
        public string ChurchTwilioNumber { get; set; }

        public string PhysicalAddress => ((PhysicalAddress1 + " " + PhysicalAddress2).Trim() + ", " + PhysicalCity + ", " + PhysicalState + " " + PhysicalZip).Trim().Trim(',').Trim();

        public string BillingAddress => ((BillingAddress1 + " " + BillingAddress2).Trim() + ", " + BillingCity + ", " + BillingState + " " + BillingZip).Trim().Trim(',').Trim();

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        // Navigation properties
        public virtual ICollection<ChurchEvent> Events { get; set; } // The events associated with the church
        //public virtual ICollection<ChurchEventType> CustomEventTypes { get; set; } // The custom event types for the church
    }

    public class GoogleMapModel
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Label { get; set; }
    }

    public class ChurchDashboard
    {
        public ChurchDashboard()
        {
            Churches = new List<Church>();
            ChurchGiving = new Dictionary<string, decimal>();
            ChurchPrayerRequests = new Dictionary<string, int>();
            Denominations = new List<Denomination>();
        }

        public Dictionary<string, decimal> ChurchGiving { get; set; }
        public Dictionary<string, int> ChurchPrayerRequests { get; set; }
        public List<Church> Churches { get; set; }
        public List<Denomination> Denominations { get; set; }
        public List<Subscription> Subscriptions { get; set; }
    }

    public class ChurchOnboardingView
    {
        public ChurchOnboardingView()
        {
            Denominations = new List<Denomination>();
        }

        public Church Church { get; set; }
        public List<Denomination> Denominations { get; set; }

        public string Plan { get; set; }
        public string AdminUserFirstname { get; set; }
        public string AdminUserLastname { get; set; }
        public string AdminUserEmail { get; set; }
        public string AdminUserPhone { get; set; }
    }

    public class ChurchUserCount
    {
        public string ChurchId { get; set; }
        public int UserCount { get; set; }
    }

    public class StateMapVM
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}