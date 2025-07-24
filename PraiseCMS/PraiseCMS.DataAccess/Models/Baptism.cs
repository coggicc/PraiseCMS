using PraiseCMS.DataAccess.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Baptisms")]
    public class Baptism : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [DisplayName("Total")]
        public int Total { get; set; }

        [DisplayName("Occurred On Date")]
        public DateTime? OccurredOnDate { get; set; }
    }

    public class BaptismSummaryModel
    {
        public BaptismSummaryModel()
        {
            AllBaptisms = new List<Baptism>();
            BaptismsByDate = new List<Baptism>();
        }

        public List<Baptism> AllBaptisms { get; set; }
        public List<Baptism> BaptismsByDate { get; set; }
        public string DateRange { get; set; }
    }
}