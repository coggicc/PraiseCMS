using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PraiseCMS.BusinessLayer
{
    public class EmailTemplateOperations : GenericRepository
    {
        public EmailTemplateOperations(ApplicationDbContext db, Work work)
           : base(db, work)
        {
        }

        public EmailTemplate Get(string id)
        {
            return Read<EmailTemplate>().FirstOrDefault(x => x.Id == id);
        }

        public List<EmailTemplate> GetEmailTemplates(string churchId = null, bool includeSystemTemplates = false)
        {
            var query = Read<EmailTemplate>().AsQueryable();

            if (!string.IsNullOrEmpty(churchId))
            {
                query = query.Where(x => x.Type == EmailTemplateTypes.Church && x.TypeId == churchId);
            }

            if (includeSystemTemplates)
            {
                query = query.Where(x => x.Type == EmailTemplateTypes.System || (x.Type == EmailTemplateTypes.Church && x.TypeId == churchId));
            }

            return query.OrderBy(x => x.Name).ToList();
        }

        //public string GetEmailTemplateByNameOld(string value, bool includeUnverified = false)
        //{
        //    var church = Work.Church.Get(SessionVariables.CurrentChurch.Id);
        //    var isChurchEmail = false;
        //    string body;

        //    switch (value)
        //    {
        //        case EmailTemplatesNameList.VerificationCode:
        //            body = EmailTemplates.VerificationCode_body.Replace("{verification-code}", "123456");
        //            break;
        //        case EmailTemplatesNameList.PaymentProcessed:
        //            isChurchEmail = true;
        //            body = EmailTemplates.PaymentProcessed_body.Replace("{amount}", 100m.ToCurrencyString())
        //               .Replace("{gift_datetime}", DateTime.Now.ToShortDateString())
        //               .Replace("{user_display}", "John")
        //               .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
        //               .Replace("{paymentmethod}", "VISA ending in ****1234")
        //               .Replace("{fund_display}", "Tithes and Offerings")
        //               .Replace("{campus_display}", "Birmingham")
        //               .Replace("{transactionid}", "123456789123Z99")
        //               .Replace("{church_thanks_note}", !string.IsNullOrEmpty(SessionVariables.CurrentChurch.GivingThankYouText) ? SessionVariables.CurrentChurch.GivingThankYouText : "Thank you for your gift.");
        //            break;
        //        case EmailTemplatesNameList.PaymentError:
        //            isChurchEmail = true;
        //            body = EmailTemplates.Payment_Error_body.Replace("{amount}", 0m.ToCurrencyString())
        //                .Replace("{user_display}", "John")
        //                .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
        //                .Replace("{paymentmethod}", "VISA ending in ****1234")
        //                .Replace("{fund_display}", "Tithes and Offerings")
        //                .Replace("{campus_display}", "Birmingham")
        //                .Replace("{error_message}", "Your credit card has expired. Please add a new payment method and submit your gift again.");
        //            break;
        //        case EmailTemplatesNameList.ScheduledGiving:
        //            isChurchEmail = true;
        //            body = EmailTemplates.ScheduledGiving_body.Replace("{amount}", 0m.ToCurrencyString())
        //              .Replace("{user_display}", "John")
        //              .Replace("{start_date}", DateTime.Now.ToShortDateString())
        //              .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
        //              .Replace("{paymentmethod}", "VISA ending in ****1234")
        //              .Replace("{fund_display}", "Tithes and Offerings")
        //              .Replace("{frequency}", "Monthly");
        //            break;
        //        case EmailTemplatesNameList.ForgotPassword:
        //            body = EmailTemplates.ForgotPassword_body.Replace("{btn_url}", "#");
        //            break;
        //        case EmailTemplatesNameList.PasswordChanged:
        //            body = EmailTemplates.PasswordChanged_body.Replace("{btn_url}", "#");
        //            break;
        //        case EmailTemplatesNameList.ResetPassword:
        //            body = EmailTemplates.ResetPassword_body.Replace("{btn_url}", "#").Replace("{temp_password}", "ukLkM@432");
        //            break;
        //        case EmailTemplatesNameList.VerifyEmail:
        //            body = EmailTemplates.VerifyEmail_body.Replace("{btn_url}", "#");
        //            includeUnverified = false;
        //            break;
        //        case EmailTemplatesNameList.NewUserAccount:
        //            body = EmailTemplates.NewUserAccount_body
        //           .Replace("{createdBy}", "John Doe")
        //           .Replace("{username}", "user@mail.com")
        //           .Replace("{password}", "#Temp1234!")
        //           .Replace("{btn_url}", "#");
        //            break;
        //        case EmailTemplatesNameList.ChurchRegistration:
        //            includeUnverified = false;
        //            body = EmailTemplates.ChurchRegistration_body.Replace("{btn_url}", "#").Replace("{message}", $"With your {Utilities.GetFreeTrialDays()}-day free trial, you can receive digital gifts, create events, manage prayer requests, and much more. Be sure to add a payment method before your free trial ends so you don't lose out on these great services.");
        //            break;
        //        case EmailTemplatesNameList.ChurchRegistrationSuperAdmin:
        //            includeUnverified = false;
        //            body = EmailTemplates.ChurchRegistrationSuperAdmin_body
        //            .Replace("{churchName}", SessionVariables.CurrentChurch.Display)
        //            .Replace("{phone}", "(555) 555-5555")
        //            .Replace("{email}", "name@email.com")
        //            .Replace("{church-admin}", "John Doe")
        //            .Replace("{created_datetime}", DateTime.Now.ToShortDateString());
        //            break;
        //        case EmailTemplatesNameList.AnnualGivingStatement:
        //            isChurchEmail = true;
        //            body = church.AnnualStatementEmailBody.IsNotNullOrEmpty()
        //                ? EmailTemplates.AnnualGivingStatement_body.Replace("{body}", church.AnnualStatementEmailBody.Replace("{current-year}", DateTime.Now.Year.ToString()))
        //                : EmailTemplates.AnnualGivingStatement_body.Replace("{body}", "Your {current-year} annual giving statement for {church_display} is now available");
        //            body = body.Replace("{current-year}", DateTime.Now.Year.ToString())
        //            .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
        //            .Replace("{view_statment_url}", "#");
        //            break;
        //        case EmailTemplatesNameList.SendInvitation:
        //            includeUnverified = false;
        //            body = EmailTemplates.InvitationEmail_body.Replace("{message}", "John Doe would like to invite you to use Praise CMS, a total church management software solution.<br><br>Praise is loaded with a ton of great features such as digital giving, powerful reporting, prayer requests, and more. Start your FREE trial and enjoy all Praise has to offer.");
        //            body = body.Replace("{btn_url}", "#");
        //            break;
        //        default:
        //            return "No Preview available";
        //    }

        //    var baseUrl = ApplicationCache.Instance.SiteConfiguration.Url;
        //    var name = ApplicationCache.Instance.SiteConfiguration.Name;
        //    var facebookProfile = string.Empty;
        //    var twitterProfile = string.Empty;
        //    var instagramProfile = string.Empty;
        //    var youTubeProfile = string.Empty;
        //    var linkedInProfile = string.Empty;
        //    var facebookLink = string.Empty;
        //    var twitterLink = string.Empty;
        //    var instagramLink = string.Empty;
        //    var youtubeLink = string.Empty;
        //    var linkedinLink = string.Empty;

        //    if (isChurchEmail)
        //    {
        //        body = EmailTemplates.Base.Replace("{body_content}", body)
        //                                  .Replace("{site-name}", name)
        //                                  .Replace("{site-logo}", church.Logo.IsNotNullOrEmpty() ? AwsHelpers.AmazonLink(church.Logo, "Uploads/Logos") : baseUrl + "/Content/assets/media/logos/default_logo.png")
        //                                  .Replace("{alt-logo}", church.Display)
        //                                  .Replace("{year}", DateTime.Now.Year.ToString())
        //                                  .Replace("{church-name}", church.Display)
        //                                  .Replace("{church-address}", church.PhysicalAddress);

        //        facebookProfile = !string.IsNullOrEmpty(church.FacebookProfile) ? church.FacebookProfile : string.Empty;
        //        twitterProfile = !string.IsNullOrEmpty(church.TwitterProfile) ? church.TwitterProfile : string.Empty;
        //        instagramProfile = !string.IsNullOrEmpty(church.InstagramProfile) ? church.InstagramProfile : string.Empty;
        //        youTubeProfile = !string.IsNullOrEmpty(church.YouTubeProfile) ? church.YouTubeProfile : string.Empty;
        //        linkedInProfile = !string.IsNullOrEmpty(church.LinkedInProfile) ? church.LinkedInProfile : string.Empty;

        //        facebookLink = !string.IsNullOrEmpty(facebookProfile) ? "<a class='o_text-light' href='https://" + facebookProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/facebook-light.png' width='36' height='36' alt='fb' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>" : string.Empty;
        //        twitterLink = !string.IsNullOrEmpty(twitterProfile) ? "<a class='o_text-light' href='https://" + twitterProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/twitter-light.png' width='36' height='36' alt='tw' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>" : string.Empty;
        //        instagramLink = !string.IsNullOrEmpty(instagramProfile) ? "<a class='o_text-light' href='https://" + instagramProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/instagram-light.png' width='36' height='36' alt='ig' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>" : string.Empty;
        //        youtubeLink = !string.IsNullOrEmpty(youTubeProfile) ? "<a class='o_text-light' href='https://" + youTubeProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/youtube-light.png' width='36' height='36' alt='sc' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>" : string.Empty;
        //        linkedinLink = !string.IsNullOrEmpty(linkedInProfile) ? "<a class='o_text-light' href='https://" + linkedInProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/linkedin-light.png' width='36' height='36' alt='sc' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>" : string.Empty;
        //    }
        //    else
        //    {
        //        var praiseChurch = Work.Church.GetPraiseChurch();
        //        var logo = baseUrl + "/Content/assets/media/logos/praise_logo_white_blue.png";
        //        body = EmailTemplates.Base.Replace("{body_content}", body)
        //                                  .Replace("{site-name}", name)
        //                                  .Replace("{site-logo}", logo)
        //                                  .Replace("{alt-logo}", "Prase CMS")
        //                                  .Replace("{year}", DateTime.Now.Year.ToString())
        //                                  .Replace("{church-name}", !string.IsNullOrEmpty(praiseChurch.Name) ? praiseChurch.Name : "Praise CMS")
        //                                  .Replace("{church-address}", !string.IsNullOrEmpty(praiseChurch.PhysicalAddress) ? praiseChurch.PhysicalAddress : "2637 Montauk Rd, Hoover, AL 35226");

        //        facebookProfile = !string.IsNullOrEmpty(praiseChurch.FacebookProfile) ? praiseChurch.FacebookProfile : "facebook.com/praisecms";
        //        twitterProfile = !string.IsNullOrEmpty(praiseChurch.TwitterProfile) ? praiseChurch.TwitterProfile : "twitter.com/PraiseCMS";
        //        instagramProfile = !string.IsNullOrEmpty(praiseChurch.InstagramProfile) ? praiseChurch.InstagramProfile : "instagram.com/praisecms";
        //        youTubeProfile = !string.IsNullOrEmpty(praiseChurch.YouTubeProfile) ? praiseChurch.YouTubeProfile : "youtube.com/channel/UC4sCxlKd_alAC5I91zWEE2Q";
        //        linkedInProfile = !string.IsNullOrEmpty(praiseChurch.LinkedInProfile) ? praiseChurch.LinkedInProfile : "linkedin.com/company/praise-church-management-solutions";

        //        facebookLink = !string.IsNullOrEmpty(facebookProfile) ? "<a class='o_text-light' href='https://" + facebookProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/facebook-light.png' width='36' height='36' alt='fb' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>" : string.Empty;
        //        twitterLink = !string.IsNullOrEmpty(twitterProfile) ? "<a class='o_text-light' href='https://" + twitterProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/twitter-light.png' width='36' height='36' alt='tw' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>" : string.Empty;
        //        instagramLink = !string.IsNullOrEmpty(instagramProfile) ? "<a class='o_text-light' href='https://" + instagramProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/instagram-light.png' width='36' height='36' alt='ig' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>" : string.Empty;
        //        youtubeLink = !string.IsNullOrEmpty(youTubeProfile) ? "<a class='o_text-light' href='https://" + youTubeProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/youtube-light.png' width='36' height='36' alt='sc' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>" : string.Empty;
        //        linkedinLink = !string.IsNullOrEmpty(linkedInProfile) ? "<a class='o_text-light' href='https://" + linkedInProfile + "' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'><img src='{site-url}/Content/assets/image/email_templates/linkedin-light.png' width='36' height='36' alt='sc' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a>" : string.Empty;
        //    }

        //    body = body.Replace("{facebook_url}", facebookLink)
        //            .Replace("{twitter_url}", twitterLink)
        //            .Replace("{instagram_url}", instagramLink)
        //            .Replace("{youtube_url}", youtubeLink)
        //            .Replace("{linkedin_url}", linkedinLink)
        //            .Replace("{site-url}", baseUrl);
        //    body = includeUnverified ? body.Replace("{unverify_email}", EmailTemplates.UnverifyEmail).Replace("{btn_url}", "#") : body.Replace("{unverify_email}", string.Empty);

        //    return body;
        //}

        public string GetEmailTemplateByName(string templateName, bool includeUnverified = false)
        {
            var church = Work.Church.Get(SessionVariables.CurrentChurch.Id);
            string body;
            bool isChurchEmail = false;

            switch (templateName)
            {
                case EmailTemplatesNameList.VerificationCode:
                    body = EmailTemplates.VerificationCode_body.Replace("{verification-code}", "123456");
                    break;
                case EmailTemplatesNameList.PaymentProcessed:
                    isChurchEmail = true;
                    body = FormatPaymentProcessedEmail();
                    break;
                case EmailTemplatesNameList.PaymentError:
                    isChurchEmail = true;
                    body = FormatPaymentErrorEmail();
                    break;
                case EmailTemplatesNameList.ScheduledGiving:
                    isChurchEmail = true;
                    body = FormatScheduledGivingEmail();
                    break;
                case EmailTemplatesNameList.ForgotPassword:
                    body = EmailTemplates.ForgotPassword_body.Replace("{btn_url}", "#");
                    break;
                case EmailTemplatesNameList.PasswordChanged:
                    body = EmailTemplates.PasswordChanged_body.Replace("{btn_url}", "#");
                    break;
                case EmailTemplatesNameList.ResetPassword:
                    body = EmailTemplates.ResetPassword_body
                          .Replace("{btn_url}", "#")
                          .Replace("{temp_password}", "ukLkM@432");
                    break;
                case EmailTemplatesNameList.VerifyEmail:
                    body = EmailTemplates.VerifyEmail_body.Replace("{btn_url}", "#");
                    includeUnverified = false;
                    break;
                case EmailTemplatesNameList.NewUserAccount:
                    body = FormatNewUserAccountEmail();
                    break;
                case EmailTemplatesNameList.ChurchRegistration:
                    includeUnverified = false;
                    body = FormatChurchRegistrationEmail();
                    break;
                case EmailTemplatesNameList.ChurchRegistrationSuperAdmin:
                    includeUnverified = false;
                    body = FormatChurchRegistrationSuperAdminEmail();
                    break;
                case EmailTemplatesNameList.AnnualGivingStatement:
                    isChurchEmail = true;
                    body = FormatAnnualGivingStatementEmail(church);
                    break;
                case EmailTemplatesNameList.SendInvitation:
                    includeUnverified = false;
                    body = FormatSendInvitationEmail();
                    break;
                default:
                    return "No Preview available";
            }

            return FormatEmailBody(body, isChurchEmail, church, includeUnverified);
        }

        private string FormatPaymentProcessedEmail()
        {
            return EmailTemplates.PaymentProcessed_body
                   .Replace("{amount}", 100m.ToCurrencyString())
                   .Replace("{gift_datetime}", DateTime.Now.ToShortDateString())
                   .Replace("{user_display}", "John")
                   .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
                   .Replace("{paymentmethod}", "VISA ending in ****1234")
                   .Replace("{fund_display}", "Tithes and Offerings")
                   .Replace("{campus_display}", "Birmingham")
                   .Replace("{transactionid}", "123456789123Z99")
                   .Replace("{church_thanks_note}",
                       !string.IsNullOrEmpty(SessionVariables.CurrentChurch.GivingThankYouText)
                           ? SessionVariables.CurrentChurch.GivingThankYouText
                           : "Thank you for your gift.");
        }

        private string FormatPaymentErrorEmail()
        {
            return EmailTemplates.Payment_Error_body
                   .Replace("{amount}", 0m.ToCurrencyString())
                   .Replace("{user_display}", "John")
                   .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
                   .Replace("{paymentmethod}", "VISA ending in ****1234")
                   .Replace("{fund_display}", "Tithes and Offerings")
                   .Replace("{campus_display}", "Birmingham")
                   .Replace("{error_message}",
                       "Your credit card has expired. Please add a new payment method and submit your gift again.");
        }

        private string FormatScheduledGivingEmail()
        {
            return EmailTemplates.ScheduledGiving_body
                   .Replace("{amount}", 0m.ToCurrencyString())
                   .Replace("{user_display}", "John")
                   .Replace("{start_date}", DateTime.Now.ToShortDateString())
                   .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
                   .Replace("{paymentmethod}", "VISA ending in ****1234")
                   .Replace("{fund_display}", "Tithes and Offerings")
                   .Replace("{frequency}", "Monthly");
        }

        private string FormatNewUserAccountEmail()
        {
            return EmailTemplates.NewUserAccount_body
                   .Replace("{createdBy}", "John Doe")
                   .Replace("{username}", "user@mail.com")
                   .Replace("{password}", "#Temp1234!")
                   .Replace("{btn_url}", "#");
        }

        private string FormatChurchRegistrationEmail()
        {
            return EmailTemplates.ChurchRegistration_body
                   .Replace("{btn_url}", "#")
                   .Replace("{message}",
                       $"With your {Utilities.GetFreeTrialDays()}-day free trial, you can receive digital gifts, create events, manage prayer requests, and much more. Be sure to add a payment method before your free trial ends so you don't lose out on these great services.");
        }

        private string FormatChurchRegistrationSuperAdminEmail()
        {
            return EmailTemplates.ChurchRegistrationSuperAdmin_body
                   .Replace("{churchName}", SessionVariables.CurrentChurch.Display)
                   .Replace("{phone}", "(555) 555-5555")
                   .Replace("{email}", "name@email.com")
                   .Replace("{church-admin}", "John Doe")
                   .Replace("{created_datetime}", DateTime.Now.ToShortDateString());
        }

        private string FormatAnnualGivingStatementEmail(Church church)
        {
            var body = church.AnnualStatementEmailBody.IsNotNullOrEmpty()
                       ? EmailTemplates.AnnualGivingStatement_body
                             .Replace("{body}", church.AnnualStatementEmailBody
                                 .Replace("{current-year}", DateTime.Now.Year.ToString()))
                       : EmailTemplates.AnnualGivingStatement_body
                             .Replace("{body}",
                                 $"Your {DateTime.Now.Year} annual giving statement for {SessionVariables.CurrentChurch.Display} is now available");

            return body.Replace("{current-year}", DateTime.Now.Year.ToString())
                       .Replace("{church_display}", SessionVariables.CurrentChurch.Display)
                       .Replace("{view_statment_url}", "#");
        }

        private string FormatSendInvitationEmail()
        {
            return EmailTemplates.InvitationEmail_body
                   .Replace("{message}",
                       "John Doe would like to invite you to use Praise CMS, a total church management software solution.<br><br>Praise is loaded with a ton of great features such as digital giving, powerful reporting, prayer requests, and more. Start your FREE trial and enjoy all Praise has to offer.")
                   .Replace("{btn_url}", "#");
        }

        private string FormatEmailBody(string bodyContent, bool isChurchEmail, Church church, bool includeUnverified)
        {
            var siteConfig = ApplicationCache.Instance.SiteConfiguration;
            var baseUrl = siteConfig.Url;
            var name = siteConfig.Name;
            var logo = isChurchEmail
                ? church.Logo.IsNotNullOrEmpty() ? AwsHelpers.AmazonLink(church.Logo, "Uploads/Logos") : $"{baseUrl}/Content/assets/media/logos/default_logo.png"
                : $"{baseUrl}/Content/assets/media/logos/praise_logo_white_blue.png";

            bodyContent = EmailTemplates.Base.Replace("{body_content}", bodyContent)
                                             .Replace("{site-name}", name)
                                             .Replace("{site-logo}", logo)
                                             .Replace("{alt-logo}", isChurchEmail ? church.Display : "Praise CMS")
                                             .Replace("{year}", DateTime.Now.Year.ToString())
                                             .Replace("{church-name}", isChurchEmail ? church.Display : "Praise CMS")
                                             .Replace("{church-address}", isChurchEmail ? church.PhysicalAddress : "2637 Montauk Rd, Hoover, AL 35226");

            var socialLinks = GenerateSocialLinks(isChurchEmail ? church : Work.Church.GetPraiseChurch(), baseUrl);

            // Handle the unverified email section
            bodyContent = includeUnverified
                ? bodyContent.Replace("{unverify_email}", EmailTemplates.UnverifyEmail).Replace("{btn_url}", "#")
                : bodyContent.Replace("{unverify_email}", string.Empty);

            return bodyContent.Replace("{social-links}", socialLinks);
        }

        private string GenerateSocialLinks(Church church, string baseUrl)
        {
            string[] profiles = {
                church.FacebookProfile,
                //church.TwitterProfile,
                church.InstagramProfile
                //church.YouTubeProfile,
                //church.LinkedInProfile
            };
            string[] icons = {
                "facebook-light.png",
                //"twitter-light.png",
                "instagram-light.png"
                //"youtube-light.png",
                //"linkedin-light.png"
            };
            var links = new StringBuilder();

            for (int i = 0; i < profiles.Length; i++)
            {
                if (!string.IsNullOrEmpty(profiles[i]))
                {
                    links.Append($"<a class='o_text-light' href='https://{profiles[i]}' target='_blank' style='text-decoration: none;outline: none;color: #82899a;'>");
                    links.Append($"<img src='{baseUrl}/Content/assets/image/email_templates/{icons[i]}' width='36' height='36' alt='{icons[i].Substring(0, 2)}' style='max-width: 36px;-ms-interpolation-mode: bicubic;vertical-align: middle;border: 0;line-height: 100%;height: auto;outline: none;text-decoration: none;'></a><span> &nbsp;</span>");
                }
            }

            return links.ToString();
        }

        #region CRUD
        public void Create(EmailTemplate entity)
        {
            Create<EmailTemplate>(entity);
            SaveChanges();
        }

        public void Update(EmailTemplate entity)
        {
            Update<EmailTemplate>(entity);
            SaveChanges();
        }

        public void Delete(string id)
        {
            var entity = Get(id);
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = SessionVariables.CurrentUser?.User?.Id;
            Delete<EmailTemplate>(entity);
            SaveChanges();
        }

        public void Delete(EmailTemplate entity)
        {
            Delete<EmailTemplate>(entity);
            SaveChanges();
        }
        #endregion
    }
}