using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("CommunicationGroups")]
    public class CommunicationGroup : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [Required]
        [DisplayName("Enable Email")]
        public bool EnableEmail { get; set; }

        [Required]
        [DisplayName("Enable Text")]
        public bool EnableText { get; set; }

        [Required]
        [DisplayName("Enable System Notification")]
        public bool EnableSystemNotification { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        [DisplayName("Allow User to Unsubscribe?")]
        public bool AllowUserToUnsubscribe { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        [NotMapped]
        public List<CommunicationGroupsPeople> CommunicationGroupsPeople { get; set; }

        [NotMapped]
        public string GroupsPersonId { get; set; }

        [NotMapped]
        public bool? DisableEmailNotifications { get; set; }

        [NotMapped]
        public bool? DisableTextNotifications { get; set; }

        [NotMapped]
        public bool? DisableSystemNotifications { get; set; }
    }
}