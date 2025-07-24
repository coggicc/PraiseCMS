using PraiseCMS.DataAccess.Misc;
using PraiseCMS.Shared.Methods;
using System;
using System.Configuration;

namespace PraiseCMS.DataAccess.Singletons
{
    public class ApplicationCache : SingletonBase<ApplicationCache>
    {
        #region Public Properties
        public string CurrentEnvironment { get; set; }
        public EmailConfiguration EmailConfiguration { get; set; }
        public SmsConfiguration SmsConfiguration { get; set; }
        public AmazonConfiguration AmazonConfiguration { get; set; }
        public SiteConfiguration SiteConfiguration { get; set; }
        public GoogleConfiguration GoogleConfiguration { get; set; }
        public EnvironmentConfiguration EnvironmentConfiguration { get; set; }
        //public MerchantConfiguration MerchantConfiguration { get; set; }

        #endregion

        private ApplicationCache()
        {
            Reload();
        }

        public void Reload()
        {
            var connString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            System.Diagnostics.Debug.WriteLine("Current Connection String: " + connString);

            CurrentEnvironment = ConfigurationManager.AppSettings["Environment.Name"] ?? "DEV";

            EnvironmentConfiguration = new EnvironmentConfiguration
            {
                Name = CurrentEnvironment,
                Url = ConfigurationManager.AppSettings["Environment.Url"]
            };

            SiteConfiguration = new SiteConfiguration
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ToString(),
                Name = ConfigurationManager.AppSettings["SiteName"],
                Title = ConfigurationManager.AppSettings["SiteTitle"],
                Url = EnvironmentConfiguration.Url,
                Keywords = ConfigurationManager.AppSettings["SiteKeywords"],
                Description = ConfigurationManager.AppSettings["SiteDescription"],
                ReCaptchaSiteKey = ConfigurationManager.AppSettings["SiteReCaptchaSiteKey"],
                ReCaptchaSecretKey = ConfigurationManager.AppSettings["SiteReCaptchaSecretKey"],
                TwoFactorAuth = Convert.ToBoolean(ConfigurationManager.AppSettings["SiteTwoFactorAuth"])
            };

            EmailConfiguration = new EmailConfiguration
            {
                Display = ConfigurationManager.AppSettings["EmailDisplay"],
                ReplyTo = ConfigurationManager.AppSettings["EmailReplyTo"],
                Username = ConfigurationManager.AppSettings["EmailUsername"],
                Password = ConfigurationManager.AppSettings["EmailPassword"],
                Smtp = ConfigurationManager.AppSettings["EmailSmtpServer"],
                Port = ConfigurationManager.AppSettings["EmailPort"],
                EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EmailEnableSsl"])
            };

            SmsConfiguration = new SmsConfiguration
            {
                AccountSid = ConfigurationManager.AppSettings["SmsAccountSid"],
                AuthToken = ConfigurationManager.AppSettings["SmsAuthToken"],
                FromNumber = ConfigurationManager.AppSettings["SmsFromNumber"]
            };

            AmazonConfiguration = new AmazonConfiguration
            {
                BucketName = ConfigurationManager.AppSettings["AwsBucketName"],
                AccessKey = ConfigurationManager.AppSettings["AwsAccessKey"],
                SecretKey = ConfigurationManager.AppSettings["AwsSecretKey"],
                ThumbDirectory = ConfigurationManager.AppSettings["AwsThumbDirectory"],
                IconDirectory = ConfigurationManager.AppSettings["AwsIconDirectory"],
                PathTemplate = ConfigurationManager.AppSettings["AwsPathTemplate"],
                Endpoint = ConfigurationManager.AppSettings["AwsEndpoint"]
            };

            GoogleConfiguration = new GoogleConfiguration
            {
                MapsApiKey = ConfigurationManager.AppSettings["GoogleMapsApiKey"]
            };

            //MerchantConfiguration = new MerchantConfiguration
            //{
            //    AgentCode = ConfigurationManager.AppSettings["Merchant.AgentCode"],
            //    AppKey = ConfigurationManager.AppSettings["Merchant.AppKey"],
            //    PartnerCode = ConfigurationManager.AppSettings["Merchant.PartnerCode"],
            //    Password = ConfigurationManager.AppSettings["Merchant.Password"],
            //    PaymentUrl = ConfigurationManager.AppSettings["Merchant.PaymentUrl"],
            //    Url = ConfigurationManager.AppSettings["Merchant.Url"],
            //    Username = ConfigurationManager.AppSettings["Merchant.Username"]
            //};
        }
    }

    #region Configuration Classes
    public class MerchantConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PartnerCode { get; set; }
        public string AgentCode { get; set; }
        public string Url { get; set; }
        public string PaymentUrl { get; set; }
        public string AppKey { get; set; }
    }

    public class EmailConfiguration
    {
        public string Display { get; set; }
        public string ReplyTo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Smtp { get; set; }
        public string Port { get; set; }
        public bool EnableSsl { get; set; }
    }

    public class SmsConfiguration
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string FromNumber { get; set; }
    }

    public class AmazonConfiguration
    {
        public string BucketName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Endpoint { get; set; }
        public string ThumbDirectory { get; set; }
        public string IconDirectory { get; set; }
        public string PathTemplate { get; set; }
    }

    public class SiteConfiguration
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public string ConnectionString { get; set; }
        public bool TwoFactorAuth { get; set; }
        public bool CmsHomePage { get; set; }
        public string ReCaptchaSiteKey { get; set; }
        public string ReCaptchaSecretKey { get; set; }
    }

    public class GoogleConfiguration
    {
        public string MapsApiKey { get; set; }
    }

    public class EnvironmentConfiguration
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    #endregion
}