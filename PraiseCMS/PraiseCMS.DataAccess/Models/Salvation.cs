using PraiseCMS.DataAccess.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Salvations")]
    public class Salvation : BaseModel
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

    public class SalvationSummaryModel
    {
        public SalvationSummaryModel()
        {
            AllSalvations = new List<Salvation>();
            SalvationsByDate = new List<Salvation>();
        }

        public List<Salvation> AllSalvations { get; set; }
        public List<Salvation> SalvationsByDate { get; set; }
        public string DateRange { get; set; }
    }
}