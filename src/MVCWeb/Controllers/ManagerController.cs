using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCWeb.Redis.Base;
using MVCWeb.Redis.Models;

namespace MVCWeb.Controllers
{
    public class ManagerController : Controller
    {
        public IMyRedisDB MyRedisDB { get; set; }

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
            return RedirectToAction("Index");
        }

        public ActionResult Manage()
        {
            ViewBag.ActionRules = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules);
            ViewBag.Managers = MyRedisDB.GetSet<Manager>(MyRedisKeys.Managers);
            return View();
        }

        [HttpPost]
        public ActionResult AddActionRule(string controller, string action, string allow)
        {
            ActionRule rule = new ActionRule();
            rule.Controller = controller;
            rule.Action = action;
            rule.AllowRoles = allow;
            MyRedisDB.SetAdd(MyRedisKeys.ActionRules, rule);
            return RedirectToAction("Manage");
        }

        public ActionResult DeleteActionRule(Guid id)
        {
            ActionRule rule = MyRedisDB.GetSet<ActionRule>(MyRedisKeys.ActionRules).Where(r => r.ID == id).FirstOrDefault();
            MyRedisDB.SetRemove(MyRedisKeys.ActionRules, rule);
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public ActionResult AddManager(string key, string value, string role)
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

    }
}