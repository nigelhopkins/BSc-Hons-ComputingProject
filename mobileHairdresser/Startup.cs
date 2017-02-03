using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(mobileHairdresser.Startup))]
namespace mobileHairdresser
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
