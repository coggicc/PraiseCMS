using PraiseCMS.DataAccess.Interfaces;
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
    [Table("Funds")]
    public class Fund : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [Required(ErrorMessage = "Please enter the fund name.")]
        [StringLength(50)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [StringLength(1000)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Tax Deductible")]
        public bool IsTaxDeductible { get; set; }

        [DisplayName("Expiration Date")]
        public DateTime? ExpirationDate { get; set; }

        //[StringLength(250)]
        //[DisplayName("Designation GUID")]
        //public string DesignationGUID { get; set; }

        [StringLength(20)]
        [DisplayName("Designation Id")]
        public string DesignationId { get; set; }

        [DisplayName("Closed")]
        public bool Closed { get; set; }

        [DisplayName("Hidden")]
        public bool Hidden { get; set; }

        [DisplayName("Deleted")]
        public bool IsDeleted { get; set; }

        [DisplayName("QRCodeLink")]
        public string QRCodeLink { get; set; }

        [AllowHtml]
        public string GivingThankYouText { get; set; }

        [DisplayName("Digital Allowed")]
        public bool IsDigitalAllowed { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        public string QRCodeLinkToken => !string.IsNullOrEmpty(QRCodeLink) ? Constants.GenerateToken(QRCodeLink) : null;

        [DisplayName("Default Fund")]
        public bool IsDefaultFund { get; set; }
    }

    public class OnboardFundViewModel
    {
        public Fund Fund { get; set; }
        public List<string> CommonFunds { get; set; }
        public bool EnableCloseOrHidden { get; set; }
        public bool AddToDigitalGiving { get; set; }
        public bool GenerateQRCode { get; set; }
    }

    public class FundReportDashboard : IGivingDashboard
    {
        public FundReportDashboard()
        {
            DigitalGiving = new List<Payment>();
            OfflineGiving = new List<OfflineGiving>();
            TotalGiving = new List<TotalGivingItem>();
            Campuses = new List<Campus>();
            CampusTotals = new List<GivingByCampus>();
            FundTotals = new List<GivingByFund>();
        }

        public List<Payment> DigitalGiving { get; set; }
        public List<OfflineGiving> OfflineGiving { get; set; }
        public List<TotalGivingItem> TotalGiving { get; set; }
        public List<Campus> Campuses { get; set; }
        public Fund Fund { get; set; }
        public string DateRange { get; set; }
        public string PaymentMethodType { get; set; }
        public GivingByCampus NoCampusTotals { get; set; }
        public List<GivingByCampus> CampusTotals { get; set; }
        public GivingByFund SingleFundTotals { get; set; }
        public List<GivingByFund> FundTotals { get; set; }
    }

    public class FundReportFilter
    {
        public FundReportFilter()
        {
            Funds = new List<Fund>();
        }

        public List<Fund> Funds { get; set; }
        [Required(ErrorMessage = "Please select a fund.")]
        public string FundId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}