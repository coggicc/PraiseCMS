using System.Web.Mvc;

namespace PraiseCMS.Web
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
#if !DEBUG
                filters.Add(new RequireHttpsAttribute()); // ADD THIS LINE
#endif
        }
    }
}