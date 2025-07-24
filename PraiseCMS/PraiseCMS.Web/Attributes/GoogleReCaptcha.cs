using Newtonsoft.Json;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Attributes
{
    public class ValidateGoogleCaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Utilities.HasCookies("recapcha"))
            {
                const string urlToPost = "https://www.google.com/recaptcha/api/siteverify";
                string secretKey = ApplicationCache.Instance.SiteConfiguration.ReCaptchaSecretKey;
                var captchaResponse = filterContext.HttpContext.Request.Form["g-recaptcha-response"];
                var validate = Convert.ToBoolean(filterContext.HttpContext.Request.Form["ValidateCaptcha"]);

                if (validate.IsNotNullOrEmpty() && validate)
                {
                    if (string.IsNullOrWhiteSpace(captchaResponse))
                    {
                        AddErrorAndRedirectToGetAction(filterContext);
                    }

                    var validateResult = ValidateFromGoogle(urlToPost, secretKey, captchaResponse);

                    if (!validateResult.Success)
                    {
                        AddErrorAndRedirectToGetAction(filterContext);
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }

        private static void AddErrorAndRedirectToGetAction(ActionExecutingContext filterContext)
        {
            filterContext.Controller.TempData["AlertMessage"] = "Invalid Captcha! Please try again.";
            filterContext.Controller.TempData["AlertMessageType"] = "alert-light-danger";
            filterContext.Controller.TempData["AlertMessageIcon"] = "fas fa-exclamation-triangle";
            filterContext.Result = new RedirectToRouteResult(filterContext.RouteData.Values);
        }

        private static ReCaptchaResponse ValidateFromGoogle(string urlToPost, string secretKey, string captchaResponse)
        {
            var postData = "secret=" + secretKey + "&response=" + captchaResponse;

            var request = (HttpWebRequest)WebRequest.Create(urlToPost);
            request.Method = "POST";
            request.ContentLength = postData.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(postData);
            }

            string result;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using var reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<ReCaptchaResponse>(result);
        }
    }

    internal class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public string ValidatedDateTime { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}