using PraiseCMS.BusinessLayer;
using PraiseCMS.DataAccess.Repository;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PraiseCMS.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                // Set the default culture to use MM/DD/YYYY format
                CultureInfo customCulture = new CultureInfo("en-US"); // English (United States)
                CultureInfo.DefaultThreadCurrentCulture = customCulture;
                CultureInfo.DefaultThreadCurrentUICulture = customCulture;

                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);

                // Load blacklisted IPs in the cache when the app starts
                Task.Run(() => BlackListedIPAttribute.LoadBlacklistedIPs());

                // Start background fund sync service
                StartFundsSyncService();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loaderMessages = string.Join(Environment.NewLine, ex.LoaderExceptions.Select(e => e.Message));
                System.IO.File.AppendAllText(@"C:\Temp\LoaderErrors.txt", loaderMessages);
                throw; // re-throw so you see the original error
            }
        }

        private static Timer _timer = new Timer();

        public static void StartFundsSyncService()
        {
            if (!GlobalVariable.IsRunning)
            {
                _timer.Interval = 1000 * 60 * 30; // 30 minutes
                _timer.Elapsed += (sender, e) => CloseExpiredFunds();
                _timer.AutoReset = true;
                _timer.Enabled = true;
                GlobalVariable.IsRunning = true;
            }
        }

        public static void CloseExpiredFunds()
        {
            try
            {
                using var work = new Work();
                work.Fund.CloseExpiredFunds();
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
        }

        //Remove server headers (for security and SEO)
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
        }

        //Remove the www (for SEO and duplicate content)
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var regex = new Regex("(http|https)://www\\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var application = sender as HttpApplication;
            var url = application.Context.Request.Url;
            var www = regex.IsMatch(url.ToString());

            if (www)
            {
                var newUrl = regex.Replace(url.ToString(), $"{url.Scheme}://");
                application.Context.Response.RedirectPermanent(newUrl);
            }

            //This checks the load balancer and forces an HTTPS request
            var loadBalancerReceivedSslRequest = string.Equals(Request.Headers["X-Forwarded-Proto"], "https");
            var serverReceivedSslRequest = Request.IsSecureConnection;

            if (!loadBalancerReceivedSslRequest && !serverReceivedSslRequest)
            {
                var uri = new UriBuilder(Context.Request.Url);

                if (!uri.Host.Equals("localhost") && uri.Host.ContainsIgnoreCase("praisecms"))
                {
                    uri.Port = 443;
                    uri.Scheme = "https";
                    Response.Redirect(uri.ToString());
                }
            }

            // === Added Bot Detection ===
            var userAgent = Request.UserAgent;

            // List of blocked User-Agent substrings
            var blockedUserAgents = new[] { "curl", "wget", "bot", "spider", "crawl", "python-requests", "scrapy" };

            if (string.IsNullOrEmpty(userAgent) || blockedUserAgents.Any(ua => userAgent.IndexOf(ua, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var logRepository = new LogsRepository();

                // Create a JSON object with details of the bot request
                var logObj = logRepository.JsonConverter(
                    "IP Address", Request.UserHostAddress,
                    "User-Agent", userAgent,
                    "URL", Request.Url.ToString(),
                    "Timestamp", DateTime.Now.ToString()
                );

                // Log the bot detection event
                logRepository.LogData(
                    "Bot Detection",
                    "Application_BeginRequest",
                    "Security",
                    null,
                    LogStatuses.Error,
                    logObj,
                    hasSession: false
                );

                // Respond with 403 Forbidden
                Response.StatusCode = 403; // Forbidden
                Response.StatusDescription = "Access Denied";
                Response.End();
            }

            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;

            // Wrap HttpRequest in HttpRequestWrapper to convert it to HttpRequestBase
            string ip = Utilities.GetClientIp(new HttpRequestWrapper(request));

            // Check if the IP is blacklisted (cached version)
            if (BlackListedIPAttribute.IsBlacklisted(ip))
            {
                response.StatusCode = 403; // Forbidden
                response.End(); // Stop further processing
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;
            var currentController = " ";
            var currentAction = " ";
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !string.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (currentRouteData.Values["action"] != null && !string.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            var ex = Server.GetLastError();
            var routeData = new RouteData();
            var action = "GenericError";

            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "NotFound";
                        break;

                        // others if any
                }
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;
            routeData.Values["exception"] = new HandleErrorInfo(ex, currentController, currentAction);

            IController errorManagerController = new Controllers.ErrorController();
            var wrapper = new HttpContextWrapper(httpContext);
            var rc = new RequestContext(wrapper, routeData);
            errorManagerController.Execute(rc);
        }
    }
}