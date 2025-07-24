using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Modules")]
    public class Modules
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(128)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [StringLength(128)]
        [DisplayName("ParentId")]
        public string ParentId { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class ModuleRole
    {
        public Modules Modules { get; set; }
        public string RoleId { get; set; }
    }

    public class ModuleVM
    {
        public ModuleVM()
        {
            Modules = new List<Modules>();
        }

        public Modules CurrentModule { get; set; }
        public List<Modules> Modules { get; set; }
    }
}