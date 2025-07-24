using PraiseCMS.DataAccess.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SubscriptionViewModel : BaseViewModel
    {
        public bool IsPaidPlan { get; set; }
        public bool IsTrialPeriod { get; set; }
        public bool IsCancelable { get; set; }
        public bool ShowActivationButton { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ActivateSubscriptionVM : ResponseModel
    {
        public ActivateSubscriptionVM()
        {
            Accounts = new List<SelectListItems>();
        }

        public string ChurchId { get; set; }
        public List<SelectListItems> Accounts { get; set; }
        [Required(ErrorMessage = "Please select a billing plan.")]
        public string BillingType { get; set; }
        [Required]
        public string DonorGUID { get; set; }
        [Required(ErrorMessage = "Please select a payment method.")]
        public string AccountGUID { get; set; }
        public decimal Amount { get; set; }
        public string MonthlyBillingAmount { get; set; }
        public string AnnualBillingAmount { get; set; }
        public string TotalAnnualBillingAmount { get; set; }
        public string FundId { get; set; }
        public bool FreeTrialAvailable { get; set; }
    }
}