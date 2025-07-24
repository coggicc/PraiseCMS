using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ModuleAutoCompleteModel
    {
        public ModuleAutoCompleteModel()
        {
            Modules = new List<ModuleAutoCompleteModel>();
        }

        public string Value { get; set; }
        public string Text { get; set; }
        public string ParentId { get; set; }
        public List<ModuleAutoCompleteModel> Modules { get; set; }
    }
}