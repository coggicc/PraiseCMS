using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("CommunicationGroupsPeoples")]
    public class CommunicationGroupsPeople : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Communication Group")]
        public string CommunicationGroupId { get; set; }

        [DisplayName("PersonId")]
        public string PersonId { get; set; }

        [DisplayName("Active User")]
        public bool IsActive { get; set; }

        [DisplayName("Disable Email Notifications")]
        public bool? DisableEmailNotifications { get; set; }

        [DisplayName("Disable Text Notifications")]
        public bool? DisableTextNotifications { get; set; }

        [DisplayName("Disable System Notifications")]
        public bool? DisableSystemNotifications { get; set; }

        [NotMapped]
        public string UserId { get; set; }

        [NotMapped]
        public Person Person { get; set; }

        [NotMapped]
        public string MemberName { get; set; }
        [NotMapped]
        public string PhoneNumber { get; set; }
        [NotMapped]
        public string Email { get; set; }
    }

    public class CommunicateWithGroupModel
    {
        public List<CommunicationGroup> CommunicationGroup { get; set; }
        public string SelectedGroupId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class CommunicationGroupsPeopleModel
    {
        public CommunicationGroupsPeopleModel()
        {
            Members = new List<string>();
            Peoples = new List<Person>();
            Person = new Person();
        }

        public string GroupId { get; set; }
        public IEnumerable<string> Members { get; set; }
        public IEnumerable<Person> Peoples { get; set; }
        public Person Person { get; set; }
    }

    public class CommunicationGroupsDashboard
    {
        public CommunicationGroupsDashboard()
        {
            CommunicationGroups = new List<CommunicationGroup>();
            CommunicationGroupsPeoples = new List<CommunicationGroupsPeople>();
        }

        public List<CommunicationGroup> CommunicationGroups { get; set; }
        public List<CommunicationGroupsPeople> CommunicationGroupsPeoples { get; set; }
    }
}