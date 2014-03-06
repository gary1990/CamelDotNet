using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CamelDotNet.Startup))]
namespace CamelDotNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
