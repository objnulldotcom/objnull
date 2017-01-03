using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using System.IO;

namespace MVCWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //设置DependencyResolver
            DependencyResolver.SetResolver(Autofac.GetDependecyResolver());
            //注册过滤器
            Filters.RegisterFilters(GlobalFilters.Filters);

            string configPath = ConfigurationManager.AppSettings["NewBeeFilePath"].Replace("NewBeeFile\\", "") + "Config.txt";
            string[] configTxt = File.OpenText(configPath).ReadToEnd().Split(new string[]{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Utils.RijndaelIV = configTxt[0];
            Utils.RijndaelKey = configTxt[1];
        }
    }
}
