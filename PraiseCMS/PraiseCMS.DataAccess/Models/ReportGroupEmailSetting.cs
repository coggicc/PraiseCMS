using PraiseCMS.DataAccess.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ReportGroupEmailSettings")]
    public class ReportGroupEmailSetting : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("User")]
        public string UserId { get; set; }

        [DisplayName("Report Group")]
        public string ReportGroupId { get; set; }

        [DisplayName("Recurrence Type")]
        public string RecurrenceType { get; set; }

        [DisplayName("Recurrence Number")]
        public string RecurrenceNumber { get; set; }

        [DisplayName("Report Group")]
        public DateTime Ending { get; set; }

        [Required]
        [DisplayName("To")]
        public string To { get; set; }

        [DisplayName("Cc")]
        public string Cc { get; set; }

        [DisplayName("Bcc")]
        public string Bcc { get; set; }

        [DisplayName("From")]
        public string From { get; set; }

        [Required]
        [DisplayName("Subject")]
        public string Subject { get; set; }

        [Required]
        [AllowHtml]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Attachment Paths")]
        public string AttachmentPaths { get; set; }
    }

    public class ReportGroupEmailSettingsView
    {
        public ReportGroupEmailSettingsView()
        {
            Users = new List<ApplicationUser>();
        }

        public ReportGroup ReportGroup { get; set; }
        public ReportGroupEmailSetting ReportGroupEmailSettings { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}