using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Web;

namespace PraiseCMS.Web.Helpers
{
    public static class LayoutHelper
    {
        public static void CheckAndSetSubscriptions(HttpContextBase context)
        {
            if (SessionVariables.CurrentChurch != null && Convert.ToDateTime(context.Session["SubscriptionsTimeout"]) < DateTime.Now)
            {
                SessionVariables.SetSubscriptions(SessionVariables.CurrentChurch.Id);
            }
        }

        public static string GetThemeColor()
        {
            return SessionVariables.CurrentUser?.Settings.DarkModeEnabled == true ? "dark.css" : "light.css";
        }

        public static List<Modules> GetModules()
        {
            return SessionVariables.CurrentUser?.Modules ?? new List<Modules>();
        }

        public static string GetContainerSize()
        {
            return SessionVariables.CurrentUser?.Settings.FullWidthView == true ? "container-fluid" : "container";
        }

        public static bool HasSidebarAccess()
        {
            var sidebarModule = GetModules().Find(x => x.Name.Equals("Sidebar"));
            if (SessionVariables.CurrentUser != null && sidebarModule != null)
            {
                return SessionVariables.CurrentUser.IsSuperAdmin ||
                       (!SessionVariables.CurrentUser.IsDonorOnly &&
                        Utilities.GetAccessLevel(sidebarModule.Id, SessionVariables.CurrentUser.AllPermissions) == Operations.ReadWrite);
            }
            return false;
        }

        public static string GetSidebarClasses()
        {
            var hasSidebarAccess = HasSidebarAccess();
            if (hasSidebarAccess)
            {
                return "aside-enabled" + (SessionVariables.CurrentUser.Settings.SidebarCollapsed ? " aside-minimize" : " aside-fixed");
            }
            return "";
        }
    }
}