using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class NotesViewModel
    {
        public NotesViewModel(string Type, string TypeId, List<Note> notes = null, List<ApplicationUser> users = null)
        {
            this.Type = Type;
            this.TypeId = TypeId;
            Notes = notes;
            Users = users;
        }
        public string Type { get; set; }
        public string TypeId { get; set; }
        public List<Note> Notes { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}