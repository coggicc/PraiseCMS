using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SermonSeriesViewModel
    {
        public SermonSeriesViewModel()
        {
            TopicsList = new List<SermonTopic>();
        }

        public SermonSeries Series { get; set; }
        public List<SermonTopic> TopicsList { get; set; }
    }
}