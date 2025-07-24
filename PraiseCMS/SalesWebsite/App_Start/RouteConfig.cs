using SalesWebsite.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace SalesWebsite
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
                namespaces: new[] { "SalesWebsite.Controllers" } // Explicitly specify the namespace
            );

            // Register the custom route handler for all routes
            routes.Add(new Route("{controller}/{action}/{id}", new LowercaseRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { id = UrlParameter.Optional })
            });
        }
    }
}