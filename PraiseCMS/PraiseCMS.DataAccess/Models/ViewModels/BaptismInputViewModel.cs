using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SalvationInputViewModel
    {
        public string ChurchId { get; set; }
        public List<Campus> Campuses { get; set; }
        public List<string> SelectedCampusIds { get; set; }
        public int WeeksOfData { get; set; }
        public int MinTotal { get; set; }
        public int MaxTotal { get; set; }

        public SalvationInputViewModel()
        {
            Campuses = new List<Campus>();
            SelectedCampusIds = new List<string>();
        }
    }
}
