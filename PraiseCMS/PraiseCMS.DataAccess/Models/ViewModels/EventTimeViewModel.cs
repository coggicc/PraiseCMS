using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class EventTimeViewModel
    {
        public EventTimeViewModel()
        {
            CampusList = new List<Campus>();
            ChurchEventScheduler = new ChurchEventScheduler();
        }

        public ChurchEventScheduler ChurchEventScheduler { get; set; }

        [Required(ErrorMessage = "Please select at least one campus.")]
        public List<string> Campuses { get; set; }
        public List<Campus> CampusList { get; set; }
    }
}