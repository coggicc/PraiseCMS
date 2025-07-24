using System.Globalization;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace PraiseCMS.API
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Set the default culture to use MM/DD/YYYY format
            CultureInfo customCulture = new CultureInfo("en-US"); // English (United States)
            CultureInfo.DefaultThreadCurrentCulture = customCulture;
            CultureInfo.DefaultThreadCurrentUICulture = customCulture;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}