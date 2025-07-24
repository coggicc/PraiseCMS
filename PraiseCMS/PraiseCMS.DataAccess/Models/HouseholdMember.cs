using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("HouseholdMembers")]
    public class HouseholdMember : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Household")]
        public string HouseholdId { get; set; }

        [DisplayName("Person")]
        public string PersonId { get; set; }

        [DisplayName("FamilyRole")]
        public string FamilyRole { get; set; }

        [DisplayName("Head")]
        public bool IsHeadofHousehold { get; set; }

        [DisplayName("Active User")]
        public bool IsActive { get; set; }

        [NotMapped]
        public string UserId { get; set; }
    }

    [NotMapped]
    public class HouseholdMemberVM : HouseholdMember
    {
        public string MemberName { get; set; }
    }

    public class HouseholdMemberViewModel
    {
        public HouseholdMemberViewModel()
        {
            Member = new HouseholdMember();
            Peoples = new List<Person>();
            People = new Person();
        }

        public HouseholdMember Member { get; set; }
        public IEnumerable<Person> Peoples { get; set; }
        public Person People { get; set; }
        public string Mode { get; set; }
    }

    public class HouseholdDashboard
    {
        public HouseholdDashboard()
        {
            Households = new List<Household>();
            HouseholdMembers = new List<HouseholdMemberVM>();
            People = new List<Person>();
        }

        public List<Person> People { get; set; }
        public List<Household> Households { get; set; }
        public List<HouseholdMemberVM> HouseholdMembers { get; set; }
    }

    public class PersonHousehold
    {
        public Household Household { get; set; }
        public HouseholdMember HouseholdMember { get; set; }
    }
}