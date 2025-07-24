using PraiseCMS.DataAccess.Interfaces;
using PraiseCMS.DataAccess.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("OfflineGiving")]
    public class OfflineGiving : BaseModel, IGivingItem
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Offline Payment Method")]
        public string OfflinePaymentMethod { get; set; }

        [Required]
        [DisplayName("Amount")]
        public decimal Amount { get; set; }

        [Required]
        [DisplayName("Fund")]
        public string FundId { get; set; }

        [DisplayName("Person")]
        public string PersonId { get; set; }

        [DisplayName("Check Number")]
        public string CheckNumber { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [Required]
        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [DisplayName("Event Time Id")]
        public string EventTimeId { get; set; }

        [DisplayName("Offline Payment Type")]
        public string OfflinePaymentType { get; set; }

        [DisplayName("Date Received")]
        public DateTime? DateReceived { get; set; }

        public string PaymentType => OfflinePaymentType;
    }

    public class OfflineGivingView
    {
        public OfflineGivingView()
        {
            Funds = new List<Fund>();
            People = new List<Person>();
            Person = new Person();
            Payments = new List<SplitPaymentViewModel>();
        }

        public OfflineGiving OfflineGiving { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Person> People { get; set; }
        public Person Person { get; set; }
        public string Mode { get; set; }
        [Required(ErrorMessage = "The amount must be $1.00 or greater.")]
        public string Amount { get; set; }
        public string OfflineGivingType { get; set; }
        public List<SplitPaymentViewModel> Payments { get; set; }
    }

    public class MassOfflineGivingViewModel
    {
        public MassOfflineGivingViewModel()
        {
            Person = new Person();
            People = new List<Person>();
            Funds = new List<Fund>();
        }

        public List<Person> People { get; set; }
        public Person Person { get; set; }
        public OfflineGiving OfflineGiving { get; set; }
        public List<Fund> Funds { get; set; }
        public List<PaymentViewModel> Payments { get; set; }
        public string OfflineGivingType { get; set; }
    }

    public class PaymentViewModel
    {
        public string Amount { get; set; }
        public string CheckNumber { get; set; }
        public string Person { get; set; }
    }

    public class SplitPaymentViewModel
    {
        [Required]
        public string Amount { get; set; }

        [Required]
        public string FundId { get; set; }
    }

    public class OfflineGivingListView
    {
        public OfflineGivingListView()
        {
            OfflineGiving = new List<OfflineGiving>();
            Funds = new List<Fund>();
            Donors = new List<Person>();
        }

        public List<OfflineGiving> OfflineGiving { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Person> Donors { get; set; }
    }
}