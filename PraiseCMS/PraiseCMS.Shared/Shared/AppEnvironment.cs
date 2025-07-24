using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraiseCMS.Shared.Shared
{
    public static class AppEnvironment
    {
        public static string Name => ConfigurationManager.AppSettings["Environment.Name"] ?? "DEV";
        public static bool IsDev => Name.Equals("DEV", StringComparison.OrdinalIgnoreCase);
    }
}