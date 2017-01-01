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
            if(filterContext.Exception.Message.Contains("未将对象引用设置到对象的实例"))
            {
                Log.Warn("-------------------------------------------------------------");
                Log.Warn("警告: " + filterContext.Exception.Message);
                Log.Warn("链接: " + filterContext.RouteData.Values["controller"] + "/" + filterContext.RouteData.Values["action"]);
                Log.Warn("IP: " + filterContext.HttpContext.Request.UserHostAddress);
            }
            else if(filterContext.Exception.Message.Contains("参数字典包含一个 null 项"))
            {
                Log.Warn("-------------------------------------------------------------");
                Log.Warn("警告: " + filterContext.Exception.Message);
                Log.Warn("链接: " + filterContext.RouteData.Values["controller"] + "/" + filterContext.RouteData.Values["action"]);
                Log.Warn("IP: " + filterContext.HttpContext.Request.UserHostAddress);
            }
            else
            {
                Log.Error("-------------------------------------------------------------");
                Log.Error("Exception: " + filterContext.Exception.Message);
                Log.Error("链接: " + filterContext.RouteData.Values["controller"] + "/" + filterContext.RouteData.Values["action"]);
                Log.Error("IP: " + filterContext.HttpContext.Request.UserHostAddress);
                Log.Error("StackTrace: " + filterContext.Exception.StackTrace);
                Exception innerE = filterContext.Exception.InnerException;
                if (innerE != null)
                {
                    Log.Error("InnerException: " + innerE.Message);
                    Log.Error("InnerStackTrace: " + innerE.StackTrace);
                }
            }
#endif
            filterContext.ExceptionHandled = true;
            if(filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { msg = "系统错误" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.HttpContext.Response.RedirectToRoute(new { controller = "Home", action = "Error" });
            }
        }
    }

    //授权处理属性
    public class HandleAuthorize : AuthorizeAttribute
    {
         IMyRedisDB MyRedisDB = DependencyResolver.Current.GetService<IMyRedisDB>();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //权限控制
            string controller = filterContext.HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string action = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
            ActionRule rule = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules).Where(r => r.Controller.ToLower() == controller.ToLower() && r.Action.ToLower() == action.ToLower()).FirstOrDefault();
            if(rule == null || rule.ActionType == 0)
            {
                throw new Exception("请求：/" + controller + "/" + action + " 未受控制");
            }
            else if (rule.ActionType == (int)EnumActionType.前台)
            {
                #region 前台
                int role = 0;

                //CurrentUser
                string uid = filterContext.HttpContext.ReadCookie("UID");
                if (!string.IsNullOrEmpty(uid))//已登录
                {
                    try
                    {
                        string loginType = filterContext.HttpContext.ReadCookie("LoginType");
                        string name = filterContext.HttpContext.ReadCookie("UName");
                        string avatar = filterContext.HttpContext.ReadCookie("UAvatar");
                        string login = filterContext.HttpContext.ReadCookie("GLogin");

                        string sKEY = filterContext.HttpContext.ReadCookie("SKEY");
                        if (sKEY != Utils.RijndaelEncrypt(uid))//SKEY检查
                        {
                            filterContext.HttpContext.Response.RedirectToRoute(new { controller = "OAuth", action = "LogOut" });
                            return;
                        }

                        string[] rolecookie = Utils.RijndaelDecrypt(filterContext.HttpContext.ReadCookie("Role")).Split(';');
                        if (rolecookie.Length != 2 || rolecookie[0] != uid)//角色检查
                        {
                            filterContext.HttpContext.Response.RedirectToRoute(new { controller = "OAuth", action = "LogOut" });
                            return;
                        }

                        role = int.Parse(rolecookie[1]);
                        filterContext.HttpContext.User = new CurrentUser() { ID = Guid.Parse(uid), Name = name, AvatarUrl = avatar, LoginType = loginType, GitHubLogin = login, Role = role };
                    }
                    catch
                    {
                        //读取cookie错误时清除cookie
                        filterContext.HttpContext.Response.RedirectToRoute(new { controller = "OAuth", action = "LogOut" });
                    }
                }
                else
                {
                    uid = "null";
                }
                
                //角色控制
                if (rule.AllowRoles != null && rule.AllowRoles.Length > 0 && !rule.AllowRoles.Contains(role))
                {
                    throw new Exception("用户" + uid + "请求：/" + controller + "/" + action + " 没有权限");
                }
                #endregion
            }
            else if (rule.ActionType == (int)EnumActionType.Manager)
            {
                #region 后台
                int mRole = 0;

                //CurrentManager 
                string mid = filterContext.HttpContext.ReadEncodeCookie("MID");
                if (!string.IsNullOrEmpty(mid))//已登录
                {
                    Manager manager = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers).Where(m => m.ID == Guid.Parse(mid)).FirstOrDefault();
                    mRole = manager.Role;
                    filterContext.HttpContext.User = new CurrentManager() { ID = Guid.Parse(mid), key = manager.Key, Role = mRole };
                }
                else
                {
                    mid = "null";
                }

                //角色控制
                if (rule.AllowRoles != null && rule.AllowRoles.Length > 0 && !rule.AllowRoles.Contains(mRole))
                {
                    throw new Exception("用户" + mid + "请求：/" + controller + "/" + action + " 没有权限");
                }
                #endregion
            }
        }
    }
}