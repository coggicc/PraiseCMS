using System;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SermonNotesListViewModel
    {
        public string NoteID { get; set; }
        public string SermonTitle { get; set; }
        public string SermonID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}