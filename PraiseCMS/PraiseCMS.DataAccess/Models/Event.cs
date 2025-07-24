using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Events")]
    public class EventSD : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [DisplayName("Start Time")]
        public string StartTime { get; set; }

        [DisplayName("End Time")]
        public string EndTime { get; set; }

        [DisplayName("All Day")]
        public bool AllDay { get; set; }

        [DisplayName("Complete")]
        public bool Complete { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        [DisplayName("Calendar Color")]
        public string CalendarColor { get; set; }

        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";

        public string DisplayWithDate => $"{Display} ({StartDate.ToShortDateString()})";

        [NotMapped]
        [DisplayName("Campus Id")]
        public string CampusId { get; set; }

        [NotMapped]
        [DisplayName("Campus Name")]
        public string CampusName { get; set; }

        [NotMapped]
        public string UniqueId { get; set; }

        [NotMapped]
        public string ChurchEventId { get; set; }

        [NotMapped]
        public string ChurchEventSchedulerId { get; set; }

        [NotMapped]
        public string ShowEventAt { get; set; }

        [NotMapped]
        public string HideEventAt { get; set; }
    }
}