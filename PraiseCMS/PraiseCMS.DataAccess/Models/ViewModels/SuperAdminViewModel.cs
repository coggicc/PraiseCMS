using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SuperAdminViewModel
    {
        public SuperAdminViewModel()
        {
            Churches = new List<Church>();
        }

        [Required(ErrorMessage = "Select a church")]
        public string SelectedChurchId { get; set; }
        public List<Church> Churches { get; set; }
    }
}