using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class BaptismInputViewModel
    {
        public string ChurchId { get; set; }
        public List<Campus> Campuses { get; set; }
        public List<string> SelectedCampusIds { get; set; }
        public int WeeksOfData { get; set; }
        public int MinTotal { get; set; }
        public int MaxTotal { get; set; }

        public BaptismInputViewModel()
        {
            Campuses = new List<Campus>();
            SelectedCampusIds = new List<string>();
        }
    }
}
