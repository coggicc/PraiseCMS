using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class ContactController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.ContactFormTimestamp = DateTime.Now;
            return View();
        }

        [HttpPost]
        public ActionResult Index(Lead model)
        {
            var response = new ResponseModel();

            var startTime = Convert.ToDateTime(Request.Form["ContactFormTimestamp"]);
            double timeStampDifference = 0;

            timeStampDifference = (DateTime.Now - startTime).TotalSeconds;

            //TODO: Add IP to IPBlacklist and Log tables
            //Check if the form was submitted in less than 1 second or if the phone honeypot was filled in.
            //We are using this to help see if it was submitted by a bot or human
            if (timeStampDifference < 1 || !string.IsNullOrEmpty(model.Phone))
            {
                //Send back response as success so the bot won't know it was ignored.
                response.Success = true;
                response.Message = "Thanks! We will be in touch soon.";

                return Json(response);
            }

            model.Id = Utilities.GenerateUniqueId();
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = "SalesWebsite";

            var dbEmail = new Email()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = "SalesWebsite",
                CreatedDate = DateTime.Now,
                Type = "Contact Form",
                TypeId = model.Id,
                Message = $"<b>Name:</b> {model.FullName} <br/><b>Phone:</b> {model.PhoneNumber} <br/><b>Message:</b> {model.Message}",
                Subject = $"Contact Form Submission  - {model.FullName}",
                Text = model.Message,
                To = "SupportEmail".AppSetting("info@praisecms.com")
            };

            var lead = new Lead()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = "SalesWebsite",
                CreatedDate = DateTime.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
                Status = (int)LeadStatuses.New
            };

            var notification = new Notification()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = "SalesWebsite",
                CreatedDate = DateTime.Now,
                Type = "Contact Form",
                TypeId = model.Id,
                Name = model.FullName,
                ChurchId = "SalesWebsite"
            };

            try
            {
                var email = new MailMessage
                {
                    IsBodyHtml = true,
                    Subject = dbEmail.Subject,
                    Body = dbEmail.Message,
                    From = new MailAddress(model.Email, model.FullName)
                };
                email.ReplyToList.Add(new MailAddress(model.Email, model.FullName));
                email.To.Add(dbEmail.To);

                var server = new SmtpClient("EmailSmtpServer".AppSetting("smtp.gmail.com"))
                {
                    Port = Convert.ToInt32("EmailPort".AppSetting("587")),
                    Credentials = new System.Net.NetworkCredential("EmailUsername".AppSetting("info@praisecms.com"), "EmailPassword".AppSetting("zllf xizw cmvt pbxk")),
                    EnableSsl = "EmailEnableSsl".AppSetting(true)
                };
                server.Send(email);

                dbEmail.Sent = true;
                dbEmail.Status = EmailStatus.Sent;
                response.Success = dbEmail.Sent;
                response.Message = "Thank you for reaching out. One of our dedicated team members will be in touch with you soon. We look forward to helping you.";
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                dbEmail.Sent = false;
                dbEmail.Status = EmailStatus.Error;
                response.Success = dbEmail.Sent;
                response.Message = $"{Constants.DefaultErrorMessage} {ex.Message}";
            }
            finally
            {
                using (var db = new ApplicationDbContext())
                {
                    //Check if the support email came from someone already in the system
                    var existingUser = db.Users.FirstOrDefault(x => x.Email.Equals(lead.Email) || x.PhoneNumber.Equals(lead.PhoneNumber));
                    var existingLead = db.Leads.FirstOrDefault(x => x.Email.Equals(lead.Email) || x.PhoneNumber.Equals(lead.PhoneNumber));
                    const string typePrefix = "Contact Form - ";

                    //If user exists, then set email TypeId to userId
                    if (existingUser != null)
                    {
                        dbEmail.Type = $"{typePrefix}{ContactFormEmailTypes.ExistingUser}";
                        dbEmail.TypeId = existingUser.Id;
                    }
                    else
                    {
                        //If lead doesn't exist, then create a new one
                        if (existingLead == null)
                        {
                            dbEmail.Type = $"{typePrefix}{ContactFormEmailTypes.NewLead}";
                            dbEmail.TypeId = lead.Id;
                            db.Leads.Add(lead);
                        }
                        else
                        {
                            dbEmail.Type = $"{typePrefix}{ContactFormEmailTypes.ExistingLead}";
                            dbEmail.TypeId = existingLead.Id;
                        }
                    }

                    //Assign notification to super admin UserId (info@praisecms.com)
                    var supportEmail = "SupportEmail".AppSetting("info@praisecms.com");
                    var adminUser = db.Users.FirstOrDefault(q => q.Email.Equals(supportEmail));

                    if (adminUser != null)
                    {
                        notification.AssignedToUserId = adminUser.Id;
                    }

                    db.Notifications.Add(notification);
                    db.Emails.Add(dbEmail);
                    db.SaveChanges();
                }
            }
            return Json(response);
        }
    }
}