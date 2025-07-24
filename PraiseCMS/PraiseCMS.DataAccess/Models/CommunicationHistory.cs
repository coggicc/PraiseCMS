using Foolproof;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("CommunicationHistory")]
    public class CommunicationHistory
    {
        [DisplayName("Id")]
        public string Id { get; set; }

        [DisplayName("Tag Id")]
        public string TagId { get; set; }

        [AllowHtml]
        [DisplayName("Message")]
        public string Message { get; set; }

        [RequiredIf("CommunicationMethod", Operator.EqualTo, (int)ContactMethod.Email, ErrorMessage = "Please enter a subject.")]
        [DisplayName("Subject")]
        public string Subject { get; set; }

        [DisplayName("Communication Method")]
        public int CommunicationMethod { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [DisplayName("Success")]
        public bool IsSuccess { get; set; }
    }

    public class EmailModel
    {
        public EmailModel()
        {
            To = new List<string>();
            CC = new List<string>();
            BCC = new List<string>();
            People = new List<Person>();
        }

        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }
        public string TagId { get; set; }
        public string Subject { get; set; }

        [Required]
        [AllowHtml]
        public string Message { get; set; }
        public List<Person> People { get; set; }
    }

    public class TextModel
    {
        public TextModel()
        {
            To = new List<string>();
            People = new List<Person>();
        }

        [RequiredList(ErrorMessage = "Please select at least one recipient.")]
        public List<string> To { get; set; }

        public string TagId { get; set; }

        [Required]
        [MaxLength(160)]
        public string Message { get; set; }

        public List<Person> People { get; set; }
    }

    public class CommunicationHistoryModel
    {
        public CommunicationHistoryModel()
        {
            CommunicationHistory = new List<CommunicationHistory>();
        }

        public string TagId { get; set; }
        public List<CommunicationHistory> CommunicationHistory { get; set; }
    }
}