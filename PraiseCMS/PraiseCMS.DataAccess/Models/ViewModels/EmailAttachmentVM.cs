namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class EmailAttachmentVM
    {
        public EmailAttachmentVM(byte[] attachment, string fileName)
        {
            Attachment = attachment;
            FileName = fileName;
        }

        public byte[] Attachment { get; set; }
        public string FileName { get; set; }
    }
}