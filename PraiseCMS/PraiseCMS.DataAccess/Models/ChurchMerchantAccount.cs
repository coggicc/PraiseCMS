using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ChurchMerchantAccounts")]
    public class ChurchMerchantAccount : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string Merchant { get; set; }
        public string MerchantAccountId { get; set; }
        public string ChurchId { get; set; }
        public bool IsActive { get; set; }
        public string BusinessType { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TaxId { get; set; }
        public string MerchantCategoryCode { get; set; }
        public string BankAccountType { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string BusinessWebsite { get; set; }

        [StringLength(100)]
        public string RespContactFirstName { get; set; }

        [StringLength(100)]
        public string RespContactLastName { get; set; }

        [StringLength(50)]
        public string RespContactPhone { get; set; }

        [StringLength(75)]
        public string RespContactEmail { get; set; }

        public DateTime? RespContactDOB { get; set; }

        public int? RespContactSSNLastFour { get; set; }

        public string RespContactSSN { get; set; }

        [StringLength(20)]
        public string RespContactDLN { get; set; }

        [StringLength(100)]
        public string RespContactAddress1 { get; set; }

        [StringLength(100)]
        public string RespContactAddress2 { get; set; }

        [StringLength(100)]
        public string RespContactCity { get; set; }

        [StringLength(50)]
        public string RespContactState { get; set; }

        [StringLength(10)]
        public string RespContactZip { get; set; }

        [DisplayName("Card Processing Fee")]
        public decimal CardProcessingFee { get; set; }

        [DisplayName("ACH Processing Fee")]
        public decimal ACHProcessingFee { get; set; }

        [StringLength(150)]
        public string ApiUsername { get; set; }

        [StringLength(150)]
        public string ApiPassword { get; set; }

        [StringLength(150)]
        public string Username { get; set; }

        [StringLength(150)]
        public string Password { get; set; }

        public string CorrelationId { get; set; }

        public string TermsAndConditionsId { get; set; }

        public string TermsAndConditionsUrl { get; set; }

        public string RespContactFullName => RespContactFirstName + " " + RespContactLastName;

        public string RespContactAddress => ((RespContactAddress1 + " " + RespContactAddress2).Trim() + ", " + RespContactCity + ", " + RespContactState + " " + RespContactZip).Trim().Trim(',').Trim();

        public string RespContactDisplay => !string.IsNullOrEmpty(RespContactFullName) ? RespContactFullName : "[No Responsible Contact Name Defined]";

        public string FormattedRespContactDOB
        {
            get
            {
                if (RespContactDOB.HasValue)
                {
                    return RespContactDOB.Value.ToShortDateString();
                }
                else
                {
                    // Handle the case where RespContactDOB is null
                    return string.Empty;
                }
            }
        }
    }

    public class ChurchMerchantAccountVM
    {
        public ChurchMerchantAccount Account { get; set; }
        public Church Church { get; set; }
        public string CorrelationId { get; set; }
        public string TermsAndConditionsId { get; set; }
        public string TermsAndConditionsUrl { get; set; }
    }
}