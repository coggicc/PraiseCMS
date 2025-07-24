using System.ComponentModel.DataAnnotations;

namespace PraiseCMS.DataAccess.Models
{
    public class GivingAmountModel
    {
        public string ChurchId { get; set; }
        public string Amount { get; set; }
        public string Fund { get; set; }
    }

    public class GivingRegisterModel
    {
        public string Id { get; set; }
        public string ChurchId { get; set; }
        public string CampusId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public string Username { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Required]
        public string ConfirmPassword { get; set; }
        public string ProfileImage { get; set; }
        public string Phone { get; set; }
        public string LoginProvider { get; set; }
        public string RegisterVia { get; set; }
        public string ExternalProviderId { get; set; }
    }

    public class VerificationCodeModel
    {
        public string Phone { get; set; }

        [Display(Name = "Verification Code")]
        [Required]
        public string VerificationCode { get; set; }
    }
}