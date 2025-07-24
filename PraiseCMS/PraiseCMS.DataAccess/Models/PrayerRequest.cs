using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("PrayerRequests")]
    public class PrayerRequest : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [Required]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Confidential")]
        public bool Confidential { get; set; }

        [DisplayName("High Priority")]
        public bool HighPriority { get; set; }

        [DisplayName("Responded")]
        public bool Responded { get; set; }

        [DisplayName("Responded Date")]
        public DateTime? RespondedDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [DisplayName("Internal Note")]
        public string InternalNote { get; set; }

        [DisplayName("Category")]
        public string CategoryId { get; set; }

        [DisplayName("Prayed Over")]
        public bool PrayedOver { get; set; }

        [DisplayName("Share Name")]
        public bool ShareName { get; set; }

        [DisplayName("Responded Via")]
        public string RespondedVia { get; set; }

        [DisplayName("Call Back Phone")]
        public string CallBackPhone { get; set; }

        [DisplayName("Person")]
        public string PersonId { get; set; }

        [DisplayName("Read")]
        public bool Read { get; set; }

        [DisplayName("Follow Up Required")]
        public bool FollowUpRequired { get; set; }

        [DisplayName("Follow Up Status")]
        public string FollowUpStatus { get; set; }

        public bool FollowUpCompleted => FollowUpStatus.IsNotNullOrEmpty() && FollowUpStatus.Equals(FollowUpStatuses.Completed);

        [DisplayName("Follow Up Method")]
        public string FollowUpMethod { get; set; }

        [DisplayName("Follow Up Date")]
        public DateTime? FollowUpDate { get; set; }

        [NotMapped]
        [DisplayName("Follow Up Time")]
        public string FollowUpTime { get; set; }

        [DisplayName("Starred")]
        public bool Starred { get; set; }

        [NotMapped]
        public Person Person { get; set; }

        [NotMapped]
        public ApplicationUser FollowUpUser { get; set; }

        [DisplayName("Prayed Over Date")]
        public DateTime? PrayedOverDate { get; set; }

        [DisplayName("Prayed Over By")]
        public string PrayedOverBy { get; set; }

        [DisplayName("Follow Up By")]
        public string FollowUpBy { get; set; }

        [DisplayName("Notify Prayer Team")]
        public bool NotifyPrayerTeam { get; set; }

        [NotMapped]
        public int OrderNumber { get; set; }

        [NotMapped]
        public string PreviousId { get; set; }

        [NotMapped]
        public string NextId { get; set; }
    }

    public class PrayerRequestVM
    {
        public PrayerRequestVM()
        {
            PrayerRequest = new PrayerRequest();
            Categories = new List<PrayerRequestCategory>();
            People = new List<Person>();
            Person = new Person();
        }

        public PrayerRequest PrayerRequest { get; set; }
        public IEnumerable<PrayerRequestCategory> Categories { get; set; }
        public IEnumerable<Person> People { get; set; }
        public Person Person { get; set; }
        public string Mode { get; set; }
    }

    public class PrayerRequestsView : Pagination
    {
        public PrayerRequestsView()
        {
            PrayerRequests = new List<PrayerRequest>();
            PrayerRequestCategories = new List<PrayerRequestCategory>();
        }

        public List<PrayerRequest> PrayerRequests { get; set; }
        public List<PrayerRequestCategory> PrayerRequestCategories { get; set; }
        public int TotalPrayerRequests { get; set; }
        public int NotPrayedOverCount { get; set; }
        public string PrayerRequestType { get; set; }
        public string ReadAction { get; set; }
        public string PrayedOverAction { get; set; }
    }

    public class Pagination
    {
        public int From { get; set; }
        public int To { get; set; }
        public string Oldest { get; set; }
        public string Newest { get; set; }
        public string CurrentUrl { get; set; }
        public string FilterKeyword { get; set; }
        public int Page { get; set; }
        public int TotalPage { get; set; }
    }

    public class PrayerRequestsSummary
    {
        public PrayerRequestsSummary()
        {
            AllPrayerRequests = new List<PrayerRequest>();
            PrayerRequestsByDate = new List<PrayerRequest>();
            Categories = new List<PrayerRequestCategory>();
            AverageResponseTimes = new Dictionary<string, string>();
            StatusCounts = new Dictionary<string, StatusCounts>();
            CategoryCounts = new Dictionary<string, int>();
        }

        public List<PrayerRequest> AllPrayerRequests { get; set; }
        public List<PrayerRequest> PrayerRequestsByDate { get; set; }
        public List<PrayerRequestCategory> Categories { get; set; }
        public string DateRange { get; set; }

        // Properties for Prayer Request Senders report
        public SenderCounts SenderCounts { get; set; }

        //// Properties for Prayer Request Response Summary report
        public Dictionary<string, string> AverageResponseTimes { get; set; } // AttemptedToContact, Completed

        // Dictionaries to store counts for various statuses and categories
        public Dictionary<string, StatusCounts> StatusCounts { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; }

        // Add this property to hold follow-up status counts
        public Dictionary<string, int> FollowUpStatusCounts { get; set; }
    }

    public class StatusCounts
    {
        public int YTD { get; set; }
        public int ByDate { get; set; }
    }

    public static class StatusKeys
    {
        public const string TotalRequests = "TotalRequests";
        public const string HighPriorityRequests = "HighPriorityRequests";
        public const string ConfidentialRequests = "ConfidentialRequests";
        public const string FollowUpRequiredRequests = "FollowUpRequiredRequests";
        public const string TotalNotPrayedOver = "TotalNotPrayedOver";
        public const string HighPriorityNotPrayedOver = "HighPriorityNotPrayedOver";
        public const string ConfidentialNotPrayedOver = "ConfidentialNotPrayedOver";
        public const string FollowUpRequiredNotPrayedOver = "FollowUpRequiredNotPrayedOver";

        // Add this collection to hold all possible status keys
        public static readonly string[] AllKeys = new[]
        {
            TotalRequests,
            HighPriorityRequests,
            ConfidentialRequests,
            FollowUpRequiredRequests,
            TotalNotPrayedOver,
            HighPriorityNotPrayedOver,
            ConfidentialNotPrayedOver,
            FollowUpRequiredNotPrayedOver
        };
    }

    public class SenderCounts
    {
        public int TotalSendersYTD { get; set; }
        public int UniqueSendersYTD { get; set; }
        public int RepeatSendersYTD { get; set; }
        public int TotalSendersByDate { get; set; }
        public int UniqueSendersByDate { get; set; }
        public int RepeatSendersByDate { get; set; }
    }

    public class SidebarViewModel
    {
        public List<PrayerRequestCategory> Categories { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; }
    }
}