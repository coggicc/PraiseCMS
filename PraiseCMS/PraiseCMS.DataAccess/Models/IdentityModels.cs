using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.DataAccess.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim("FirstName", FirstName ?? string.Empty));
            userIdentity.AddClaim(new Claim("LastName", LastName ?? string.Empty));
            userIdentity.AddClaim(new Claim("PhoneVerificationCode", PhoneVerificationCode ?? string.Empty));

            return userIdentity;
        }

        //Extended Properties
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override string Id { get; set; } //Overrides base Applicationuser Id to use custom GUID.

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneVerificationCode { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime? LastAccessedDate { get; set; }

        //public bool TwoFactorEnabled { get; set; }

        public bool IsActive { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

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

        [DisplayName("External Provider")]
        public string ExternalProvider { get; set; }

        [DisplayName("External Provider")]
        public string ExternalProviderId { get; set; }

        [DisplayName("Email Verification Code")]
        public string EmailVerificationCode { get; set; }

        [DisplayName("Church Name")]
        public bool AssignedToChurch { get; set; }

        [DisplayName("Person")]
        public string PersonId { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [DisplayName("Converted to User By")]
        public string ConvertedToUserById { get; set; }

        [DisplayName("Converted to User Date")]
        public DateTime? ConvertedToUserDate { get; set; }

        public bool ShowWelcomeMessage { get; set; }

        [DisplayName("Inbox Density")]
        public string InboxDensity { get; set; } = PraiseCMS.Shared.Shared.InboxDensity.Default;

        [DisplayName("Inbox Type")]
        public string InboxType { get; set; } = PraiseCMS.Shared.Shared.InboxType.Default;

        [NotMapped]
        public List<string> UserRolesList { get; set; }

        [NotMapped]
        public Person Person { get; set; }

        public string FullName => string.Join(" ", new[] { FirstName?.Trim(), LastName?.Trim() }.Where(s => !string.IsNullOrWhiteSpace(s)));

        public string Initials => FirstName.SubstringIt(1) + " " + LastName.SubstringIt(1);

        public string Display => !string.IsNullOrEmpty(FullName.Trim()) ? FullName : Constants.DisplayDefaultText;

        public string Address => ((Address1 + " " + Address2).Trim() + ", " + City + " " + State + " " + Zip).Trim().Trim(',').Trim();
    }

    public class UserView
    {
        public UserView()
        {
            Attachments = new List<AttachmentSD>();
            Notes = new List<Note>();
            Roles = new List<ApplicationRoles>();
            CurrentUserRoles = new List<string>();
            Users = new List<ApplicationUser>();
            Relationships = new List<Relationship>();
            Groups = new List<CommunicationGroup>();
        }

        public string Panel { get; set; }
        public string Type { get; set; }
        public string Households { get; set; }
        public DonorStatus DonorStatus { get; set; }
        public string ReturnUrl { get; set; }
        public ApplicationUser User { get; set; }
        public Person Person { get; set; }
        public UserSetting Settings { get; set; }
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
        public List<ApplicationRoles> Roles { get; set; }
        public List<string> CurrentUserRoles { get; set; }
        public List<AttachmentSD> Attachments { get; set; }
        public List<Note> Notes { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public IEnumerable<CommunicationGroup> Groups { get; set; }
        public IEnumerable<Relationship> Relationships { get; set; }
    }
}