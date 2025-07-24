using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "71154639657f9bcf4b52b14992b89a")]
    public class MessageRequestsController : BaseController
    {
        public ActionResult Index()
        {
            var messageRequestDashboardViewModel = work.MessageRequest.GetMessageRequestDashboard(SessionVariables.CurrentChurch.Id);
            return View(messageRequestDashboardViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        [OverrideActionFilters]
        public ActionResult CreateMessageRequestExternal(string id)
        {
            if (!id.IsNotNullOrEmpty()) return Json("Invalid Church Id", JsonRequestBehavior.AllowGet);

            var model = work.MessageRequest.GetCreateMessageRequestModel(id);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [OverrideActionFilters]
        public ActionResult CreateMessageRequestExternal(MessageRequestViewModel model)
        {
            var church = work.Church.Get(model.MessageRequest.ChurchId);

            var result = new ResponseModel();

            try
            {
                work.MessageRequest.CreateMessageRequest(model.MessageRequest);

                var notification = new Notification
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedBy = Constants.System,
                    CreatedDate = DateTime.Now,
                    Type = "External Message Request",
                    TypeId = model.MessageRequest.Id,
                    ChurchId = church.Id,
                    Name = "New Message Request",
                    Viewed = false
                };

                work.Notification.Create(notification);

                const string subject = "New Message Request";

                if (church.Email.IsNotNullOrEmpty())
                {
                    var email = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = EmailTemplates.General.Replace("{message}", model.MessageRequest.Message),
                        To = church.PrayerRequestEmail,
                        Attachments = null,
                        Subject = subject,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Constants.System
                    };

                    Emailer.SendEmail(email, null, null, new Domain()
                    {
                        EmailLogo = church.Logo,
                        Name = church.Display,
                        EmailReplyAddress = church.Email,
                        EmailDisplay = church.Display
                    }, church);
                }

                result.Success = true;

                return View("ThankYouPage", result);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"There was a problem submitting your message request. Please try again later. {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var returnModel = work.MessageRequest.GetCreateMessageRequestModel(church.Id);

                return View(returnModel);
            }
        }
    }
}