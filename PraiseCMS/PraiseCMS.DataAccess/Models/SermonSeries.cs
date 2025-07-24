using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SermonSeries")]
    public class SermonSeries : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Topic")]
        public string TopicId { get; set; }

        [Required]
        [DisplayName("Is Current")]
        public bool IsCurrent { get; set; }

        [Required]
        [DisplayName("Messages in Series")]
        public int MessagesInSeries { get; set; }

        [DisplayName("Messages in Series Remaining")]
        public int MessagesInSeriesRemaining { get; set; }

        [DisplayName("Image")]
        public string Image { get; set; }

        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";
    }

    public class SeriesView
    {
        public SeriesView()
        {
            Sermons = new List<Sermon>();
            TopicsList = new List<SermonTopic>();
        }

        public SermonSeries Series { get; set; }
        public SermonTopic Topic { get; set; }
        public List<Sermon> Sermons { get; set; }
        public List<SermonTopic> TopicsList { get; set; }
    }
}