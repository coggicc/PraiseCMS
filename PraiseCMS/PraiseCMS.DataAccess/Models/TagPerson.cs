using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("TagPeople")]
    public class TagPerson
    {
        [Required]
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Tag Id")]
        public string TagId { get; set; }

        [Required]
        [DisplayName("Person")]
        public string PersonId { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
    }

    public class TagPeopleViewModel
    {
        public TagPeopleViewModel()
        {
            People = new List<Person>();
            TagPeople = new List<string>();
        }

        public List<Person> People { get; set; }
        public List<string> TagPeople { get; set; }
        public string TagId { get; set; }
    }
}