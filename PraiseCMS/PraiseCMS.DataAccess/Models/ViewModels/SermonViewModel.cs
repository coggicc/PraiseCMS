using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SermonViewModel
    {
        public SermonViewModel()
        {
            NotesList = new List<SermonNote>();
            SeriesList = new List<SermonSeries>();
            SermonList = new List<Sermon>();
            TopicsList = new List<SermonTopic>();
            Pastors = new List<UsersWithRoles>();
        }

        public SermonNote MessageNotes { get; set; }
        public Sermon Sermon { get; set; }
        public SermonSeries Series { get; set; }
        public List<SermonNote> NotesList { get; set; }
        public List<SermonSeries> SeriesList { get; set; }
        public List<Sermon> SermonList { get; set; }
        public List<SermonTopic> TopicsList { get; set; }
        public List<UsersWithRoles> Pastors { get; set; }
    }
}