using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Folders")]
    public class Folder : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Parent Id")]
        public string ParentId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }
        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        public string Text => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        [NotMapped]
        public List<FolderJsonModel> Children { get; set; }
    }

    public class FolderJsonModel
    {
        public FolderJsonModel()
        {
            Children = new List<FolderJsonModel>();
        }

        public string Id { get; set; }
        public string Text { get; set; }
        public string ParentId { get; set; }
        public string Icon { get; set; }
        public NodeState State { get; set; }
        public List<FolderJsonModel> Children { get; set; }
    }

    public class NodeState
    {
        public bool Disabled { get; set; }
        public bool Opened { get; set; }
        public bool Selected { get; set; }
    }

    public class AssignToTagModel
    {
        public List<Person> People { get; set; }
        public FolderJsonModel FolderJson { get; set; }
        public Tag Tag { get; set; }
    }

    public class FoldersTagsViewModel
    {
        public Folder ParentFolder { get; set; }
        public IEnumerable<Folder> Folders { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
    }
}