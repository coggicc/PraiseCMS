using System;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class PaymentConfirmationViewModel
    {
        public string CurrentChurchID { get; set; }
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
        public string Gift { get; set; }
        public string FundId { get; set; }
        public DateTime RecurringStartDate { get; set; }
        public DateTime RecurringEndDate { get; set; }
        public string FrequencyId { get; set; }
    }
}