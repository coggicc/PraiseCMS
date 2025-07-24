using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class ScheduleDemoController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(Lead model)
        {
            var response = new ResponseModel();

            model.Id = Utilities.GenerateUniqueId();
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = "SalesWebsite";
            model.IsActive = true;
            model.Status = (int)LeadStatuses.New;

            var dbEmail = CreateEmailFromLead(model);
            var notification = CreateNotificationFromLead(model);

            try
            {
                await SendEmailAsync(dbEmail, model);

                dbEmail.Sent = true;
                dbEmail.Status = EmailStatus.Sent;
                response.Success = dbEmail.Sent;
                response.Message = "Thank you for your interest in Praise CMS. We will be in touch very soon.";
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);

                dbEmail.Sent = false;
                dbEmail.Status = EmailStatus.Error;
                response.Success = dbEmail.Sent;
                response.Message = $"{Constants.DefaultErrorMessage} {ex.Message}";

                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", response.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Index", null, "Error", logObj);
            }
            finally
            {
                work.Leads.Create(model);
                work.Notification.Create(notification);
                work.Email.Create(dbEmail);
            }
            return Json(response);
        }

        private Email CreateEmailFromLead(Lead lead)
        {
            return new Email
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = "SalesWebsite",
                CreatedDate = DateTime.Now,
                Type = "Lead",
                TypeId = lead.Id,
                Message = $"<b>Name:</b> {lead.FullName} <br/><b>Church Name:</b> {lead.Church} <br/><b>Phone:</b> {lead.PhoneNumber} <br/><b>Message:</b> {lead.Message}",
                Subject = $"Demo Request - {lead.Display}",
                Text = lead.Message,
                To = "SupportEmail".AppSetting("info@praisecms.com")
            };
        }

        private Notification CreateNotificationFromLead(Lead lead)
        {
            var supportEmail = "SupportEmail".AppSetting("info@praisecms.com");
            var user = db.Users.FirstOrDefault(q => q.Email.Equals(supportEmail));
            var assignedToUserId = user?.Id;

            return new Notification
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = "SalesWebsite",
                CreatedDate = DateTime.Now,
                Type = "Lead",
                TypeId = lead.Id,
                Name = lead.FullName,
                ChurchId = "SalesWebsite",
                AssignedToUserId = assignedToUserId
            };
        }

        private async Task SendEmailAsync(Email email, Lead model)
        {
            using (var smtpClient = new SmtpClient("EmailSmtpServer".AppSetting("smtp.gmail.com")))
            {
                smtpClient.Port = Convert.ToInt32("EmailPort".AppSetting("587"));
                smtpClient.Credentials = new NetworkCredential("EmailUsername".AppSetting("info@praisecms.com"), "EmailPassword".AppSetting("zllf xizw cmvt pbxk"));
                smtpClient.EnableSsl = "EmailEnableSsl".AppSetting(true);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Subject = email.Subject;
                    mailMessage.Body = email.Message;
                    mailMessage.From = new MailAddress(model.Email, model.FullName);
                    mailMessage.ReplyToList.Add(new MailAddress(model.Email, model.FullName));
                    mailMessage.To.Add(email.To);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }
    }
}