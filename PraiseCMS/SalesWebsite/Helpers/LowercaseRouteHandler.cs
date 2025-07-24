using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SalesWebsite.Helpers
{
    public class LowercaseRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            // Convert the URL to lowercase
            var request = requestContext.HttpContext.Request;
            var url = request.Url.AbsolutePath;
            var lowerCaseUrl = url.ToLowerInvariant();

            // If the URL has changed, redirect to the lowercase version
            if (url != lowerCaseUrl)
            {
                var redirectUrl = lowerCaseUrl + (request.Url.Query ?? string.Empty);
                requestContext.HttpContext.Response.Redirect(redirectUrl, true);
                return null;
            }

            return base.GetHttpHandler(requestContext);
        }
    }
}