using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(MVCWeb.Startup))]
namespace MVCWeb
{
    //Owin启动器
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}