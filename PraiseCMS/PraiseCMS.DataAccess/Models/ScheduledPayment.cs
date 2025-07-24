using Foolproof;
using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ScheduledPayments")]
    public class ScheduledPayment : BaseModel
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

        [Required]
        [DisplayName("User")]
        public string UserId { get; set; }

        [Required]
        [DisplayName("Amount")]
        public decimal? Amount { get; set; }

        [DisplayName("Fund")]
        public string FundId { get; set; }

        [DisplayName("Frequency")]
        public string Frequency { get; set; }

        [Required(ErrorMessage = "Please choose a giving frequency.")]
        [DisplayName("Recurring Frequency")]
        public string RecurringFrequency { get; set; }

        [Required(ErrorMessage = "Please choose a starting date.")]
        [DisplayName("Starting On")]
        public DateTime? RecurringStartDate { get; set; }

        [RequiredIf("GiftEndingReason", Operator.EqualTo, GiftEndingReasons.OnASpecificDate, ErrorMessage = "Please choose an ending date.")]
        [DisplayName("Ending Date")]
        public DateTime? RecurringEndDate { get; set; }

        [DisplayName("Next Charge Date")]
        public DateTime? NextChargeDate { get; set; }

        [DisplayName("Number of Payments Made")]
        public int? PaymentsMade { get; set; }

        [DisplayName("Is Processed")]
        public bool IsProcessed { get; set; }

        [RequiredIf("GiftEndingReason", Operator.EqualTo, GiftEndingReasons.AfterMaxNumberofGifts, ErrorMessage = "Please specify the number of gifts.")]
        [DisplayName("Max Number of Gifts")]
        public int? MaxGifts { get; set; }

        [Required]
        [DisplayName("Payment Method")]
        public string PaymentMethod { get; set; }

        [DisplayName("Transaction Type")]
        public string TransactionType { get; set; }

        [DisplayName("Gift Ending Reason")]
        [Required]
        public string GiftEndingReason { get; set; }

        [DisplayName("Last Charge Date")]
        public DateTime? LastChargeDate { get; set; }

        [DisplayName("Include Processing Fee With Payment")]
        public bool IncludeProcessingFee { get; set; }

        public bool IsActive { get; set; }
    }
}