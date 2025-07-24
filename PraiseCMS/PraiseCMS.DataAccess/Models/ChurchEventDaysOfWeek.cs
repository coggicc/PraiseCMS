using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.Shared.Methods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ChurchEventDaysOfWeek")]
    public class ChurchEventDaysOfWeek : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        public string ChurchEventId { get; set; }

        [Required]
        public string DayOfWeek { get; set; }

        // Navigation property
        public ChurchEvent ChurchEvent { get; set; }
    }
}