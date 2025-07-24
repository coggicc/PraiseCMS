using PraiseCMS.DataAccess.Models.ViewModels.Base;
using PraiseCMS.Shared.Methods;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ChuchGivingAccountViewModel : BaseViewModel
    {
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
        [Required(ErrorMessage = "Please select a bank account type.")]
        public string BankAccountType { get; set; }
        [Required(ErrorMessage = "Please provide an account number.")]
        public string AccountNumber { get; set; }
        [Required(ErrorMessage = "Please provide a routing number.")]
        public string RoutingNumber { get; set; }
        public string BusinessWebsite { get; set; }

        [StringLength(40)]
        [Required(ErrorMessage = "Please provide a first name.")]
        public string RespContactFirstName { get; set; }

        [StringLength(40)]
        [Required(ErrorMessage = "Please provide a last name.")]
        public string RespContactLastName { get; set; }

        [StringLength(40)]
        [Required(ErrorMessage = "Please provide a phone number.")]
        public string RespContactPhone { get; set; }

        [StringLength(75)]
        [Required(ErrorMessage = "Please provide an email.")]
        public string RespContactEmail { get; set; }

        [Required(ErrorMessage = "Please provide a date of birth.")]
        public string RespContactDOB { get; set; }

        [Required(ErrorMessage = "Please provide a SSN.")]
        public string RespContactSSN { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Please provide a driver's license number.")]
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

        public string ApiUsername { get; set; }

        public string ApiPassword { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string RespContactFullName => RespContactFirstName + " " + RespContactLastName;

        public string RespContactAddress => ((RespContactAddress1 + " " + RespContactAddress2).Trim() + ", " + RespContactCity + ", " + RespContactState + " " + RespContactZip).Trim().Trim(',').Trim();

        public string RespContactDisplay => !string.IsNullOrEmpty(RespContactFullName) ? RespContactFullName : "[No Responsible Contact Name Defined]";
        public string RespContactPhoneDisplay => !string.IsNullOrEmpty(RespContactPhone) ? RespContactPhone.PhoneFriendly() : "[No Phone Provided]";

        public string RespContactSSNLastFourDisplay
        {
            get
            {
                // Ensure that RespContactSSN is not null and has at least four characters
                if (!string.IsNullOrEmpty(RespContactSSN) && RespContactSSN.Length >= 4)
                {
                    // Take the last four characters of RespContactSSN
                    string lastFourDigits = RespContactSSN.Substring(RespContactSSN.Length - 4);

                    // Insert asterisks and hyphens before the last four digits
                    return $"***-**-{lastFourDigits}";
                }
                // Handle the case where RespContactSSN is null or has less than four characters
                return string.Empty; // or any default value you prefer
            }
        }
    }
}
