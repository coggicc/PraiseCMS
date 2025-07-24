using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class IPViewModel
    {
        public IPViewModel()
        {
            Users = new List<ApplicationUser>();
            IPBlacklists = new List<IPBlacklist>();
            IPWhitelists = new List<IPWhitelist>();
        }

        public List<ApplicationUser> Users { get; set; }
        public List<IPBlacklist> IPBlacklists { get; set; }
        public List<IPWhitelist> IPWhitelists { get; set; }
    }
}