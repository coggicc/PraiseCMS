using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.ComponentModel;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Leads")]
    public class Lead : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string Church { get; set; }

        [StringLength(50)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [DisplayName("Email")]
        public string Email { get; set; }

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Please enter a valid phone number.")]
        [DisplayName("Phone Number")]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [DisplayName("IsActive")]
        public bool IsActive { get; set; }

        [StringLength(100)]
        [DisplayName("Address Line 1")]
        public string Address1 { get; set; }

        [StringLength(100)]
        [DisplayName("Address Line 2")]
        public string Address2 { get; set; }

        [StringLength(50)]
        [DisplayName("City")]
        public string City { get; set; }

        [StringLength(50)]
        [DisplayName("State")]
        public string State { get; set; }

        [StringLength(10)]
        [DisplayName("Zip")]
        public string Zip { get; set; }

        [DisplayName("Number")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [DisplayName("Demo Date")]
        public DateTime? DemoDate { get; set; }

        [Required]
        [DisplayName("Status")]
        public int Status { get; set; }

        [NotMapped]
        public string Message { get; set; }

        //This field will be used as a honey pot to catch and stop bots from spamming the form.
        [NotMapped]
        public string Phone { get; set; }

        public string FullName
        {
            get
            {
                var name = $"{FirstName} {LastName}";
                return name.IsNotNullOrEmpty() ? name.Trim() : Constants.DisplayDefaultText;
            }
        }

        public string Display => Church.IsNotNullOrEmpty() ? Church.Trim() : "[No Church Name Defined]";
    }
}