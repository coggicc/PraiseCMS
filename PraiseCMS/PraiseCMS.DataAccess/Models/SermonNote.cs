using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SermonNotes")]
    public class SermonNote : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Sermon")]
        public string SermonId { get; set; }

        [Required]
        [AllowHtml]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Pastor Version")]
        public bool IsPastorVersion { get; set; }

        [AllowHtml]
        [DisplayName("Pastor Only Notes")]
        public string PastorOnlyNotes { get; set; }

        public bool NotifySubscribers { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }
        public string NoteType { get; set; }
    }

    public class SermonNoteView
    {
        public SermonNoteView()
        {
            Sermons = new List<Sermon>();
        }

        public List<Sermon> Sermons { get; set; }
        public SermonNote SermonNote { get; set; }
    }
}