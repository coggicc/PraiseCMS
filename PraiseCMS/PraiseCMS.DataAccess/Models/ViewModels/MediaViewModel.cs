using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class MediaViewModel
    {
        public MediaViewModel()
        {
            Series = new List<SermonSeries>();
            Sermons = new List<Sermon>();
        }

        public Sermon CurrentSermon { get; set; }
        public List<SermonSeries> Series { get; set; }
        public List<Sermon> Sermons { get; set; }
    }
}