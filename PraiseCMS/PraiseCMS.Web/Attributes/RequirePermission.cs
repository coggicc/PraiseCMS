using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace PraiseCMS.Web.Attributes
{
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        public string ModuleId { get; set; }
        public int RequiredOperation { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionVariables.CurrentUser == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                if (!SessionVariables.CurrentUser.IsSuperAdmin)
                {
                    if (SessionVariables.CurrentUser.AllPermissions == null)
                    {
                        filterContext.Result = new RedirectResult("~/Error/NoAccess");
                    }
                    else
                    {
                        var accessLevel = Utilities.GetAccessLevel(ModuleId, SessionVariables.CurrentUser.AllPermissions);

                        if (accessLevel.Equals(Operations.NoAccess) || accessLevel < RequiredOperation)
                        {
                            filterContext.Result = new RedirectResult("~/Error/NoAccess");
                        }
                    }

                    //module permission by plan
                    if (Convert.ToDateTime(HttpContext.Current.Session["PlanPermissionsTimeout"]) < DateTime.Now || SessionVariables.PlanPermissions.IsNullOrEmpty())
                    {
                        SessionVariables.SetPlanPermissions();
                    }
                    else
                    {
                        var planPermission = Utilities.GetPermissionByPlan(ModuleId, SessionVariables.PlanPermissions);

                        if (planPermission.Equals(Operations.NoAccess))
                        {
                            filterContext.Result = new RedirectResult("~/Error/NoAccess");
                        }
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }

    public class BlackListedIPAttribute : ActionFilterAttribute
    {
        private static readonly string cacheKey = "BlackListedIPs";
        private static readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);

        // Load blacklisted IPs at application start
        public static void LoadBlacklistedIPs()
        {
            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    var blackListedIPs = Utilities.GetBlackListedIPs(dbContext);
                    HttpRuntime.Cache.Insert(cacheKey, blackListedIPs, null, DateTime.UtcNow.Add(cacheDuration), Cache.NoSlidingExpiration);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                HttpRuntime.Cache.Insert(cacheKey, new List<string>(), null, DateTime.UtcNow.Add(cacheDuration), Cache.NoSlidingExpiration);
            }
        }

        // Check if an IP is blacklisted (without querying DB every time)
        public static bool IsBlacklisted(string ip)
        {
            List<string> blackListedIPs = HttpRuntime.Cache[cacheKey] as List<string>;

            if (blackListedIPs == null)
            {
                LoadBlacklistedIPs(); // Reload in case of cache expiration
                blackListedIPs = HttpRuntime.Cache[cacheKey] as List<string>;
            }

            return blackListedIPs.Contains(ip);
        }
    }
}