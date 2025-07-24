using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Subscriptions")]
    public class Subscription : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Plan Type")]
        public string PlanTypeId { get; set; }

        [DisplayName("Free Trial")]
        public bool FreeTrial { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [DisplayName("Billing Plan")]
        public string BillingPlan { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}