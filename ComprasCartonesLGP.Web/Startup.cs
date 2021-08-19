using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ComprasCartonesLGP.Web.Startup))]
namespace ComprasCartonesLGP.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
