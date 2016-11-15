using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mvc;
using MVCWeb.DataSvc.Svc;
using MVCWeb.Model.DBContext;
using StackExchange.Redis;
using MVCWeb.Redis.Base;
using log4net;
using log4net.Config;

namespace MVCWeb
{
    public static class Autofac
    {
        public static AutofacDependencyResolver GetDependecyResolver()
        {
            ContainerBuilder cb = new ContainerBuilder();

            //注册Controller
            cb.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            //注册DataSvc
            cb.RegisterAssemblyTypes(typeof(NullUserDataSvc).Assembly).AsImplementedInterfaces().PropertiesAutowired().InstancePerRequest();
            //注册DBContext
            cb.Register(d => new MyDBContext()).AsSelf().InstancePerLifetimeScope();

            //注册Redis
            cb.RegisterAssemblyTypes(typeof(MyRedisDB).Assembly).AsImplementedInterfaces().PropertiesAutowired().InstancePerRequest();
            //使用单例注册RedisServer和ReidsDB
            cb.RegisterInstance(MyRedisDBFactory.GetServer()).As<IServer>();
            cb.RegisterInstance(MyRedisDBFactory.GetDB()).As<IDatabase>();

            //注册ILog并配置log4net
            XmlConfigurator.Configure();
            cb.RegisterInstance(LogManager.GetLogger("Logger")).As<ILog>();

            return new AutofacDependencyResolver(cb.Build());
        }
    }
}