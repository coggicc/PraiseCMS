using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace PraiseCMS.DataAccess.Session
{
    public static class SessionVariables
    {
        private static void ClearCookies()
        {
            var authenticationCookie = HttpContext.Current.Request.Cookies[CookieNames.Authentication];
            if (authenticationCookie?.Values.Count > 0)
            {
                authenticationCookie.Values[CookieValues.UserId] = string.Empty;
                authenticationCookie.Values[CookieValues.Email] = string.Empty;
                HttpContext.Current.Response.Cookies.Set(authenticationCookie);
            }
        }

        public static void Clear()
        {
            ClearCookies();
            HttpContext.Current.Session.Clear();
            CurrentUser = null;
            CurrentChurch = null;
            CurrentCampus = null;
            Subscriptions = null;
            PlanPermissions = null;
            AllPermissions = null;
            Roles = null;
            CurrentDomain = null;
            CurrentMerchant = null;
            Widgets = null;
        }

        public static CurrentUser CurrentUser
        {
            get
            {
                if (HttpContext.Current?.Session != null)
                {
                    var currentUser = (CurrentUser)HttpContext.Current.Session[SessionVariableNames.CurrentUser];

                    if (currentUser == null)
                    {
                        if (HttpContext.Current.Request.Cookies[CookieNames.Authentication]?.Values.Count > 0 && HttpContext.Current.Request.Cookies[CookieNames.Authentication].Values[CookieValues.UserId] != null)
                        {
                            var userId = HttpContext.Current.Request.Cookies[CookieNames.Authentication].Values[CookieValues.UserId];

                            if (!string.IsNullOrEmpty(userId))
                            {
                                var db = new ApplicationDbContext();
                                var user = db.Users.Find(userId);

                                if (user != null)
                                {
                                    var userSettings = db.UserSettings.FirstOrDefault(x => x.UserId == user.Id);

                                    if (userSettings != null)
                                    {
                                        SetCurrentUser(user, Roles, AllPermissions, Widgets, userSettings.PrimaryChurchId, userSettings.PrimaryChurchCampusId, ReportCategories);
                                        return (CurrentUser)HttpContext.Current.Session[SessionVariableNames.CurrentUser];
                                    }

                                    SetCurrentUser(user, Roles, AllPermissions, Widgets, null, null, ReportCategories);
                                    return (CurrentUser)HttpContext.Current.Session[SessionVariableNames.CurrentUser];
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentUser.AllPermissions.IsNull() || currentUser.AllPermissions.Count == 0)
                        {
                            currentUser.AllPermissions = AllPermissions;
                        }
                    }

                    return currentUser;
                }

                return new CurrentUser(CurrentUser.User, Roles, AllPermissions, Widgets);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                HttpContext.Current.Session[SessionVariableNames.CurrentUser] = value;
                HttpContext.Current.Session[SessionVariableNames.Roles] = value.Roles;
                HttpContext.Current.Session[SessionVariableNames.Modules] = value.Modules;
                HttpContext.Current.Session[SessionVariableNames.AllPermissions] = value.AllPermissions;
            }
        }

        public static Church CurrentChurch
        {
            get
            {
                if (HttpContext.Current.IsNotNull())
                {
                    if (HttpContext.Current.Session[SessionVariableNames.CurrentChurch] == null)
                    {
                        if (CurrentUser.IsNotNull() && !string.IsNullOrEmpty(CurrentUser.User?.Id))
                        {
                            var db = new ApplicationDbContext();
                            var userSetting = db.UserSettings.FirstOrDefault(x => x.UserId == CurrentUser.User.Id);
                            if (userSetting != null && !string.IsNullOrEmpty(userSetting.PrimaryChurchId))
                            {
                                HttpContext.Current.Session[SessionVariableNames.CurrentChurch] = db.Churches.FirstOrDefault(x => x.Id == userSetting.PrimaryChurchId);
                            }
                        }
                    }

                    return (Church)HttpContext.Current.Session[SessionVariableNames.CurrentChurch] ?? new Church();
                }

                return new Church();
            }
            set => HttpContext.Current.Session[SessionVariableNames.CurrentChurch] = value;
        }

        public static Campus CurrentCampus
        {
            get => HttpContext.Current != null ? (Campus)HttpContext.Current.Session[SessionVariableNames.CurrentCampus] : null;
            set => HttpContext.Current.Session[SessionVariableNames.CurrentCampus] = value;
        }

        public static List<Campus> Campuses
        {
            get => HttpContext.Current != null ? (List<Campus>)HttpContext.Current.Session[SessionVariableNames.Campuses] : null;
            set
            {
                if (HttpContext.Current.IsNotNullOrEmpty())
                {
                    HttpContext.Current.Session[SessionVariableNames.Campuses] = value;
                    HttpContext.Current.Session["CampusesTimeout"] = DateTime.Now.AddHours(1);
                }
            }
        }

        public static Domain CurrentDomain
        {
            get => HttpContext.Current != null ? (Domain)HttpContext.Current.Session[SessionVariableNames.CurrentDomain] : null;
            set => HttpContext.Current.Session[SessionVariableNames.CurrentDomain] = value;
        }

        public static ChurchMerchantAccount CurrentMerchant
        {
            get => HttpContext.Current != null ? (ChurchMerchantAccount)HttpContext.Current.Session[SessionVariableNames.CurrentMerchant] : null;
            set => HttpContext.Current.Session[SessionVariableNames.CurrentMerchant] = value;
        }

        public static List<Subscription> Subscriptions
        {
            get => HttpContext.Current != null ? (List<Subscription>)HttpContext.Current.Session[SessionVariableNames.Subscriptions] : null;
            set
            {
                if (HttpContext.Current.IsNotNullOrEmpty())
                {
                    HttpContext.Current.Session[SessionVariableNames.Subscriptions] = value;
                    HttpContext.Current.Session["SubscriptionsTimeout"] = DateTime.Now.AddHours(1);
                }
            }
        }

        public static List<ReportCategory> ReportCategories
        {
            get => HttpContext.Current != null ? (List<ReportCategory>)HttpContext.Current.Session[SessionVariableNames.ReportCategories] : null;
            set => HttpContext.Current.Session[SessionVariableNames.ReportCategories] = value;
        }

        public static List<ApplicationRoles> Roles
        {
            get
            {
                if (HttpContext.Current.Session != null)
                {
                    var roles = (List<ApplicationRoles>)HttpContext.Current.Session[SessionVariableNames.Roles];
                    if (roles == null)
                    {
                        try
                        {
                            using (var adoDataAccess = new AdoDataAccess())
                            {
                                var userId = HttpContext.Current.Request.Cookies[CookieNames.Authentication]?.Values[CookieValues.UserId];
                                roles = adoDataAccess.ReadRolesByUserId(userId);
                            }
                            HttpContext.Current.Session[SessionVariableNames.Roles] = roles;
                        }
                        catch (Exception ex)
                        {
                            ExceptionLogger.LogException(ex);
                            roles = new List<ApplicationRoles>();
                        }
                    }
                    return roles;
                }
                return new List<ApplicationRoles>();
            }
            set => HttpContext.Current.Session[SessionVariableNames.Roles] = value;
        }

        public static List<Permissions> AllPermissions
        {
            get
            {
                if (HttpContext.Current.Session != null)
                {
                    var permissions = (List<Permissions>)HttpContext.Current.Session[SessionVariableNames.AllPermissions];
                    if (permissions == null || permissions.Count == 0)
                    {
                        try
                        {
                            using (var adoDataAccess = new AdoDataAccess())
                            {
                                var userId = HttpContext.Current.Request.Cookies[CookieNames.Authentication]?.Values[CookieValues.UserId];
                                var modules = adoDataAccess.ReadPermittedModulesByRolesAndUserId(Roles, userId);
                                permissions = adoDataAccess.ReadPermissionsByModulesAndUserId(modules, Roles, userId);
                            }
                            HttpContext.Current.Session[SessionVariableNames.AllPermissions] = permissions;
                        }
                        catch (Exception ex)
                        {
                            ExceptionLogger.LogException(ex);
                            permissions = new List<Permissions>();
                        }
                    }
                    return permissions;
                }
                return new List<Permissions>();
            }
            set => HttpContext.Current.Session[SessionVariableNames.AllPermissions] = value;
        }

        public static List<Permissions> PlanPermissions
        {
            get => HttpContext.Current != null ? (List<Permissions>)HttpContext.Current.Session[SessionVariableNames.PlanPermissions] : null;
            set
            {
                if (HttpContext.Current.IsNotNullOrEmpty())
                {
                    HttpContext.Current.Session[SessionVariableNames.PlanPermissions] = value;
                    HttpContext.Current.Session["PlanPermissionsTimeout"] = DateTime.Now.AddHours(1);
                }
            }
        }

        public static List<WidgetSortable> Widgets
        {
            get
            {
                if (HttpContext.Current.Session != null)
                {
                    var widgets = (List<WidgetSortable>)HttpContext.Current.Session[SessionVariableNames.Widgets];
                    if (widgets == null || widgets.Count == 0)
                    {
                        try
                        {
                            var userId = HttpContext.Current.Request.Cookies[CookieNames.Authentication]?.Values[CookieValues.UserId];
                            widgets = Utilities.GetActiveWidgetSortable(userId, Roles.Select(x => x.Id).ToList());
                            HttpContext.Current.Session[SessionVariableNames.Widgets] = widgets;
                        }
                        catch (Exception ex)
                        {
                            ExceptionLogger.LogException(ex);
                            widgets = new List<WidgetSortable>();
                        }
                    }
                    return widgets;
                }
                return new List<WidgetSortable>();
            }
            set => HttpContext.Current.Session[SessionVariableNames.Widgets] = value;
        }

        public static void SetCurrentUser(ApplicationUser user, List<ApplicationRoles> roles, List<Permissions> permissions, List<WidgetSortable> widgets, string primaryChurchId, string primaryCampusId, List<ReportCategory> reportCategories)
        {
            SetCurrentUser(user, roles, permissions, widgets);
            SetCurrentChurch(primaryChurchId);
            SetCurrentCampus(primaryCampusId);
            SetCampuses(primaryChurchId);
            SetSubscriptions(primaryChurchId);
            //SetReportCategories(primaryChurchId);
            SetPermissions(permissions);
            SetRoles(roles);
            SetCurrentMerchant(primaryChurchId);
            SetWidgets(widgets);
            SetAuthCookie(user.Email);
            SetRememberMeCookie(user.Email, user.Id);
        }

        public static void SetCurrentChurch(string churchId)
        {
            CurrentChurch = !string.IsNullOrEmpty(churchId) ? GetChurchById(churchId) : null;
        }

        public static void SetCurrentCampus(string campusId)
        {
            CurrentCampus = !string.IsNullOrEmpty(campusId) ? GetCampusById(campusId) : null;
        }

        public static void SetCampuses(string churchId)
        {
            Campuses = !string.IsNullOrEmpty(churchId) ? GetActiveCampusesByChurchId(churchId) : new List<Campus>();
        }

        public static void SetCurrentDomain(string domainId)
        {
            using (var db = new ApplicationDbContext())
            {
                var domain = !string.IsNullOrEmpty(domainId) ? db.Domains.Find(domainId) : null;
                if (domain != null)
                {
                    db.Entry(domain).State = EntityState.Detached; // Detach the domain entity
                }
                CurrentDomain = domain;
            }
        }

        public static void SetCurrentMerchant(string churchId)
        {
            CurrentMerchant = !string.IsNullOrEmpty(churchId) ? GetPrimaryMerchantAccountByChurchId(churchId) : null;
        }

        public static void SetSubscriptions(string churchId)
        {
            Subscriptions = !string.IsNullOrEmpty(churchId) ? GetSubscriptionsByChurchId(churchId) : new List<Subscription>();
        }

        public static void SetReportCategories(string churchId)
        {
            ReportCategories = !string.IsNullOrEmpty(churchId) ? GetReportCategoriesByChurchId(churchId) : new List<ReportCategory>();
        }

        public static void SetRoles(List<ApplicationRoles> roles)
        {
            Roles = roles;
        }

        public static void SetPermissions(List<Permissions> permissions)
        {
            AllPermissions = permissions;
        }

        public static void SetPlanPermissions()
        {
            using (var db = new ApplicationDbContext())
            {
                var permissions = db.Permissions.Where(q => q.Type.Equals(PermissionType.Subscription.ToString())).ToList();
                // Detach all permission entities
                foreach (var permission in permissions)
                {
                    db.Entry(permission).State = EntityState.Detached;
                }
                PlanPermissions = permissions;
            }
        }

        public static void SetWidgets(List<WidgetSortable> widgets)
        {
            Widgets = widgets;
        }

        public static void SetCurrentUser(ApplicationUser user, List<ApplicationRoles> roles, List<Permissions> permissions, List<WidgetSortable> widgets)
        {
            CurrentUser = new CurrentUser(user, roles, permissions, widgets);
        }

        private static void SetAuthCookie(string email)
        {
            FormsAuthentication.SetAuthCookie(email, true);
        }

        private static void SetRememberMeCookie(string email, string userId)
        {
            if (HttpContext.Current?.Response == null)
            {
                return;
            }

            var cookie = HttpContext.Current.Response.Cookies[CookieNames.Authentication] ?? new HttpCookie(CookieNames.Authentication);

            cookie.Values.Set(CookieValues.RememberMe, true.ToString());
            cookie.Values.Set(CookieValues.Email, email);
            cookie.Values.Set(CookieValues.UserId, userId);

            cookie.Expires = DateTime.Now.AddDays(60);
            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        // Helper methods to fetch data from the database
        private static Church GetChurchById(string churchId)
        {
            using (var db = new ApplicationDbContext())
            {
                var church = db.Churches.Find(churchId);
                if (church != null)
                {
                    db.Entry(church).State = EntityState.Detached; // Detach the church entity
                }
                return church;
            }
        }

        private static Campus GetCampusById(string campusId)
        {
            using (var db = new ApplicationDbContext())
            {
                var campus = db.Campuses.Find(campusId);
                if (campus != null)
                {
                    db.Entry(campus).State = EntityState.Detached; // Detach the campus entity
                }
                return campus;
            }
        }

        private static List<Campus> GetActiveCampusesByChurchId(string churchId)
        {
            using (var db = new ApplicationDbContext())
            {
                // Fetch all active campuses
                var campuses = db.Campuses
                    .Where(x => x.ChurchId == churchId && x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToList();

                // Detach each entity from the context before returning
                foreach (var campus in campuses)
                {
                    db.Entry(campus).State = EntityState.Detached;
                }

                return campuses;
            }
        }

        private static ChurchMerchantAccount GetPrimaryMerchantAccountByChurchId(string churchId)
        {
            using (var db = new ApplicationDbContext())
            {
                var merchantAccount = db.ChurchMerchantAccounts
                    .FirstOrDefault(x => x.ChurchId == churchId && x.IsActive && x.Merchant == MerchantProviders.Nuvei);
                if (merchantAccount != null)
                {
                    db.Entry(merchantAccount).State = EntityState.Detached; // Detach the merchant account entity
                }
                return merchantAccount;
            }
        }

        private static List<Subscription> GetSubscriptionsByChurchId(string churchId)
        {
            using (var db = new ApplicationDbContext())
            {
                var subscriptions = db.Subscription.Where(q => q.ChurchId.Equals(churchId)).ToList();
                // Detach all subscription entities
                foreach (var subscription in subscriptions)
                {
                    db.Entry(subscription).State = EntityState.Detached;
                }
                return subscriptions;
            }
        }

        private static List<ReportCategory> GetReportCategoriesByChurchId(string churchId)
        {
            using (var db = new ApplicationDbContext())
            {
                var reportCategories = db.ReportCategories.Where(q => q.ChurchId.Equals(churchId)).ToList();
                // Detach all report category entities
                foreach (var reportCategory in reportCategories)
                {
                    db.Entry(reportCategory).State = EntityState.Detached;
                }
                return reportCategories;
            }
        }
    }

    public class CurrentUser
    {
        public ApplicationUser User { get; set; }
        public List<ApplicationRoles> Roles { get; set; }
        public List<Modules> Modules { get; set; }
        public List<Permissions> AllPermissions { get; set; }
        public UserSetting Settings { get; set; }
        public List<Campus> Locations { get; set; }
        public List<UserMerchantAccount> UserMerchantAccounts { get; set; }
        public List<WidgetSortable> Widgets { get; set; }

        public CurrentUser(ApplicationUser user, List<ApplicationRoles> roles, List<Permissions> permissions, List<WidgetSortable> widgets)
        {
            User = user;
            Roles = roles;
            AllPermissions = permissions;
            Widgets = widgets;

            using (var db = new ApplicationDbContext())
            {
                var settings = db.UserSettings.FirstOrDefault(x => x.UserId.Equals(user.Id));
                Settings = settings ?? new UserSetting();
                Locations = new List<Campus>();
                Modules = db.Modules.ToList();
                UserMerchantAccounts = db.UserMerchantAccounts.Where(x => x.UserId == user.Id && x.Merchant == MerchantProviders.Nuvei && x.IsActive).ToList();
            }
        }

        public bool MemberOf(string role)
        {
            try
            {
                return SessionVariables.CurrentUser.Roles.Any(x => x.Name.EqualsIgnoreCase(role) || x.Name.EqualsIgnoreCase(PraiseCMS.Shared.Shared.Roles.SuperAdmin));
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }
        }

        public bool MemberOf(params string[] roles)
        {
            try
            {
                return SessionVariables.Roles.Any(x => roles.Contains(x.Name) || x.Name.EqualsIgnoreCase(PraiseCMS.Shared.Shared.Roles.SuperAdmin));
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }
        }

        public bool IsDonorOnly => Utilities.IsDonorOnly(SessionVariables.CurrentUser.Roles);

        public bool IsSuperAdmin => MemberOf(PraiseCMS.Shared.Shared.Roles.SuperAdmin);

        public bool IsAdmin => MemberOf(PraiseCMS.Shared.Shared.Roles.Administrator);
    }
}