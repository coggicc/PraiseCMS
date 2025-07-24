using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Permissions")]
    public class Permissions
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("TypeId")]
        public string TypeId { get; set; }

        [DisplayName("ModuleId")]
        public string ModuleId { get; set; }

        [DisplayName("OperationId")]
        public int OperationId { get; set; }

        [DisplayName("ModuleValue")]
        public string ModuleValue { get; set; }

        [NotMapped]
        public string Role { get; set; }
    }
}