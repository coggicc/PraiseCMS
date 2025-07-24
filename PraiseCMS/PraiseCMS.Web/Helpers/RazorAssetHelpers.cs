using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PraiseCMS.Web.Helpers
{
    public static class RazorAssetHelpers
    {
        public static string ResolveDefaultProfileImageUrl(UrlHelper url)
        {
            return url.Content(Constants.DefaultProfileImage);
        }
    }
}