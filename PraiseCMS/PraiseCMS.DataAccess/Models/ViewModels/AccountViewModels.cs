using Microsoft.AspNet.Identity;
using PraiseCMS.DataAccess.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter the verification code first.")]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }
        public string Token { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        //[Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        //[Required]
        [Display(Name = "Phone")]
        [EmailAddress]
        public string Phone { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string ResponseStatus { get; set; }
        public string LoginVia { get; set; }
    }

    public class PhoneLoginViewModel
    {
        public PhoneLoginViewModel()
        {
            Campuses = new List<Campus>();
        }

        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        public string VerificationCode { get; set; }
        public Church Church { get; set; }
        public Payment Payment { get; set; }

        public List<Campus> Campuses { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }

    public class PhoneSignUpViewModel
    {
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public int VerificationCode { get; set; }
        public string Title { get; set; }
    }

    [EmailOrPhoneRequired]
    public class SignUpViewModel
    {
        public SignUpViewModel()
        {
            Church = new Church();
            Campus = new Campus();
            Payment = new Payment();
            Campuses = new List<SelectListItem>();
            Funds = new List<SelectListItem>();
        }

        public Church Church { get; set; }
        public Campus Campus { get; set; }

        [Required]
        public Payment Payment { get; set; }

        public List<SelectListItem> Campuses { get; set; }

        public List<SelectListItem> Funds { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string VerificationCode { get; set; }
        public string Title { get; set; }
        public bool AccountFound { get; set; }
        public string ResponseStatus { get; set; }
        public string RegisterVia { get; set; }
        public bool IsValid { get; set; }
        public bool ValidateCaptcha { get; set; }
        public string Amount { get; set; }
        public string PlanType { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
    public class EmailOrPhoneRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (SignUpViewModel)validationContext.ObjectInstance;

            if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.Phone))
            {
                return new ValidationResult("Please enter either an email address or a phone number.");
            }

            return ValidationResult.Success;
        }
    }

    public class GuestPaymentModel
    {
        public GuestPaymentModel()
        {
            Church = new Church();
            Accounts = new List<SelectListItems>();
            Campuses = new List<SelectListItem>();
            Funds = new List<SelectListItem>();
        }
        public string Amount { get; set; }
        public Church Church { get; set; }
        public string CampusId { get; set; }
        public string FundId { get; set; }
        public List<SelectListItems> Accounts { get; set; }
        public List<SelectListItem> Campuses { get; set; }
        public List<SelectListItem> Funds { get; set; }
        public PaymentCard PaymentCard { get; set; }
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Zip { get; set; }
        public string FullName => FirstName + " " + LastName;
        public bool CreateAccount { get; set; }
        public bool IncludeProcessingFee { get; set; }
        public bool HasMerchantAccount { get; set; } = true;
    }

    public class GivingSignUpViewModel
    {
        public GivingSignUpViewModel()
        {
            Church = new Church();
            Payment = new Payment();
            ScheduledPayment = new ScheduledPayment();
            Campuses = new List<SelectListItem>();
            Funds = new List<SelectListItem>();
            Accounts = new List<SelectListItems>();
        }
        public Church Church { get; set; }
        public Payment Payment { get; set; }
        public ScheduledPayment ScheduledPayment { get; set; }
        public List<SelectListItem> Campuses { get; set; }
        public List<SelectListItem> Funds { get; set; }
        public List<SelectListItems> Accounts { get; set; }
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        public string DonorGUID { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string VerificationCode { get; set; }
        public string Title { get; set; }
        public bool AccountFound { get; set; }
        public bool HasMerchantAccount { get; set; }
        public string ResponseStatus { get; set; }
        public string RegisterVia { get; set; }
        public bool IncludeProcessingFee { get; set; }
        public bool IsValid { get; set; }
        public bool IsGuest { get; set; } = true;
        public string Amount { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class SetupPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class PinLoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        public bool IsEmailPhone { get; set; }
    }

    public class CreateAccountViewModel
    {
        public CreateAccountViewModel()
        {
            Result = new IdentityResult();
        }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please enter the first name.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Please enter the last name.")]
        public string LastName { get; set; }
        [RegularExpressionIfProvided(@"^((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}$", ErrorMessage = "Please enter a valid phone number.")]
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Type { get; set; }
        [Required(ErrorMessage = "Please select a role.")]
        public string TypeId { get; set; }
        public string Campus { get; set; }
        public IdentityResult Result { get; set; }
    }

    public class UsersViewModel
    {
        public UsersViewModel()
        {
            ApplicationUsers = new List<ApplicationUser>();
            UsersWithRoles = new List<UsersWithRoles>();
            UserSettings = new List<UserSetting>();
            ApplicationSingleRole = new ApplicationRoles();
            ApplicationRoles = new Dictionary<string, List<ApplicationRoles>>();
        }

        public List<ApplicationUser> ApplicationUsers { get; set; }
        public List<UsersWithRoles> UsersWithRoles { get; set; }
        public List<UserSetting> UserSettings { get; set; }
        public ApplicationRoles ApplicationSingleRole { get; set; }
        public Dictionary<string, List<ApplicationRoles>> ApplicationRoles { get; set; }
    }

    [NotMapped]
    public class UsersWithRoles : ApplicationUser
    {
        public string UserRoles { get; set; }
        public string ProfileImage { get; set; }
    }
}