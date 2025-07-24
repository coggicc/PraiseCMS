using System;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class MyGivingVM
    {
        public DateTime CreatedDate { get; set; }
        public string FundId { get; set; }
        public string CampusId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string CheckNumber { get; set; }
        public string OfflinePaymentMethod { get; set; }
        public string PersonId { get; set; }
    }

    public class ChurchPaymentsVM
    {
        public DateTime CreatedDate { get; set; }
        public string Fund { get; set; }
        public string Campus { get; set; }
        public decimal Amount { get; set; }
        public decimal ProcessingFee { get; set; }
        public string TransactionId { get; set; }
    }

    public class ChurchPaymentsDashboard
    {
        public ChurchPaymentsDashboard()
        {
            Payments = new List<ChurchPaymentsVM>();
            Funds = new List<Fund>();
            Campuses = new List<Campus>();
        }

        public List<ChurchPaymentsVM> Payments { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Campus> Campuses { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string FundId { get; set; }
        public string CampusId { get; set; }
    }

    public class ChurchDepositSummaryVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string DepositID { get; set; }
        public string CreditID { get; set; }
        public string DepositDate { get; set; }
        public string From { get; set; }
        public string Description { get; set; }
        public string GrossAmount { get; set; }
        public string NetAmount { get; set; }
    }

    public class DepositSummaryDashboard
    {
        public DepositSummaryDashboard()
        {
            //Deposits = new List<StewardshipDeposit>();
        }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        //public List<StewardshipDeposit> Deposits { get; set; }
    }

    public class DepositDetailsDashboard
    {
        public DepositDetailsDashboard()
        {
            //DepositDetails = new List<StewardshipDepositDetail>();
        }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        //public List<StewardshipDepositDetail> DepositDetails { get; set; }
    }
}