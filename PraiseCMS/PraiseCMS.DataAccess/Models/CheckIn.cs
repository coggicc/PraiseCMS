using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("CheckIns")]
    public class CheckIn : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [DisplayName("Church Event Time Id")]
        public string ChurchEventTimeId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }

        [DisplayName("Person")]
        public string PersonId { get; set; }

        [NotMapped]
        public Person Person { get; set; }

        [DisplayName("Check In Time")]
        public DateTime CheckInTime { get; set; }
    }
}