using PraiseCMS.Shared.Methods;
using System.Web;

namespace PraiseCMS.Web.Helpers
{
    public static class RouteHelpers
    {
        public static string CurrentController => (string)HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];

        public static string CurrentAction => (string)HttpContext.Current.Request.RequestContext.RouteData.Values["action"];

        public static string CurrentArea => (string)HttpContext.Current.Request.RequestContext.RouteData.DataTokens["area"];

        public static bool Creating => CurrentAction.ContainsIgnoreCase("create");
    }
}