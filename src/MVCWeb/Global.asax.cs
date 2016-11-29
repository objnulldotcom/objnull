﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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
            //设置加解密向量和秘钥
            Utils.RijndaelIV = "ObjectIsNull@001";
            Utils.RijndaelKey = "kowfswiefx*@&JX13d9:fawxf3j34x%1";
        }
    }
}
