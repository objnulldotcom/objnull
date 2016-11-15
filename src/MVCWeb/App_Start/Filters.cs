using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using MVCWeb.Redis.Base;
using MVCWeb.Redis.Models;

namespace MVCWeb
{
    public static class Filters
    {
        public static void RegisterFilters(GlobalFilterCollection filterCollection)
        {
            //添加处理属性到全局过滤器实现全局处理
            filterCollection.Add(new HandleAuthorize());
            filterCollection.Add(new HandleError());
        }
    }

    //全局错误处理属性
    public class HandleError : HandleErrorAttribute
    {
        ILog Log = DependencyResolver.Current.GetService<ILog>();

        public override void OnException(ExceptionContext filterContext)
        {
            //异常处理 日志记录 Logger
#if Release
            Log.Error("-------------------------------------------------------------");
            Log.Error("Exception: " + filterContext.Exception.Message);
            Log.Error("StackTrace: " + filterContext.Exception.StackTrace);
            Exception innerE = filterContext.Exception.InnerException;
            if (innerE != null)
            {
                Log.Error("InnerException: " + innerE.Message);
                Log.Error("InnerStackTrace: " + innerE.StackTrace);
            }
#endif
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.RedirectToRoute(new { controller = "Demo", action = "Error" });
        }
    }

    //授权处理属性
    public class HandleAuthorize : AuthorizeAttribute
    {
         IMyRedisDB MyRedisDB = DependencyResolver.Current.GetService<IMyRedisDB>();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //CurrentUser
            string loginType = filterContext.HttpContext.ReadCookie("LoginType");
            if (loginType == "github")
            {
                Guid id = Guid.Parse(filterContext.HttpContext.ReadCookie("UID"));
                string name = filterContext.HttpContext.ReadCookie("UName");
                string avatar = filterContext.HttpContext.ReadCookie("UAvatar");
                string login = filterContext.HttpContext.ReadCookie("GLogin");
                string token = filterContext.HttpContext.ReadCookie("GToken");
                filterContext.HttpContext.User = new CurrentUser() { ID = id, Name = name, AvatarUrl = avatar, LoginType = loginType, GitHubLogin = login, GitHubAccessToken = token };
            }

            //权限控制
            string controller = filterContext.HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string action = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
            ActionRule rule = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules).Where(r => r.Controller.ToLower() == controller.ToLower() && r.Action.ToLower() == action.ToLower()).FirstOrDefault();
            if(rule != null)
            {
                //CurrentManager
                string id = filterContext.HttpContext.ReadEncodeCookie("MID");
                if (id == "")
                {
                    filterContext.HttpContext.Response.RedirectToRoute(new { controller = "Demo", action = "Error" });
                    return;
                }
                Manager manager = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers).Where(m => m.ID == Guid.Parse(id)).FirstOrDefault();
                if (!rule.AllowRoles.Contains(manager.Role))
                {
                    filterContext.HttpContext.Response.RedirectToRoute(new { controller = "Demo", action = "Error" });
                    return;
                }
                filterContext.HttpContext.User = new CurrentManager() { ID = Guid.Parse(id), key = manager.Key, Role = manager.Role };
            }
        }
    }
}