using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SmsMessage")]
    public class SmsMessage
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }

        [Required]
        [DisplayName("To")]
        public string To { get; set; }

        [Required]
        [AllowHtml]
        [StringLength(8000)]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Sent")]
        public bool Sent { get; set; }

        [StringLength(1000)]
        [DisplayName("Error")]
        public string Error { get; set; }

        [StringLength(15)]
        [DisplayName("Error")]
        public string ErrorCode { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
    }
}