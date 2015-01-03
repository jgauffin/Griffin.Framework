using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GriffinFrameworkWeb.Startup))]
namespace GriffinFrameworkWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
