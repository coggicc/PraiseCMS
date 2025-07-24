using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class DetailTasksViewModel
    {
        public DetailTasksViewModel()
        {
            Tasks = new TaskSD();
            Users = new List<ApplicationUser>();
            UserSetting = new UserSetting();
        }

        public TaskSD Tasks { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public UserSetting UserSetting { get; set; }
    }
}