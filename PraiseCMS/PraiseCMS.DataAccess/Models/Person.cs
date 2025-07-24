using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("People")]
    public class Person : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        private string _firstName;
        [Required]
        [StringLength(50)]
        [DisplayName("First Name")]
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                UpdateFullName();
                UpdateDisplay();
            }
        }

        private string _lastName;
        [Required]
        [StringLength(50)]
        [DisplayName("Last Name")]
        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                UpdateFullName();
                UpdateDisplay();
            }
        }

        [StringLength(50)]
        [DisplayName("Email")]
        public string Email { get; set; }

        [StringLength(50)]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [DisplayName("IsActive")]
        public bool IsActive { get; set; } = true;

        [DisplayName("Profile Image")]
        public string ProfileImage { get; set; }

        [DisplayName("Address 1")]
        public string Address1 { get; set; }

        [DisplayName("Address 2")]
        public string Address2 { get; set; }

        [DisplayName("City")]
        public string City { get; set; }

        [DisplayName("State")]
        public string State { get; set; }

        [DisplayName("Zip")]
        public string Zip { get; set; }

        [DisplayName("Address")]
        public string Address => ((Address1 + " " + Address2).Trim() + ", " + City + " " + State + " " + Zip).Trim().Trim(',').Trim();

        public int? Age
        {
            get
            {
                if (DOB.IsNotNullOrEmptyOrDbNull())
                {
                    return DateTime.Now.Year - Convert.ToDateTime(DOB).Year;
                }

                return null;
            }
        }

        public string ProfileImageURL
        {
            get
            {
                if (ProfileImage.IsNotNullOrEmptyOrDbNull())
                {
                    return AwsHelpers.AmazonLink(ProfileImage, "Uploads/ProfileImages");
                }

                return $"{ApplicationCache.Instance.SiteConfiguration.Url}/Content/assets/media/users/blank.png".Trim();
            }
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [DisplayName("Primary Language")]
        public string PrimaryLanguage { get; set; }

        [DisplayName("Birthday")]
        public DateTime? DOB { get; set; }

        [DisplayName("Gender")]
        public string Gender { get; set; }

        [DisplayName("Marital Status")]
        public string MaritalStatus { get; set; }

        [DisplayName("Ethnicity")]
        public string Ethnicity { get; set; }

        [DisplayName("Education")]
        public string Education { get; set; }

        [DisplayName("EmploymentStatus")]
        public string EmploymentStatus { get; set; }

        [DisplayName("Family Size")]
        public int? FamilySize { get; set; }

        public DateTime? DeceasedDate { get; set; }

        public DateTime? BaptismDate { get; set; }

        [DisplayName("Member Visitor Status")]
        public string MemberVisitorStatus { get; set; }

        [DisplayName("First Visit Date")]
        public DateTime? FirstVisitDate { get; set; }

        [DisplayName("Membership Date")]
        public DateTime? MembershipDate { get; set; }

        [NotMapped]
        public string UserId { get; set; }

        [NotMapped]
        public string Initials => FirstName.SubstringIt(1) + " " + LastName.SubstringIt(1);

        [NotMapped]
        public string FullName { get; set; }

        [NotMapped]
        public string Display { get; set; }

        private void UpdateFullName()
        {
            FullName = $"{FirstName} {LastName}";
        }

        private void UpdateDisplay()
        {
            Display = !string.IsNullOrEmpty(FullName.Trim()) ? FullName : Constants.DisplayDefaultText;
        }

        [NotMapped]
        public string DisplayWithAddress =>
            FullName.IsNotNullOrEmpty()
                ? Address1.IsNotNullOrEmpty() ? $"{FullName} ({Address1})" : FullName
                : Constants.DisplayDefaultText;

        [NotMapped]
        public string Households { get; set; }

        [NotMapped]
        public DonorStatus DonorStatus { get; set; }
    }

    public class PeopleDashboard
    {
        public PeopleDashboard()
        {
            People = new List<Person>();
        }

        public List<Person> People { get; set; }
    }

    public class DeletePersonViewModel
    {
        public string SelectedPersonId { get; set; }
        public List<SelectListItem> People { get; set; }
    }
}