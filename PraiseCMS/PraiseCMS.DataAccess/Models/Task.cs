using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Tasks")]
    public class TaskSD : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        public string ChurchId { get; set; }
        public string AssignedToUserId { get; set; }

        [Required]
        [DisplayName("Task")]
        public string Name { get; set; }

        public DateTime? DueDate { get; set; }

        public string Description { get; set; }

        public bool Completed { get; set; }

        public string CompletedBy { get; set; }

        public DateTime? DateCompleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        public bool NotifyViaEmail { get; set; }

        [NotMapped]
        public string ReturnUrl { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : "[No Task Defined]";
    }
}