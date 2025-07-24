using PraiseCMS.DataAccess.Session;
using System.Web;
using System.Web.Mvc;

namespace PraiseCMS.Web.Attributes
{
    public class RequireUserAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return SessionVariables.CurrentUser != null;
        }
    }
}