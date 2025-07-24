using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult GenericError(HandleErrorInfo exception)
        {
            Response.ContentType = "text/html";
            ViewBag.Stylesheet = "error-6.css";

            var model = CreateErrorViewModel(exception);
            LogError("Generic Error", model);

            return View(model);
        }

        public ActionResult NotFound(HandleErrorInfo exception)
        {
            Response.ContentType = "text/html";
            ViewBag.Stylesheet = "error-3.css";

            var model = CreateErrorViewModel(exception);

            //Only log errors if the user is logged in. This prevents spam bot errors logged.
            if (SessionVariables.CurrentUser != null)
            {
                LogError("Not Found", model);
            }

            return View(model);
        }

        public ActionResult NoAccess(string accessDeniedMessage = null)
        {
            var model = CreateErrorViewModel(null);
            model.AccessDeniedMessage = accessDeniedMessage.IsNotNullOrEmpty() ? accessDeniedMessage : string.Empty;
            LogError("No Access");

            return View(model);
        }

        public ActionResult _NoAccess(string accessDeniedMessage)
        {
            var model = new ErrorViewModel
            {
                AccessDeniedMessage = accessDeniedMessage
            };
            LogError("No Access");

            return PartialView(model);
        }

        private ErrorViewModel CreateErrorViewModel(HandleErrorInfo exception)
        {
            var model = new ErrorViewModel
            {
                IsDevEnvironment = bool.Parse(ConfigurationManager.AppSettings["IsDevEnvironment"]),
                Exception = exception
            };

            // Show error details if it's a development environment or if the current user is a super admin in production
            model.ShowErrorDetails = model.IsDevEnvironment || (SessionVariables.CurrentUser?.IsSuperAdmin == true);

            return model;
        }

        private void LogError(string errorType, ErrorViewModel model = null)
        {
            var userId = GetUserId();
            var exception = model?.Exception?.Exception;
            var logObj = logRepository.JsonConverter("Exception Message", exception?.Message);

            // Include inner exception and other error details in the logObj
            if (exception != null)
            {
                logObj += $"; Inner Exception: {exception.InnerException?.Message}";
                logObj += $"; Stack Trace: {exception.StackTrace}";
                logObj += $"; Source: {exception.Source}";
                logObj += $"; Target Site: {exception.TargetSite}";

                if (exception.Data?.Count > 0)
                {
                    logObj += "; Custom Data:";
                    foreach (var key in exception.Data.Keys)
                    {
                        logObj += $"; {key}: {exception.Data[key]}";
                    }
                }
            }

            logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, errorType, userId, LogStatuses.Error, logObj);

            // If an exception occurred, send an email with error details
            if (exception != null)
            {
                var emailSubject = $"Praise Error: {errorType}";
                var emailMessage = $"An error occurred in the application:<br>Error Type: {errorType}<br>Exception Message: {exception.Message}<br>";

                // Add additional information such as UserId and RouteHelpers
                emailMessage += $"User ID: {userId}<br>";
                emailMessage += $"Page: {(string.IsNullOrEmpty(RouteHelpers.CurrentAction) || string.IsNullOrEmpty(RouteHelpers.CurrentController) ? "Unknown (No route information available)" : $"{RouteHelpers.CurrentAction}/{RouteHelpers.CurrentController}")}";

                // Add error details from the view model, if available
                emailMessage += "<br>Error Details:<br>";
                emailMessage += $"Error Type: {exception.GetType().FullName ?? "Unknown"}<br>";
                emailMessage += $"Exception Message: {exception.Message}<br>";

                if (exception.InnerException != null)
                {
                    emailMessage += "Inner Exception:<br>";
                    emailMessage += $"Error Type: {exception.InnerException.GetType().FullName ?? "Unknown"}<br>";
                    emailMessage += $"Exception Message: {exception.InnerException.Message}<br>";
                }

                emailMessage += $"Source: {exception.Source ?? "Unknown"}<br>";
                emailMessage += $"Target Site: {(exception.TargetSite != null ? exception.TargetSite.ToString() : "Unknown")}<br>";

                if (exception.Data?.Count > 0)
                {
                    emailMessage += "Custom Data:<br>";
                    emailMessage += "<ul>";
                    foreach (var key in exception.Data.Keys)
                    {
                        emailMessage += $"<li>{key}: {exception.Data[key]}</li>";
                    }
                    emailMessage += "</ul><br>";
                }

                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    emailMessage += "Stack Trace:<br>";
                    emailMessage += $"{exception.StackTrace}";
                }

                // Construct and send the email
                var email = new Email
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = EmailTemplates.General.Replace("{message}", emailMessage),
                    To = "info@praisecms.com",
                    Subject = emailSubject,
                    CreatedDate = DateTime.Now,
                    CreatedBy = Constants.System
                };

                Emailer.SendEmail(email, null, null, null, false, "info@praisecms.com", "Praise CMS");
            }
        }

        private string GetUserId()
        {
            return SessionVariables.CurrentUser?.User?.Id ?? string.Empty;
        }
    }
}