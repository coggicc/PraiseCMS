using PraiseCMS.DataAccess.Models.ViewModels.Base;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Funds = new List<Fund>();
            Campuses = new List<Campus>();
            Denominations = new List<Denomination>();
            Administrators = new List<ApplicationUser>();
        }

        public ChurchMerchantAccount ChurchMerchantAccount { get; set; }
        public List<Campus> Campuses { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Denomination> Denominations { get; set; }
        public List<ApplicationUser> Administrators { get; set; }
        public string GiveNowButton { get; set; }
        public string GiveNotButtonStyling { get; set; }
        public string ExternalPrayerRequestCode { get; set; }
        public string ExternalMessageRequestCode { get; set; }
    }
}