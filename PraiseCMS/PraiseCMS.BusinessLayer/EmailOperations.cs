using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PraiseCMS.BusinessLayer
{
    public class EmailOperations : GenericRepository
    {
        public EmailOperations(ApplicationDbContext db, Work work)
         : base(db, work)
        {
        }

        public Email Get(string id)
        {
            return Read<Email>().FirstOrDefault(x => x.Id == id);
        }

        public List<Email> GetAll()
        {
            return Read<Email>().OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<Email> GetAllByChurchId(string churchId)
        {
            var churchUsers = Work.User.GetAllUsersIdsByChurchId(churchId);
            if (churchUsers.Count > 0)
                return Read<Email>().Where(x => churchUsers.Contains(x.CreatedBy) || x.CreatedBy == churchId).OrderByDescending(x => x.CreatedDate).ToList();
            return new List<Email>();
        }

        public List<Email> GetAllByUserId(string userId)
        {
            return Read<Email>().Where(x => x.CreatedBy == userId).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public List<Email> GetAllByStatus(string status)
        {
            return Read<Email>().Where(x => !string.IsNullOrEmpty(x.Status) && x.Status.Equals(status)).OrderByDescending(x => x.CreatedDate).ToList();
        }

        #region CRUD
        public void Create(Email entity)
        {
            Create<Email>(entity);
            SaveChanges();
        }

        public void Update(Email entity)
        {
            Update<Email>(entity);
            SaveChanges();
        }

        public void Delete(string id)
        {
            var entity = Get(id);
            Delete<Email>(entity);
            SaveChanges();
        }

        public void Delete(Email entity)
        {
            Delete<Email>(entity);
            SaveChanges();
        }
        #endregion

        #region Email Campaign
        public List<EmailCampaign> GetAllCampaign(string churchId)
        {
            return Read<EmailCampaign>().Where(x => x.ChurchId == churchId).ToList();
        }

        public EmailCampaign GetCampaign(string id)
        {
            return Read<EmailCampaign>().FirstOrDefault(x => x.Id == id);
        }
        #endregion

        #region CRUD Email Campaign
        public void CreateCampaign(EmailCampaign entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateCampaign(EmailCampaign entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteCampaign(string id)
        {
            var entity = GetCampaign(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteCampaign(EmailCampaign entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion

        public void UpdateStatus(string emailId)
        {
            var email = Work.Email.Get(emailId);
            if (email.IsNotNull())
            {
                email.ViewedCount++;
                Work.Email.Update(email);
            }
        }

        public async Task SendQueuedEmailsAsync()
        {
            var queuedEmails = Work.Email.GetAllByStatus(EmailStatus.Queued);

            foreach (var email in queuedEmails)
            {
                try
                {
                    using (var mail = new MailMessage())
                    {
                        mail.IsBodyHtml = true;
                        mail.Subject = email.Subject;
                        mail.Body = email.Message;

                        if (email.To.IsNotNullOrEmpty())
                        {
                            foreach (var recipient in email.To.Split(',').Select(x => x.Trim()))
                            {
                                mail.To.Add(recipient);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(email.Cc))
                        {
                            foreach (var recipient in email.Cc.Split(',').Select(x => x.Trim()))
                            {
                                mail.CC.Add(recipient);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(email.Bcc))
                        {
                            foreach (var recipient in email.Bcc.Split(',').Select(x => x.Trim()))
                            {
                                mail.Bcc.Add(recipient);
                            }
                        }

                        mail.From = new MailAddress("EmailUsername".AppSetting("info@praisecms.com"));

                        using (var server = new SmtpClient("EmailSmtpServer".AppSetting("smtp.gmail.com")))
                        {
                            server.Port = Convert.ToInt32("EmailPort".AppSetting("587"));
                            server.Credentials = new System.Net.NetworkCredential("EmailUsername".AppSetting("info@praisecms.com"), "EmailPassword".AppSetting("zllf xizw cmvt pbxk"));
                            server.EnableSsl = "EmailEnableSsl".AppSetting(true);

                            await server.SendMailAsync(mail);
                        }

                        email.Sent = true;
                        email.Status = EmailStatus.Sent;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    email.Sent = false;
                    email.Status = EmailStatus.Error;
                }
            }

            if (queuedEmails.Any())
            {
                await Db.SaveChangesAsync();
            }
        }
    }
}