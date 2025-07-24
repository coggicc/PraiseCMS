using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Sermons")]
    public class Sermon : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Preached By")]
        public string PreachedById { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Topic")]
        public string TopicId { get; set; }

        [DisplayName("Services Preached")]
        public string Services { get; set; }

        [DisplayName("Series")]
        public string SeriesId { get; set; }

        [DisplayName("Is Current")]
        public bool IsCurrent { get; set; }

        [DisplayName("Image")]
        public string Image { get; set; }

        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";
    }

    public class SermonView
    {
        public Sermon Sermon { get; set; }
        public SermonSeries Series { get; set; }
        public SermonTopic Topic { get; set; }
        public SermonNote Notes { get; set; }
        public List<ApplicationUser> Pastors { get; set; }
    }

    public class SermonListView
    {
        public SermonListView()
        {
            Sermons = new List<Sermon>();
        }

        public List<Sermon> Sermons { get; set; }
        public SermonSeries Series { get; set; }
        public SermonTopic Topic { get; set; }
        public SermonNote Notes { get; set; }
    }
}