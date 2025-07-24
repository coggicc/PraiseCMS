using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class AttachmentsViewModel
    {
        public AttachmentsViewModel()
        {
            Attachments = new List<AttachmentSD>();
            Users = new List<ApplicationUser>();
        }

        public AttachmentsViewModel(string Type, string TypeId, string returnUrl, List<AttachmentSD> attachments = null, List<ApplicationUser> users = null)
        {
            this.Type = Type;
            this.TypeId = TypeId;
            this.returnUrl = returnUrl;
            Attachments = attachments;
            Users = users;
        }

        public string TypeId { get; set; }
        public string Type { get; set; }
        public string returnUrl { get; set; }
        public bool UseTypeVerbiage { get; set; }
        public bool DisableDragAndDrop { get; set; }
        public List<AttachmentSD> Attachments { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class AttachmentView
    {
        public AttachmentView()
        {
            Attachment = new AttachmentSD();
            User = new ApplicationUser();
        }

        public AttachmentSD Attachment { get; set; }
        public ApplicationUser User { get; set; }
    }
}