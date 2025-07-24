using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("MessageRequests")]
    public class MessageRequest : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Message Request Category")]
        public string MessageRequestCategoryId { get; set; }

        [DisplayName("PrayedOver")]
        public bool Archived { get; set; }

        [DisplayName("PrayedOver Date")]
        public DateTime? ArchivedDate { get; set; }

        public string Display => !string.IsNullOrEmpty(Message) ? Message : Constants.DisplayDefaultText;

        public MessageRequestCategory SelectedCategory { get; set; }
    }
}