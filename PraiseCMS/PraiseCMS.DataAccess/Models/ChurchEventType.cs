using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    //Only standard event types are stored in this table. Custom events are stored in ChurchEvents
    [Table("ChurchEventTypes")]
    public class ChurchEventType : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }


        [Required]
        public string Type { get; set; }

        public string CalendarColor { get; set; }

        public bool IsDeleted { get; set; }

        public string Display => !string.IsNullOrEmpty(Type) ? Type : Constants.DisplayDefaultText;

        // Navigation property
        //public virtual Church Church { get; set; }
    }

    public class ChurchEventTypeView
    {
        public ChurchEventType ChurchEventType { get; set; }
        public List<string> CommonEventType { get; set; }
        public string ReturnUrl { get; set; }
    }
}