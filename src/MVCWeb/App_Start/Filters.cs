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
            //当前action有rule
            if (rule != null)
            {
                if(string.IsNullOrEmpty(rule.AllowRoles))//allowrole为空 限制未登录用户
                {
                    if (string.IsNullOrEmpty(loginType))//未登录
                    {
                        filterContext.HttpContext.Response.RedirectToRoute(new { controller = "Home", action = "Error" });
                        return;
                    }
                }
                else//allowrole不为空 修改当前用户为CurrentManager
                {
                    //CurrentManager
                    string mRole = "";
                    string mid = filterContext.HttpContext.ReadEncodeCookie("MID");
                    if (!string.IsNullOrEmpty(mid))
                    {
                        Manager manager = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers).Where(m => m.ID == Guid.Parse(mid)).FirstOrDefault();
                        filterContext.HttpContext.User = new CurrentManager() { ID = Guid.Parse(mid), key = manager.Key, Role = manager.Role };
                        mRole = manager.Role;
                    }

                    if (string.IsNullOrEmpty(mRole) || !rule.AllowRoles.Contains(mRole))
                    {
                        //不是管理员且不是允许角色 不允许访问
                        filterContext.HttpContext.Response.RedirectToRoute(new { controller = "Home", action = "Error" });
                        return;
                    }
                }
            }
        }
    }
}