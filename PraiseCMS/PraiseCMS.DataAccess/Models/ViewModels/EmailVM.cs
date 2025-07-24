using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class EmailVM
    {
        public EmailVM()
        {
            Emails = new List<Email>();
            Users = new List<ApplicationUser>();
        }

        public List<Email> Emails { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public string EmailId { get; set; }
    }
}