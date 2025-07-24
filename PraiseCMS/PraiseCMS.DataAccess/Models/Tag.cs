using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Tags")]
    public class Tag : BaseModel
    {
        [Required]
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Church Id")]
        public string ChurchId { get; set; }

        [Required]
        [DisplayName("Folder Id")]
        public string FolderId { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class TagDetailsViewModel
    {
        public TagDetailsViewModel()
        {
            People = new List<Person>();
        }

        public Tag Tag { get; set; }
        public List<Person> People { get; set; }
        public CommunicationHistoryModel CommunicationHistory { get; set; }
    }
}