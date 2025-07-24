using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("EmailTemplates")]
    public class EmailTemplate : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type ID")]
        public string TypeId { get; set; }

        [Required(ErrorMessage = "Please provide a name for the email template.")]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required]
        [AllowHtml]
        [DisplayName("Body")]
        public string Body { get; set; }

        public string Display => !string.IsNullOrWhiteSpace(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class InvitationEmailModel
    {
        [Required(ErrorMessage = "Please enter an email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a first name.")]
        [MaxLength(15)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter a last name.")]
        [MaxLength(15)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter a message.")]
        [MaxLength(1000)]
        public string Message { get; set; }
        [Required]
        public string InvitedBy { get; set; }
    }
}