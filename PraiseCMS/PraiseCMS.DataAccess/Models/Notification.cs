using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Notifications")]
    public class Notification : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [StringLength(50)]
        [DisplayName("Notification")]
        public string Name { get; set; }

        [DisplayName("Assigned To")]
        public string AssignedToUserId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        public bool? Viewed { get; set; }

        [Display(Name = "Viewed Date")]
        public DateTime? ViewedDate { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}