using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SubscriptionTransactions")]
    public class SubscriptionTransaction
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Subscription Id")]
        public string SubscriptionId { get; set; }

        [DisplayName("Transaction Date")]
        public DateTime TransactionDate { get; set; }

        [DisplayName("Billing Type")]
        public string BillingType { get; set; }

        [DisplayName("Payment Method")]
        public string PaymentMethod { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Amount")]
        public decimal Amount { get; set; }

        [DisplayName("Transaction")]
        public string TransactionId { get; set; }

        [DisplayName("Account Schedule GUID")]
        public string AccountScheduleGUID { get; set; } //This is Nuvei's TransactionResponseModel.payment_reference_number
    }

    public class SubscriptionTransactionVM
    {
        public SubscriptionTransaction Transaction { get; set; }
        public Church Church { get; set; }
    }
}