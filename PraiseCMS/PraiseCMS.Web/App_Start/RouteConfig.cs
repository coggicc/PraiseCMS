using PraiseCMS.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace PraiseCMS.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Register the default route first
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "PraiseCMS.Web.Controllers" } // Explicitly use Web controllers
            );

            // Register the custom route handler for all routes
            routes.Add(new Route("{controller}/{action}/{id}", new LowercaseRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { id = UrlParameter.Optional })
            });
        }
    }
}