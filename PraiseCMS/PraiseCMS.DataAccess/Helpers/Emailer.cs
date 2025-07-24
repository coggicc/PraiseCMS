using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace PraiseCMS.DataAccess.Helpers
{
    public static class Emailer
    {
        public static ApplicationDbContext db = new ApplicationDbContext();

        public static bool SendEmail(Email message, List<EmailAttachmentVM> attachments, Domain domain = null, Church church = null, bool sendFromCurrentUser = false, string replyEmailAddress = null, string replyName = null)
        {
            try
            {
                var domainOrDefault = domain ?? SessionVariables.CurrentDomain;
                var churchOrDefault = church ?? SessionVariables.CurrentChurch;
                var baseUrl = domainOrDefault?.BaseUrl.IsNotNullOrEmpty() == true ? $"https://{domainOrDefault.BaseUrl}" : ApplicationCache.Instance.SiteConfiguration.Url;
                var name = ApplicationCache.Instance.SiteConfiguration.Name;
                var logo = (domainOrDefault?.EmailLogo.IsNotNullOrEmpty() == true) ? AwsHelpers.AmazonLink(domainOrDefault.EmailLogo, "Uploads/Logos") : $"{baseUrl}/Content/assets/media/logos/praise_logo_white_blue.png";
                var altLogo = domainOrDefault?.Name ?? "Praise CMS";
                var description = domainOrDefault?.SiteTitle ?? ApplicationCache.Instance.SiteConfiguration.Title;

                // Prepare church-related variables
                var (facebookProfile, twitterProfile, instagramProfile, youTubeProfile, linkedInProfile) = churchOrDefault != null ?
                    (churchOrDefault.FacebookProfile, churchOrDefault.TwitterProfile, churchOrDefault.InstagramProfile, churchOrDefault.YouTubeProfile, churchOrDefault.LinkedInProfile) :
                    (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

                var (facebookLink, twitterLink, instagramLink, youtubeLink, linkedinLink) = (
                    GenerateSocialMediaLink(facebookProfile, "facebook", SocialMediaIcons.Facebook),
                    GenerateSocialMediaLink(twitterProfile, "twitter", SocialMediaIcons.Twitter),
                    GenerateSocialMediaLink(instagramProfile, "instagram", SocialMediaIcons.Instagram),
                    GenerateSocialMediaLink(youTubeProfile, "youtube", SocialMediaIcons.YouTube),
                    GenerateSocialMediaLink(linkedInProfile, "linkedin", SocialMediaIcons.LinkedIn)
                );

                // Prepare email body
                var body = EmailTemplates.Base.Replace("{body_content}", message.Message)
                    .Replace("{site-name}", name)
                    .Replace("{alt-logo}", altLogo)
                    .Replace("{site-logo}", logo)
                    .Replace("{church-name}", churchOrDefault?.Display ?? "Praise Church")
                    .Replace("{church-address}", churchOrDefault?.PhysicalAddress ?? string.Empty)
                    .Replace("{facebook_url}", facebookLink)
                    .Replace("{twitter_url}", twitterLink)
                    .Replace("{instagram_url}", instagramLink)
                    .Replace("{youtube_url}", youtubeLink)
                    .Replace("{linkedin_url}", linkedinLink)
                    .Replace("{site-url}", baseUrl);

                // Verify and set email recipient block
                if (message.To.IsNotNullOrEmpty())
                {
                    body = VerifyEmailBlock(body, message.To.Split(',').Select(x => x.Trim()).FirstOrDefault(),
                                            message.Subject == "Account Setup" || message.Subject == "Email verification" ||
                                            message.Subject == "Church Registration" || message.Subject.Contains("invited you to join Praise CMS."));
                }

                // Compose the email message
                var email = new MailMessage
                {
                    IsBodyHtml = true,
                    Subject = message.Subject,
                    Body = Utilities.ValidateEmailStatusUpdater(body)
                };

                // Set email sender
                if (!string.IsNullOrEmpty(replyEmailAddress) && !string.IsNullOrEmpty(replyName))
                {
                    email.From = new MailAddress(replyEmailAddress, replyName);
                }
                else if (sendFromCurrentUser && SessionVariables.CurrentUser != null)
                {
                    email.From = new MailAddress(SessionVariables.CurrentUser.User.Email, SessionVariables.CurrentUser.User.FullName);
                }
                else
                {
                    email.From = domain != null ? new MailAddress(domain.EmailReplyAddress, domain.EmailDisplay) :
                                                  new MailAddress(ApplicationCache.Instance.EmailConfiguration.ReplyTo, ApplicationCache.Instance.EmailConfiguration.Display);
                }

                // Add email recipients
                if (message.To.IsNotNullOrEmpty())
                {
                    foreach (var recipient in message.To.Split(',').Select(x => x.Trim()))
                    {
                        if (ExtensionMethods.CheckEmailIsValid(recipient))
                        {
                            email.To.Add(recipient);
                        }
                    }
                }

                // Add Reply-To address
                email.ReplyToList.Add(new MailAddress("no-reply@praisecms.com"));

                // Add CC recipients
                if (!string.IsNullOrWhiteSpace(message.Cc))
                {
                    foreach (var recipient in message.Cc.Split(',').Select(x => x.Trim()))
                    {
                        if (ExtensionMethods.CheckEmailIsValid(recipient))
                        {
                            email.CC.Add(recipient);
                        }
                    }
                }

                // Add BCC recipients
                if (!string.IsNullOrWhiteSpace(message.Bcc))
                {
                    foreach (var recipient in message.Bcc.Split(',').Select(x => x.Trim()))
                    {
                        if (ExtensionMethods.CheckEmailIsValid(recipient))
                        {
                            email.Bcc.Add(recipient);
                        }
                    }
                }

                // Process email attachments
                if (attachments != null)
                {
                    foreach (var emailAttachment in attachments)
                    {
                        ProcessEmailAttachment(emailAttachment, email, message);
                    }
                }

                // Send the email if email sending is enabled
                if (ShouldSendEmail(message))
                {
                    var server = new SmtpClient(ApplicationCache.Instance.EmailConfiguration.Smtp)
                    {
                        Port = Convert.ToInt32(ApplicationCache.Instance.EmailConfiguration.Port),
                        Credentials = new System.Net.NetworkCredential(ApplicationCache.Instance.EmailConfiguration.Username, ApplicationCache.Instance.EmailConfiguration.Password),
                        EnableSsl = ApplicationCache.Instance.EmailConfiguration.EnableSsl
                    };

                    server.Send(email);
                    message.Sent = true;
                    message.Status = EmailStatus.Sent;
                }
                else
                {
                    message.Status = EmailStatus.Queued;
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                HandleEmailSendingError(ex, message, church);
            }
            finally
            {
                SaveEmailToDatabase(message);
            }

            return message.Sent;
        }

        // Helper methods
        private static string GenerateSocialMediaLink(string profileUrl, string baseUrl, string iconFileName)
        {
            if (!string.IsNullOrEmpty(profileUrl))
            {
                return $"<a class='o_text-light' href='https://{profileUrl}' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'>" +
                       $"<img src='{baseUrl}/Content/assets/image/email_templates/{iconFileName}' width='36' height='36' alt='{profileUrl}' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>";
            }
            return string.Empty;
        }

        private static void ProcessEmailAttachment(EmailAttachmentVM emailAttachment, MailMessage email, Email message)
        {
            if (emailAttachment.Attachment?.Length > 0)
            {
                var fileName = !string.IsNullOrWhiteSpace(emailAttachment.FileName) ? emailAttachment.FileName : $"{DateTime.Now.Ticks}.pdf";

                var stream = new MemoryStream(emailAttachment.Attachment);
                var attachment = new Attachment(stream, fileName);

                email.Attachments.Add(attachment);

                try
                {
                    var awsStream = new MemoryStream(emailAttachment.Attachment);
                    var uniqueFileName = Utilities.GenerateUniqueFileName(fileName);
                    var success = AwsHelpers.UploadFile(uniqueFileName, awsStream);
                    if (success)
                    {
                        message.Attachments += $"{uniqueFileName};";
                    }
                }
                catch
                {
                    // Ignore
                }
            }
        }

        private static bool ShouldSendEmail(Email message)
        {
            var emailsTurnedOn = "EmailsTurnedOn".AppSetting(false);
            return emailsTurnedOn || (message.To.IsNotNullOrEmpty() && (message.To == "coggicc@gmail.com" ||
                   message.To == "info@novadevelopment.net" || message.To.Contains("@praisecms.com")));
        }

        private static void HandleEmailSendingError(Exception ex, Email message, Church church)
        {
            message.Sent = false;
            message.Status = EmailStatus.Error;
            var exceptionMessage = ex.Message;
            var innerException = ex.InnerException;
            var innerExceptionMessage = innerException != null ? innerException.InnerException?.Message ?? "Inner Exception Null" : "Inner Exception Null";

            var log = new Log
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = church != null && !string.IsNullOrEmpty(church.Id) ? church.Id : string.Empty,
                Controller = "Emailer",
                Action = "SendEmail",
                Status = "Error",
                Parameter = $"Exception Message: {exceptionMessage} || Inner Exception Message: {innerExceptionMessage}",
                Type = "Email",
                TypeId = !string.IsNullOrEmpty(message.Id) ? message.Id : string.Empty,
                Autosave = false,
                CreatedDate = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(message.CreatedBy) ? message.CreatedBy : string.Empty
            };
            db.Logs.Add(log);
            db.SaveChanges();
        }

        private static void SaveEmailToDatabase(Email message)
        {
            db.Emails.Add(message);
            db.SaveChanges();
        }

        public static bool SendEmail(Email message, byte[] fileAttachment = null, string fileAttachmentName = null, Domain domain = null, Church church = null, bool sendFromCurrentUser = false, string replyEmailAddress = null, string replyName = null)
        {
            var emailAttachment = fileAttachment != null ? new EmailAttachmentVM(fileAttachment, fileAttachmentName) : null;
            var emailAttachments = emailAttachment != null ? new List<EmailAttachmentVM> { emailAttachment } : new List<EmailAttachmentVM>();

            return SendEmail(message, emailAttachments, domain, church, sendFromCurrentUser, replyEmailAddress, replyName);
        }

        public static ResponseModel SendPlainEmail(string Subject, string message, MailAddress from, MailAddress to = null, List<MailAddress> multipleTo = null, List<MailAddress> cc = null, List<MailAddress> bCC = null, string type = null, string typeId = null)
        {
            var response = new ResponseModel();
            var emailModel = new Email()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                Type = type,
                TypeId = typeId,
                Message = message,
                Subject = Subject,
                Text = message
            };

            //remove HTML from message
            System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex("<[^>]*>");
            emailModel.Text = rx.Replace(emailModel.Text, string.Empty);

            try
            {
                var email = new MailMessage
                {
                    IsBodyHtml = true,
                    Subject = Subject,
                    Body = message,
                    From = from
                };
                email.ReplyToList.Add(from);

                if (multipleTo.IsNullOrEmpty() && cc.IsNullOrEmpty() && bCC.IsNullOrEmpty() && to.IsNullOrEmpty())
                {
                    response.Message = "No recipient available.";
                    response.Success = false;

                    return response;
                }

                if (multipleTo.IsNotNullOrEmpty() && multipleTo.Any())
                {
                    foreach (var mail in multipleTo)
                    {
                        email.To.Add(mail);
                        emailModel.To += $"{mail.Address}, ";
                    }
                }
                else if (to.IsNotNullOrEmpty())
                {
                    email.To.Add(to);
                    emailModel.To = to.Address;
                }

                if (cc.IsNotNullOrEmpty() && cc.Any())
                {
                    foreach (var mail in cc)
                    {
                        email.CC.Add(mail);
                        emailModel.Cc += $"{mail.Address}, ";
                    }

                    if (emailModel.To.IsNullOrEmpty())
                    {
                        emailModel.To = emailModel.Cc;
                    }
                }

                if (bCC.IsNotNullOrEmpty() && bCC.Any())
                {
                    foreach (var mail in bCC)
                    {
                        email.Bcc.Add(mail);
                        emailModel.Bcc += $"{mail.Address}, ";
                    }

                    if (emailModel.To.IsNullOrEmpty())
                    {
                        emailModel.To = emailModel.Bcc;
                    }
                }
                emailModel.To = emailModel.To.IsNotNullOrEmpty() ? emailModel.To.TrimEnd(',').Trim() : emailModel.To;
                emailModel.Cc = emailModel.Cc.IsNotNullOrEmpty() ? emailModel.Cc.TrimEnd(',').Trim() : emailModel.Cc;
                emailModel.Bcc = emailModel.Bcc.IsNotNullOrEmpty() ? emailModel.Bcc.TrimEnd(',').Trim() : emailModel.Bcc;

                var server = new SmtpClient(ApplicationCache.Instance.EmailConfiguration.Smtp)
                {
                    Port = Convert.ToInt32(ApplicationCache.Instance.EmailConfiguration.Port),
                    Credentials = new System.Net.NetworkCredential(ApplicationCache.Instance.EmailConfiguration.Username, ApplicationCache.Instance.EmailConfiguration.Password),
                    EnableSsl = ApplicationCache.Instance.EmailConfiguration.EnableSsl
                };

                //Check if we have email sending turned on or if we are sending to one of my emails.  It is off by default for testing purposes.
                var emailsTurnedOn = "EmailsTurnedOn".AppSetting(false);

                if (emailsTurnedOn)
                {
                    server.Send(email);
                }

                response.Success = emailModel.Sent = true;
                response.Message = "Your email has been sent.";
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                emailModel.Sent = false;
                response.Success = emailModel.Sent;
                response.Message = $"{Constants.DefaultErrorMessage} {ex.Message}";
            }
            finally
            {
                db.Emails.Add(emailModel);
                db.SaveChanges();
            }

            return response;
        }

        public static string VerifyEmailBlock(string body, string email, bool clear = false)
        {
            var user = db.Users.FirstOrDefault(x => x.Email == email);
            var superAdmin = ConfigurationManager.AppSettings["SuperAdmin"];

            if (user?.EmailConfirmed == false && email != superAdmin && !clear)
            {
                user.EmailVerificationCode = Utilities.GenerateUniqueId();
                db.SaveChanges();
                return body.Replace("{unverify_email}", EmailTemplates.UnverifyEmail)
                    .Replace("{btn_url}", $"{ApplicationCache.Instance.SiteConfiguration.Url}/users/_VerifyEmail?verificationCode={user.EmailVerificationCode}&id={user.Id}");
            }
            else
            {
                return body.Replace("{unverify_email}", string.Empty);
            }
        }
    }
}