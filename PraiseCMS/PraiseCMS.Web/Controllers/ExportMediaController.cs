using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using SelectPdf;
using System;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class ExportMediaController : Controller
    {
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExportPDF(string Message, string NoteType)
        {
            const string pdf_page_size = "A4";
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), pdf_page_size, true);
            const string pdf_orientation = "Landscape";
            PdfPageOrientation pdfOrientation =
                (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation),
                pdf_orientation, true);

            const int webPageWidth = 1024;
            const int webPageHeight = 0;
            string path = string.Empty;

            if (NoteType == "StandardFilled")
            {
                path = Server.MapPath("~/Content/PDFTemplate/SermonNotesPDF.html");
            }

            if (NoteType == "Blank")
            {
                path = Server.MapPath("~/Content/PDFTemplate/BlankPDF.html");
            }

            string BootstrapFile = Server.MapPath("~/Content/assets/css/bootstrap-4.5.0.min.css");
            string JqueryFile = Server.MapPath("~/Content/assets/plugins/general/jquery/dist/jquery.min.js");
            string Logopath = Server.MapPath("~/Content/assets/image/highlands_logo.png");
            string htmlString = System.IO.File.ReadAllText(path);
            htmlString = htmlString.Replace("{msg}", Message);
            htmlString = htmlString.Replace("{img}", Logopath);
            htmlString = htmlString.Replace("{bootstrap}", BootstrapFile);
            htmlString = htmlString.Replace("{Jquery}", JqueryFile);
            // string htmlString = Message.ToString();

            // get base url
            string baseUrl = ControllerContext.HttpContext.Request.Url.
                AbsoluteUri.Substring(
                0, ControllerContext.HttpContext.Request.Url.
                AbsoluteUri.Length - "ExportMedia/PDF".Length);

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;
            converter.Options.MarginRight = 40;
            converter.Options.MarginLeft = 40;
            converter.Options.CssMediaType = (HtmlToPdfCssMediaType)Enum.Parse(typeof(HtmlToPdfCssMediaType), "Print", true);
            converter.Options.PageBreaksEnhancedAlgorithm = true;
            converter.Options.MaxPageLoadTime = 120;

            // create a new pdf document converting a html string
            PdfDocument doc = converter.ConvertHtmlString(htmlString, baseUrl);

            // save pdf document
            byte[] pdf = doc.Save();

            // close pdf document
            doc.Close();

            // return resulted pdf document
            return new FileContentResult(pdf, "application/pdf")
            {
                FileDownloadName = "Document.pdf"
            };
        }

        [HttpPost]
        public ActionResult ExportBlankNotePDF()
        {
            return new Rotativa.PartialViewAsPdf("_BlankNotesPartial", SessionVariables.CurrentChurch) { FileName = "Empty Message Notes.pdf", PageOrientation = Rotativa.Options.Orientation.Landscape };
        }

        [HttpPost]
        public ActionResult ExportStandardNotePDF()
        {
            return new Rotativa.PartialViewAsPdf("_StandardNotesPartial", new SermonNote()) { FileName = "Standard Message Notes.pdf", PageOrientation = Rotativa.Options.Orientation.Portrait };
        }

        [HttpPost]
        public ActionResult ExportStandardFilledNotePDF()
        {
            return new Rotativa.PartialViewAsPdf("_StandardFilledNotesPartial", new SermonNote()) { FileName = "Standard Filled Notes.pdf", PageOrientation = Rotativa.Options.Orientation.Portrait };
        }

        [HttpPost]
        public ActionResult Print(SermonNote note)
        {
            return View(note);
        }
    }
}