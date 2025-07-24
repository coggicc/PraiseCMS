using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class SupportController : BaseController
    {
        public ActionResult Index()
        {
            var model = work.Support.GetAll(SessionVariables.CurrentUser.User.Id);
            return View(model);
        }

        [HttpGet]
        public ActionResult _CreateSupportRequest()
        {
            var model = new SupportEmail()
            {
                FromEmail = SessionVariables.CurrentUser.User.Email,
                Name = SessionVariables.CurrentUser.User.Display,
                Phone = SessionVariables.CurrentUser.User.PhoneNumber,
                Priority = Priorities.Low
            };

            return PartialView("_CreateSupportRequest", model);
        }

        [HttpPost]
        public ActionResult _CreateSupportRequest(SupportEmail model)
        {
            if (ModelState.IsValid)
            {
                var messageBody = $"Name: {model.Name}\n" +
                  $"Email: {model.FromEmail}\n" +
                  $"Phone: {model.Phone}\n" +
                  $"Message: {model.Message}";

                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = messageBody,
                    IsSupportEmail = true,
                    To = ConfigurationManager.AppSettings["SupportEmail"],
                    Attachments = null,
                    Subject = model.Priority + " - New Support Request",
                    CreatedBy = SessionVariables.CurrentUser?.User?.Id ?? string.Empty,
                    CreatedDate = DateTime.Now
                };

                var result = Emailer.SendEmail(email);

                if (result)
                {
                    CreateAlertMessage("Your support request has been sent. We will be in touch soon.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                }

                return AjaxRedirectTo("/Support/Index");
            }

            CreateAlertMessage("Please add your message before sending.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
            return PartialView("_CreateSupportRequest", model);
        }
    }
}