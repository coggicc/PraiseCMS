using PraiseCMS.DataAccess.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Attendance")]
    public class Attendance : BaseModel
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

    public class AttendanceSummaryModel
    {
        public AttendanceSummaryModel()
        {
            AllAttendance = new List<Attendance>();
            AttendanceByDate = new List<Attendance>();
        }

        public List<Attendance> AllAttendance { get; set; }
        public List<Attendance> AttendanceByDate { get; set; }
        public string DateRange { get; set; }
    }
}