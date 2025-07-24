using PraiseCMS.DataAccess.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ChurchEventScheduler")]
    public class ChurchEventScheduler : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string EventId { get; set; }
        [NotMapped]
        public ChurchEvent Event { get; set; }
        public string CampusId { get; set; }

        [NotMapped]
        public string Campus { get; set; }
        [NotMapped]
        public ChurchEventTime Time { get; set; }

        [Required(ErrorMessage = "Please select a start date.")]
        public DateTime StartDate { get; set; }

        public DateTime? SystemEndDate { get; set; }

        [DisplayName("Frequency ")]
        public string Frequency { get; set; }

        [DisplayName("Event Ends")]
        [Required]
        public string EventEnds { get; set; }

        [DisplayName("Occurrences")]
        public int? Occurrences { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [DisplayName("Every Count")]
        public int? EveryCount { get; set; }

        [DisplayName("Every Type")]
        public string EveryType { get; set; }

        [DisplayName("Repeat On")]
        public string RepeatOn { get; set; }

        [DisplayName("Is Deleted")]
        public bool IsDeleted { get; set; }

        [NotMapped]
        public List<ChurchEventTime> Times { get; set; }
    }
}