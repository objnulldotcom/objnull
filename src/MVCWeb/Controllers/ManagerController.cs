using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Diagnostics;
using MVCWeb.Redis.Base;
using MVCWeb.Redis.Models;

namespace MVCWeb.Controllers
{
    public class ManagerController : Controller
    {
        public IMyRedisDB MyRedisDB { get; set; }

        #region Login&Out

        public ActionResult MLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MLogin(string key, string value)
        {
            Manager manager = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers).Where(m => m.Key == key && m.Value == value).FirstOrDefault();
            if (manager != null)
            {
                HttpContext.WriteEncodeCookie("MID", manager.ID.ToString());
                return RedirectToAction("Manage");
            }
            else
            {
                return View();
            }
        }

        public ActionResult MLoginOut()
        {
            HttpContext.WriteEncodeCookie("MID", "", DateTime.Now.AddDays(-1));
            return RedirectToAction("Index", "Home");
        }

        #endregion

        //超级主页
        public ActionResult Manage()
        {
            ViewBag.ActionRules = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules).OrderBy(a => a.Controller).ThenBy(a => a.Action);
            ViewBag.Managers = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers);
            return View();
        }

        #region ActionRule

        //一键生成ActionRule
        public ActionResult OnkeyRules()
        {
            IEnumerable<ActionRule> ARules = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules);
            Dictionary<string, List<string>> actionDic = new Dictionary<string, List<string>>();//当前的所有action
            Assembly mvcweb = Assembly.GetExecutingAssembly();
            foreach(Type type in mvcweb.GetTypes())
            {
                if (type.Name.EndsWith("Controller"))
                {
                    string controller = type.Name.Replace("Controller", "");
                    actionDic.Add(controller, new List<string>());

                    int actionType = (int)EnumActionType.前台;
                    if (controller == "Manager")
                    {
                        actionType = (int)EnumActionType.后台;
                    }
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.ReturnType == typeof(ActionResult))
                        {
                            string action = method.Name;
                            actionDic[controller].Add(action);

                            ActionRule rule = ARules.Where(a => a.Controller == controller && a.Action == action).FirstOrDefault();
                            if (rule == null)//没有添加
                            {
                                rule = new ActionRule();
                                rule.Controller = controller;
                                rule.Action = action;
                                rule.ActionType = actionType;
                                MyRedisDB.SetAdd(MyRedisKeys.ActionRules, rule);
                            }
                        }
                    }
                }
            }
            foreach(ActionRule rule in ARules)//多余删除
            {
                if(!actionDic.Keys.Contains(rule.Controller) || !actionDic[rule.Controller].Contains(rule.Action))
                {
                    MyRedisDB.SetRemove(MyRedisKeys.ActionRules, rule);
                }
            }

            return RedirectToAction("Manage");
        }

        //添加或更新ActionRule
        [HttpPost]
        public ActionResult AddActionRule(string controller, string action, int aType, string allowRoles)
        {
            IEnumerable<ActionRule> ARules = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules);
            ActionRule rule = ARules.Where(a => a.Controller == controller && a.Action == action).FirstOrDefault();
            if(rule != null)
            {
                MyRedisDB.SetRemove(MyRedisKeys.ActionRules, rule);
                rule.ActionType = aType;
                if (!string.IsNullOrEmpty(allowRoles))
                {
                    rule.AllowRoles = allowRoles.Split(',').Select(r => int.Parse(r)).ToArray();
                }
                else
                {
                    rule.AllowRoles = null;
                }
            }
            else
            {
                rule = new ActionRule();
                rule.Controller = controller;
                rule.Action = action;
                rule.ActionType = aType;
                if (!string.IsNullOrEmpty(allowRoles))
                {
                    rule.AllowRoles = allowRoles.Split(',').Select(r => int.Parse(r)).ToArray();
                }
            }
            MyRedisDB.SetAdd(MyRedisKeys.ActionRules, rule);
            return RedirectToAction("Manage");
        }

        //删除ActionRule
        public ActionResult DeleteActionRule(Guid id)
        {
            ActionRule rule = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules).Where(r => r.ID == id).FirstOrDefault();
            MyRedisDB.SetRemove(MyRedisKeys.ActionRules, rule);
            return RedirectToAction("Manage");
        }

        #endregion

        #region Manager

        [HttpPost]
        public ActionResult AddManager(string key, string value, int role)
        {
            Manager manager = new Manager();
            manager.Key = key;
            manager.Value = value;
            manager.Role = role;
            MyRedisDB.SetAdd(MyRedisKeys.Managers, manager);
            return RedirectToAction("Manage");
        }

        public ActionResult DeleteManager(Guid id)
        {
            Manager manager = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers).Where(r => r.ID == id).FirstOrDefault();
            MyRedisDB.SetRemove(MyRedisKeys.Managers, manager);
            return RedirectToAction("Manage");
        }

        #endregion
    }
}