using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PraiseCMS.Web.Startup))]
namespace PraiseCMS.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}