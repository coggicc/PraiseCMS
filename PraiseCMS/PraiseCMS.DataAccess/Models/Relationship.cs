using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Relationships")]
    public class Relationship : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Person")]
        public string PersonId { get; set; }

        [DisplayName("Relative Person")]
        public string RelativePersonId { get; set; }

        [NotMapped]
        public Person RelativePerson { get; set; }

        [DisplayName("Relation")]
        public string Relation { get; set; }
    }
}