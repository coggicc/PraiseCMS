namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class SingleAttachmentViewModel
    {
        public SingleAttachmentViewModel()
        {
            Attachment = new AttachmentSD();
        }

        public AttachmentSD Attachment { get; set; }
    }
}