using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using Rotativa;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class PdfController : BaseController
    {
        //[AllowAnonymous]
        //public ActionResult ShortInventorySheet(string search, string marketId, string areaId, string subdivisionId, string start, string end, string type, string status, string phaseId, string planId, string progressStatus, string layout, bool? download, string fileName)
        //{
        //    using (var provider = new UnitProvider())
        //    {
        //        var model = provider.GetUnitDashboard(SessionVariables.CurrentCompany.Id, search, marketId, areaId, subdivisionId, start, end, type, status, phaseId, planId, progressStatus);

        //        if (download != null && download.Value)
        //        {
        //            return new ViewAsPdf("ShortInventorySheet", "_PdfLayout", model) { FileName = fileName, PageOrientation = Rotativa.Options.Orientation.Landscape, PageSize = Rotativa.Options.Size.Legal, PageMargins = new Rotativa.Options.Margins(3, 3, 3, 3) };
        //        }
        //        else
        //        {
        //            return View("ShortInventorySheet", !string.IsNullOrEmpty(layout) ? layout : "_PdfLayout", model);
        //        }
        //    }
        //}

        //[AllowAnonymous]
        //public ActionResult InventorySheet(string search, string marketId, string areaId, string subdivisionId, string start, string end, string type, string status, string phaseId, string planId, string progressStatus, string layout, bool? download, string fileName)
        //{
        //    using (var provider = new UnitProvider())
        //    {
        //        var model = provider.GetUnitDashboard(SessionVariables.CurrentCompany.Id, search, marketId, areaId, subdivisionId, start, end, type, status, phaseId, planId, progressStatus);

        //        if (download != null && download.Value)
        //        {
        //            return new ViewAsPdf("InventorySheet", "_PdfLayout", model) { FileName = fileName, PageOrientation = Rotativa.Options.Orientation.Landscape, PageSize = Rotativa.Options.Size.Legal, PageMargins = new Rotativa.Options.Margins(3, 3, 3, 3) };
        //        }
        //        else
        //        {
        //            return View("InventorySheet", !string.IsNullOrEmpty(layout) ? layout : "_PdfLayout", model);
        //        }
        //    }
        //}

        //[AllowAnonymous]
        //public ActionResult ConstructionEstimateSheet(string id)
        //{
        //    using (var provider = new UnitProvider())
        //    {
        //        var model = provider.GetUnitView(id);

        //        return new ViewAsPdf("ConstructionEstimateSheet", "_PdfLayout", model) { FileName = model.Unit.UnitCode.HtmlFriendly() + "-construction-estimate.pdf", PageOrientation = Rotativa.Options.Orientation.Landscape, PageSize = Rotativa.Options.Size.Legal, PageMargins = new Rotativa.Options.Margins(3, 3, 3, 3) };
        //    }
        //}

        //[HttpPost]
        //[RequireUser]
        //public ActionResult _SendText(SendPdfVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var smsMessage = new SmsMessageSD();

        //        smsMessage.Id = Utilities.GenerateUniqueId();
        //        smsMessage.DateCreated = DateTime.Now;
        //        smsMessage.Type = model.View;
        //        smsMessage.TypeId = model.ViewId;
        //        smsMessage.To = model.To;
        //        smsMessage.Message = model.Message;

        //        var success = SmsMessager.SendMessage(smsMessage);
        //        if (success)
        //        {
        //            AlertMessage = "Successfully sent text message to the recipient!";
        //            AlertMessageType = AlertMessageTypes.Success;
        //        }
        //        else
        //        {
        //            AlertMessage = "Eek, something went wrong trying to send your text message. Please try again";
        //            AlertMessageType = AlertMessageTypes.Failure;
        //        }

        //        return AjaxReloadPage;
        //    }

        //    return PartialView(model);
        //}

        [HttpGet]
        [RequireUser]
        public ActionResult _SendPdf(string view, string viewId, string to, string subject, string fileName)
        {
            var model = new SendPdfVM() { View = view, ViewId = viewId, To = to, Subject = subject };

            return PartialView(model);
        }

        //[HttpPost]
        //[RequireUser]
        //public ActionResult _SendPdf(SendPdfVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var pdf = Bytes(model.View, model.ViewId);
        //        var email = new EmailSD();

        //        email.Id = Utilities.GenerateUniqueId();
        //        email.DateCreated = DateTime.Now;
        //        email.CreatedBy = SessionVariables.CurrentUser.User.Id;
        //        email.Type = model.View;
        //        email.TypeId = model.ViewId;
        //        email.To = model.To;
        //        email.Cc = model.Cc;
        //        email.Subject = model.Subject;
        //        email.Message = model.Message;

        //        var success = Emailer.SendEmail(email, pdf, !string.IsNullOrEmpty(model.FileName) ? model.FileName : model.View.ToLower() + ".pdf", null, null, true);
        //        if (success)
        //        {
        //            AlertMessage = "Successfully send email to your recipient";
        //            AlertMessageType = AlertMessageTypes.Success;
        //        }
        //        else
        //        {
        //            AlertMessage = "There was an issue sending the email to this recipient, please try again";
        //            AlertMessageType = AlertMessageTypes.Failure;
        //        }

        //        return AjaxReloadPage;
        //    }

        //    return PartialView(model);
        //}

        public byte[] Bytes(string view, string viewId)
        {
            var pdfResult = new ActionAsPdf(view, new { id = viewId });

            return pdfResult.BuildFile(ControllerContext);
        }

        //public SaveAttachmentResultVM SaveAttachment(string view, string viewId, string fileName, string attachmentType, string attachmentTypeId)
        //{
        //    var pdf = Bytes(view, viewId);
        //    var stream = new MemoryStream(pdf);
        //    var success = AwsHelpers.UploadFile(fileName, stream);

        //    if (success)
        //    {
        //        using (var attachmentProvider = new AttachmentProvider())
        //        {
        //            var attachment = new AttachmentSD();

        //            attachment.Id = Utilities.GenerateUniqueId();
        //            attachment.DateCreated = DateTime.Now;
        //            attachment.CreatedBy = SessionVariables.CurrentUser != null ? SessionVariables.CurrentUser.User.Id : "unknown";
        //            attachment.Type = attachmentType;
        //            attachment.TypeId = attachmentTypeId;
        //            attachment.FileName = fileName;
        //            attachment.Name = fileName;

        //            attachmentProvider.Insert(attachment);
        //        }
        //    }

        //    return new SaveAttachmentResultVM { Pdf = pdf, Success = success };
        //}
    }

    public class SendPdfVM
    {
        public string View { get; set; }
        public string ViewId { get; set; }
        public string FileName { get; set; }

        public string To { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }

        [AllowHtml]
        public string Message { get; set; }
    }

    public class SendTextVM
    {
        public string View { get; set; }
        public string ViewId { get; set; }
        public string To { get; set; }

        [AllowHtml]
        public string Message { get; set; }
    }

    public class SaveAttachmentResultVM
    {
        public byte[] Pdf { get; set; }
        public bool Success { get; set; }
    }
}