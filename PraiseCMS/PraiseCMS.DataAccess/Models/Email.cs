using PraiseCMS.Shared.Shared;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Emails")]
    public class Email
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("To")]
        public string To { get; set; }

        [DisplayName("Cc")]
        public string Cc { get; set; }

        [DisplayName("Bcc")]
        public string Bcc { get; set; }

        [DisplayName("Subject")]
        public string Subject { get; set; }

        [Required]
        [AllowHtml]
        [DisplayName("Message")]
        public string Message { get; set; }

        [AllowHtml]
        [DisplayName("Message Text")]
        public string Text { get; set; }

        public string DisplayMessage => !string.IsNullOrEmpty(Text) ? Text : "[No Message Defined]";

        [DisplayName("Sent")]
        public bool Sent { get; set; }

        [DisplayName("Status")]
        public string Status { get; set; }

        [DisplayName("Is Support Email")]
        public bool IsSupportEmail { get; set; }

        [DisplayName("Viewed Count")]
        public int ViewedCount { get; set; }

        [DisplayName("Attachments")]
        public string Attachments { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }
    }

    public class SupportEmail
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string FromEmail { get; set; }
        public string Priority { get; set; } = Priorities.Low;
        [Required(ErrorMessage = "Please add your message.")]
        [AllowHtml]
        public string Message { get; set; }
    }
}