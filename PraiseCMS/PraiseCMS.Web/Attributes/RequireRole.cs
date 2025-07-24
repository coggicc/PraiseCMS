using PraiseCMS.DataAccess.Session;
using System.Web.Mvc;

namespace PraiseCMS.Web.Attributes
{
    public class RequireRoleAttribute : ActionFilterAttribute
    {
        public string Role { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionVariables.CurrentUser == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                //TODO Undo this once MemberOf has been added.
                if (!SessionVariables.CurrentUser.MemberOf(Role))
                {
                    filterContext.Result = new RedirectResult("~/Error/NoAccess");
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}