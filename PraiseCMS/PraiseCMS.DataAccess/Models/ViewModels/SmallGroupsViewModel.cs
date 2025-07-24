using PraiseCMS.Shared.Shared;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SmallGroupsViewModel
    {
        public SmallGroup SmallGroup { get; set; }
        public SelectList StateList { get; set; }

        public SmallGroupsViewModel()
        {
            StateList = new SelectList(Constants.GetAbbrevToStateAsDropdown(), "Value", "Text");
        }
    }
}