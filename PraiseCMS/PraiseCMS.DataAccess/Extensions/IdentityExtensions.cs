using System.Security.Claims;
using System.Security.Principal;

namespace PraiseCMS.DataAccess.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetFirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            return claim != null ? claim.Value : string.Empty;
        }

        public static string GetLastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastName");
            return claim != null ? claim.Value : string.Empty;
        }

        public static string GetPhoneVerificationCode(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("PhoneVerificationCode");
            return claim != null ? claim.Value : string.Empty;
        }
    }
}