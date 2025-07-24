using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class EmailsController : BaseController
    {
        [RequireUser]
        public ActionResult Index(string id = null)
        {
            var vm = new EmailVM();

            if (id.IsNotNullOrEmpty())
            {
                vm.EmailId = id;
                vm.Emails.Add(work.Email.Get(id));
            }
            else
            {
                if (SessionVariables.CurrentUser.IsSuperAdmin)
                {
                    vm.Emails = work.Email.GetAll();
                    ViewBag.Title = "Emails";
                }
                else if (SessionVariables.CurrentUser.IsAdmin)
                {
                    vm.Emails = work.Email.GetAllByChurchId(SessionVariables.CurrentChurch.Id);
                    ViewBag.Title = "Church Emails";
                }
                else
                {
                    vm.Emails = work.Email.GetAllByUserId(SessionVariables.CurrentUser.User.Id);
                    ViewBag.Title = "My Emails";
                }
            }

            if (vm.Emails.Count > 0)
            {
                vm.Users = work.User.GetAllByEmails(vm.Emails);
            }

            return View(vm);
        }

        public ActionResult UpdateStatus(string id)
        {
            if (id.IsNotNullOrEmpty())
            {
                work.Email.UpdateStatus(id);
            }

            string imagePath = HttpContext.Server.MapPath("~/Content/assets/image/favicon-32x32.png");
            byte[] cover = System.IO.File.ReadAllBytes(imagePath);

            return cover != null ? File(cover, "image/jpg") : null;
        }
    }
}