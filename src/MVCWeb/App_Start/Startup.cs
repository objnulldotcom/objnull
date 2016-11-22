using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
using MVCWeb.SignalRHubs;

[assembly: OwinStartup(typeof(MVCWeb.Startup))]
namespace MVCWeb
{
    //Owin启动器
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //替换signalR客户端id生成器
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new MyUserIdProvider());
            app.MapSignalR();
        }
    }
}