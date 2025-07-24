using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ChurchEventsViewModel
    {
        public ChurchEvent ChurchEvent { get; set; }
        public ChurchEventScheduler ChurchEventScheduler { get; set; }
        public ChurchEventTime ChurchEventTime { get; set; }
        public ChurchEventType ChurchEventType { get; set; }
        public string CampusName { get; set; }
    }
}