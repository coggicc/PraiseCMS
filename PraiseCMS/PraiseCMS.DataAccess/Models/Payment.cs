using PraiseCMS.DataAccess.Interfaces;
using PraiseCMS.DataAccess.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Payments")]
    public class Payment : IGivingItem
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Merchant")]
        public string Merchant { get; set; }

        [DisplayName("Merchant Id")]
        public string MerchantId { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [DisplayName("User")]
        public string UserId { get; set; }

        [Required]
        [DisplayName("Amount")]
        public decimal Amount { get; set; }

        [DisplayName("Fund")]
        public string FundId { get; set; }

        [DisplayName("Frequency")]
        public string Frequency { get; set; }

        [DisplayName("Recurring Frequency")]
        public string RecurringFrequency { get; set; }

        [Required]
        [DisplayName("Payment Method")]
        public string PaymentMethod { get; set; }

        [DisplayName("Payment Status")] //success, error, cancel, etc...
        public string PaymentStatus { get; set; }

        [DisplayName("Transaction Type")]
        public string TransactionType { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [DisplayName("Scheduled Payment Id")]
        public string ScheduledPaymentId { get; set; }

        [DisplayName("Digital Payment Method")]
        public string DigitalPaymentMethod { get; set; }

        [DisplayName("Digital Payment Type")]
        public string DigitalPaymentType { get; set; }

        [DisplayName("Transaction")]
        public string TransactionId { get; set; }

        [DisplayName("Account Schedule GUID")]
        public string AccountScheduleGUID { get; set; } //This is Nuvei's TransactionResponseModel.payment_reference_number

        [DisplayName("Payment Processing Fee")]
        public decimal ProcessingFee { get; set; }

        [DisplayName("Is Donor Paid Merchant Fee")]
        public bool DonorPaidMerchantFee { get; set; }

        public string PaymentType => DigitalPaymentType;
    }

    public class GivingViewModel
    {
        public GivingViewModel()
        {
            Payment = new Payment();
            Church = new Church();
            ScheduledPayment = new ScheduledPayment();
            Campuses = new List<SelectListItem>();
            Funds = new List<SelectListItem>();
            Accounts = new List<SelectListItems>();
        }

        public string ChurchId { get; set; }
        public string DonorGUID { get; set; }
        //public string DesignationGUID { get; set; }
        public string AccountGUID { get; set; }
        public string AccountType { get; set; }
        public string CcCvc { get; set; }
        public Payment Payment { get; set; }
        public ScheduledPayment ScheduledPayment { get; set; }
        public Church Church { get; set; }
        public List<SelectListItem> Campuses { get; set; }

        public List<SelectListItem> Funds { get; set; }

        public List<SelectListItems> Accounts { get; set; }

        public Payment LastGift { get; set; }
        [Required(ErrorMessage = "The amount must be $1.00 or greater.")]
        public string Amount { get; set; }
        public bool IncludeProcessingFee { get; set; }
    }

    public class ScheduledGiftViewModel
    {
        public ScheduledGiftViewModel()
        {
            ScheduledPayment = new ScheduledPayment();
            Campuses = new List<SelectListItem>();
            Funds = new List<SelectListItem>();
            Accounts = new List<SelectListItems>();
        }

        [Required(ErrorMessage = "The amount must be $1.00 or greater.")]
        public string Amount { get; set; }
        public string ChurchId { get; set; }
        public bool AllowDonorCoverProcessingFee { get; set; }
        public string DonorGUID { get; set; }
        public string DesignationGUID { get; set; }
        public string AccountGUID { get; set; }
        public ScheduledPayment ScheduledPayment { get; set; }

        public List<SelectListItem> Campuses { get; set; }

        public List<SelectListItem> Funds { get; set; }

        public List<SelectListItems> Accounts { get; set; }
    }

    public class SelectListItems : SelectListItem
    {
        public bool IsPrimary { get; set; }
    }

    public class ResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    //public class PaymentMethodViewModel : StewardshipBase
    public class PaymentMethodViewModel
    {
        public PaymentMethodViewModel()
        {
            PaymentMethods = new PaymentMethods();
            //StewardshipAccounts = new StewardshipAccounts();
            User = new ApplicationUser();
            UserMerchantAccount = new UserMerchantAccount();
            PaymentCard = new PaymentCard();
            PaymentAccount = new PaymentAccount();
        }

        public PaymentMethods PaymentMethods { get; set; }
        //public StewardshipAccounts StewardshipAccounts { get; set; }
        public UserMerchantAccount UserMerchantAccount { get; set; }
        public ApplicationUser User { get; set; }
        public PaymentCard PaymentCard { get; set; }
        public PaymentAccount PaymentAccount { get; set; }

        [DisplayName("Donor")]
        public string DonorGUID { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Payment Method")]
        public string PaymentMethod { get; set; }

        [DisplayName("Primary Account")]
        public bool IsPrimary { get; set; }
        public string PrimaryAccountGUID { get; set; }
        public bool IsEditable { get; set; }
    }

    public class PaymentCard
    {
        [DisplayName("Account")]
        public string AccountGUID { get; set; }

        [DisplayName("Nick Name")]
        public string NickName { get; set; }

        [DisplayName("Card Type")]
        [Required]
        public string CcType { get; set; }

        [DisplayName("Number")]
        [Required]
        public string CcNumber { get; set; }
        public string CcExpiry { get; set; }

        [DisplayName("Exp Month")]
        [Required(ErrorMessage = "Please enter an expiration month.")]
        public string CcExpMonth { get; set; }

        [DisplayName("Exp Year")]
        [Required(ErrorMessage = "Please enter an expiration year.")]
        public string CcExpYear { get; set; }

        [DisplayName("CVC")]
        public string CcCvc { get; set; }

        [DisplayName("Name")]
        [Required]
        public string CcName { get; set; }

        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }
    }

    public class PaymentAccount
    {
        [DisplayName("Account")]
        public string AccountGUID { get; set; }

        [DisplayName("Nickname")]
        public string NickName { get; set; }

        [DisplayName("Routing Number")]
        [Required]
        public string RoutingNumber { get; set; }

        [DisplayName("Account Number")]
        [Required]
        public string AccountNumber { get; set; }

        [DisplayName("Account Type")]
        [Required]
        public string AccountType { get; set; }
    }

    public class GivingDashboard
    {
        public GivingDashboard()
        {
            MyGiving = new List<MyGivingVM>();
            Payments = new List<Payment>();
            CreditCards = new List<CreditCard>();
            BankAccounts = new List<BankAccount>();
            ScheduledPayments = new List<ScheduledPayment>();
            Funds = new List<Fund>();
            Campuses = new List<Campus>();
            User = new ApplicationUser();
        }

        public ApplicationUser User { get; set; }
        public List<PaymentMethodAccount> PaymentMethods { get; set; }
        public List<Payment> Payments { get; set; }
        public List<MyGivingVM> MyGiving { get; set; }
        public List<ScheduledPayment> ScheduledPayments { get; set; }
        public List<CreditCard> CreditCards { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Campus> Campuses { get; set; }
        public string PrimaryAccountGUID { get; set; }
        public string PersonId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string FundId { get; set; }
        public string CampusId { get; set; }
        public string CardExpiryNotification { get; set; }
        public string CardExpiredNotification { get; set; }
        public bool IsPrimary { get; set; }
        public string ExpiredCardAccountGUID { get; set; }
        public Payment LastGift { get; set; }
    }

    public class PaymentMethodDashboard
    {
        public PaymentMethodDashboard()
        {
            CreditCards = new List<CreditCard>();
            BankAccounts = new List<BankAccount>();
        }

        public List<CreditCard> CreditCards { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
    }

    public class CardExpirationModel
    {
        public bool IsExpiring { get; set; }
        public string Message { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class GivingSummaryDashboard : IGivingDashboard
    {
        public GivingSummaryDashboard()
        {
            DigitalGiving = new List<Payment>();
            OfflineGiving = new List<OfflineGiving>();
            TotalGiving = new List<TotalGivingItem>();
            Funds = new List<Fund>();
            Campuses = new List<Campus>();

            NoCampusTotals = new GivingByCampus();
            CampusTotals = new List<GivingByCampus>();
            FundTotals = new List<GivingByFund>();
        }

        public List<Payment> DigitalGiving { get; set; }
        public List<OfflineGiving> OfflineGiving { get; set; }
        public List<TotalGivingItem> TotalGiving { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Campus> Campuses { get; set; }
        //public Fund CurrentFund { get; set; }
        public Campus CurrentCampus { get; set; }
        public string DateRange { get; set; }

        public GivingByCampus NoCampusTotals { get; set; }
        public List<GivingByCampus> CampusTotals { get; set; }
        public List<GivingByFund> FundTotals { get; set; }

        public FundData TithesFundData { get; set; }
        public FundData GeneralFundData { get; set; }
        public FundData MissionsFundData { get; set; }
    }

    public abstract class GivingSummaryBase
    {
        public string TotalGiving { get; set; }
        public string DigitalGiving { get; set; }
        public string OfflineGiving { get; set; }
        public string OnlineGiving { get; set; }
        public string TextMessageGiving { get; set; }
        public string OfferingPlateGiving { get; set; }
        public string DropOffGiving { get; set; }
        public string MailedGiving { get; set; }

        // DigitalGiving breakdown
        public string OnlineCardGiving { get; set; }
        public string OnlineAchGiving { get; set; }
        public string TextMessageCardGiving { get; set; }
        public string TextMessageAchGiving { get; set; }

        // OfflineGiving breakdown
        public string OfferingPlateCashGiving { get; set; }
        public string OfferingPlateCheckGiving { get; set; }
        public string DropOffCashGiving { get; set; }
        public string DropOffCheckGiving { get; set; }
        public string MailedCashGiving { get; set; }
        public string MailedCheckGiving { get; set; }
    }

    public class GivingByCampus : GivingSummaryBase
    {
        public string CampusId { get; set; }
    }

    public class GivingByFund : GivingSummaryBase
    {
        public string FundId { get; set; }
    }

    public class TotalGivingItem
    {
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CampusId { get; set; }
        public string FundId { get; set; }
        public string UserId { get; set; }
        public string PersonId { get; set; }
        public string Frequency { get; set; }
        public string PaymentType { get; set; }
    }

    public class FundData
    {
        public string FundUrl { get; set; }
        public string FundAmount { get; set; }
        public string BgClass { get; set; }
        public string IconClass { get; set; }
    }

    public class CompleteViewModel : ResponseModel
    {
        public Church Church { get; set; }
        public string FundId { get; set; }
        public string CampusName { get; set; }
        public decimal? ProcessingFee { get; set; }
        public string PaymentAmount { get; set; }
        public bool Guest { get; set; }
        public string PaymentOccurrence { get; set; }
        public List<SelectListItem> Campuses { get; set; }
        public List<SelectListItem> Funds { get; set; }
        public List<SelectListItems> Accounts { get; set; }
    }

    #region Donor Demographics Report
    public class DonorsModel
    {
        public DonorsModel()
        {
            People = new List<Person>();
            TotalGiving = new List<TotalGivingItem>();
            PeopleByDate = new List<Person>();
            TotalGivingByDate = new List<TotalGivingItem>();
            GenderStats = new Dictionary<string, GenderStat>();
            MaritalStatusStats = new Dictionary<string, MaritalStatusStat>();
            AgeGroupStats = new Dictionary<string, AgeGroupStat>();
            EthnicTypeStats = new Dictionary<string, EthnicTypeStat>();
            EducationTypeStats = new Dictionary<string, EducationTypeStat>();
            EmploymentStatusStats = new Dictionary<string, EmploymentStatusStat>();
        }

        public List<TotalGivingItem> TotalGiving { get; set; }
        public List<Person> People { get; set; }
        public List<TotalGivingItem> TotalGivingByDate { get; set; }
        public List<Person> PeopleByDate { get; set; }
        public string DateRange { get; set; }
        public Dictionary<string, GenderStat> GenderStats { get; set; }
        public Dictionary<string, MaritalStatusStat> MaritalStatusStats { get; set; }
        public Dictionary<string, AgeGroupStat> AgeGroupStats { get; set; }
        public Dictionary<string, EthnicTypeStat> EthnicTypeStats { get; set; }
        public Dictionary<string, EducationTypeStat> EducationTypeStats { get; set; }
        public Dictionary<string, EmploymentStatusStat> EmploymentStatusStats { get; set; }

        // Totals for all genders combined
        public int TotalGenderCount { get; set; }
        public decimal TotalGenderAverageDonation { get; set; }
        public double TotalGenderPercentage { get; set; }

        // Totals for all marital statuses combined
        public int TotalMaritalStatusCount { get; set; }
        public decimal TotalMaritalStatusAverageDonation { get; set; }
        public double TotalMaritalStatusPercentage { get; set; }

        // Totals for all age groups combined
        public int TotalAgeGroupCount { get; set; }
        public decimal TotalAgeGroupAverageDonation { get; set; }
        public double TotalAgeGroupPercentage { get; set; }

        // Totals for all ethnic types combined
        public int TotalEthnicTypeCount { get; set; }
        public decimal TotalEthnicTypeAverageDonation { get; set; }
        public double TotalEthnicTypePercentage { get; set; }

        // Totals for all education types combined
        public int TotalEducationTypeCount { get; set; }
        public decimal TotalEducationTypeAverageDonation { get; set; }
        public double TotalEducationTypePercentage { get; set; }

        // Totals for all employment statuses combined
        public int TotalEmploymentStatusCount { get; set; }
        public decimal TotalEmploymentStatusAverageDonation { get; set; }
        public double TotalEmploymentStatusPercentage { get; set; }
    }

    public class GenderStat
    {
        public string Gender { get; set; }  // Optional, helpful when debugging or to display gender
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageDonation { get; set; }
    }

    public class MaritalStatusStat
    {
        public string MaritalStatus { get; set; }  // Optional, helpful when debugging or to display marital status name
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageDonation { get; set; }
    }

    public class AgeGroupStat
    {
        public string AgeGroup { get; set; }  // Optional, helpful when debugging or to display age group name
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageDonation { get; set; }
    }

    public class EthnicTypeStat
    {
        public string EthnicType { get; set; }  // Optional, helpful when debugging or to display ethnic type name
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageDonation { get; set; }
    }

    public class EducationTypeStat
    {
        public string EducationType { get; set; }  // Optional, helpful when debugging or to display education type name
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageDonation { get; set; }
    }

    public class EmploymentStatusStat
    {
        public string EmploymentStatus { get; set; }  // Optional, helpful when debugging or to display employment status name
        public int Count { get; set; }
        public double Percentage { get; set; }
        public decimal AverageDonation { get; set; }
    }
    #endregion

    #region Donor Status Report
    public class DonorStatusReportViewModel
    {
        public DonorStatusReportViewModel()
        {
            People = new List<Person>();
            TotalGiving = new List<TotalGivingItem>();
            PeopleByDate = new List<Person>();
            TotalGivingByDate = new List<TotalGivingItem>();
        }

        public List<TotalGivingItem> TotalGiving { get; set; }
        public List<Person> People { get; set; }
        public List<TotalGivingItem> TotalGivingByDate { get; set; }
        public List<Person> PeopleByDate { get; set; }
        public string DateRange { get; set; }

        public int TotalDonors { get; set; }
        public int FirstTimeDonorsCount { get; set; }
        public double FirstTimePercentage { get; set; }
        public decimal FirstTimeAverageDonation { get; set; }
        public int SecondTimeDonorsCount { get; set; }
        public double SecondTimePercentage { get; set; }
        public decimal SecondTimeAverageDonation { get; set; }
        public int OccasionalDonorsCount { get; set; }
        public double OccasionalPercentage { get; set; }
        public decimal OccasionalAverageDonation { get; set; }
        public int RegularDonorsCount { get; set; }
        public double RegularPercentage { get; set; }
        public decimal RegularAverageDonation { get; set; }
        public int RecurringDonorsCount { get; set; }
        public double RecurringPercentage { get; set; }
        public decimal RecurringAverageDonation { get; set; }
        public int InActiveDonorsCount { get; set; }
        public double InActivePercentage { get; set; }
        public decimal InActiveAverageDonation { get; set; }
        public decimal TotalAverageDonation { get; set; }

        public double TotalPercentage { get; set; }
        public int TotalDonorCount { get; set; }
        public int FirstTimeDonorCount { get; set; }
        public int RepeatDonorCount { get; set; }
    }
    #endregion

    public class RefundModel
    {
        public Payment Payment { get; set; }
        //public RefundReasonTypes RefundReasonTypes { get; set; }
        //public Transaction Transaction { get; set; }
        public string Fund { get; set; }
        public string PaymentMethod { get; set; }
        public string RefundReasonId { get; set; }
        public string TransactionId { get; set; }
        public string TransactionGUID { get; set; }
    }

    public class PersonDigitalGiving
    {
        public Payment Payment { get; set; }
        public Person Person { get; set; }
    }
}