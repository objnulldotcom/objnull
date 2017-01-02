using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using MVCWeb.Redis.Base;
using MVCWeb.Redis.Models;
using MVCWeb.DataSvc.Svc;
using MVCWeb.Model.Models;

namespace MVCWeb.Controllers
{
    public class ManagerController : Controller
    {
        public IMyRedisDB MyRedisDB { get; set; }
        public INullUserDataSvc NullUserDataSvc { get; set; }
        public IBlogDataSvc BlogDataSvc { get; set; }
        public INewBeeDataSvc NewBeeDataSvc { get; set; }

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
                        actionType = (int)EnumActionType.Manager;
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

        #region 数据管理

        //数据管理
        public ActionResult DataManage()
        {
            return View();
        }
        
        //用户列表
        [HttpPost]
        public ActionResult UserPage(int pageSize, int pageNum = 1, string condition = "")
        {
            int totalCount;
            if(string.IsNullOrEmpty(condition))
            {
                ViewBag.UserList = NullUserDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => true, u => u.InsertDate, true, out totalCount).ToList();
            }
            else
            {
                Guid id = Guid.Empty;
                if(Guid.TryParse(condition, out id))
                {
                    ViewBag.UserList = NullUserDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => u.ID == id, u => u.InsertDate, true, out totalCount).ToList();
                }
                else
                {
                    ViewBag.UserList = NullUserDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => u.GitHubLogin.Contains(condition), u => u.InsertDate, true, out totalCount).ToList();
                }
            }
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.DisabledUsers = MyRedisDB.GetSet<DisabledUser>(MyRedisKeys.DisabledUsers);
            return View();
        }

        //启禁用户
        [HttpPost]
        public ActionResult UserOperate(string type, Guid uid, int objectType, int days = 0)
        {
            if(type == "启")
            {
                DisabledUser du = MyRedisDB.GetSet<DisabledUser>(MyRedisKeys.DisabledUsers).Where(d => d.UserID == uid && d.ObjectType == objectType).FirstOrDefault();
                MyRedisDB.SetRemove(MyRedisKeys.DisabledUsers, du);
                SysMsg msg = new SysMsg();
                msg.Date = DateTime.Now;
                msg.Title = "你的账号已解封";
                msg.Msg = "你的账号在" + Enum.GetName(typeof(EnumObjectType), objectType) + "版块中已解封";
                string key = MyRedisKeys.Pre_SysMsg + uid;
                MyRedisDB.SetAdd(key, msg);

            }
            else
            {
                DisabledUser du = MyRedisDB.GetSet<DisabledUser>(MyRedisKeys.DisabledUsers).Where(d => d.UserID == uid && d.ObjectType == objectType).FirstOrDefault();
                if(du == null)
                {
                    du = new DisabledUser();
                    du.UserID = uid;
                    du.ObjectType = objectType;
                    du.AbleDate = DateTime.Now.AddDays(days);
                    MyRedisDB.SetAdd(MyRedisKeys.DisabledUsers, du);
                }
                else
                {
                    MyRedisDB.SetRemove(MyRedisKeys.DisabledUsers, du);
                    du.AbleDate = DateTime.Now.AddDays(days);
                    MyRedisDB.SetAdd(MyRedisKeys.DisabledUsers, du);
                }

                SysMsg msg = new SysMsg();
                msg.Date = DateTime.Now;
                msg.Title = "你的账号被封禁";
                msg.Msg = "你在" + Enum.GetName(typeof(EnumObjectType), objectType) + "版块被封禁至" + du.AbleDate.ToString("yyyy-MM-dd HH:mm");
                string key = MyRedisKeys.Pre_SysMsg + uid;
                MyRedisDB.SetAdd(key, msg);
            }
            return Json(new { msg = "done" });
        }
        
        //姿势列表
        [HttpPost]
        public ActionResult BlogPage(int pageSize, int pageNum = 1, string condition = "")
        {
            int totalCount;
            if (string.IsNullOrEmpty(condition))
            {
                ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => true, u => u.InsertDate, true, out totalCount).ToList();
            }
            else
            {
                Guid id = Guid.Empty;
                if (Guid.TryParse(condition, out id))
                {
                    ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => u.ID == id, u => u.InsertDate, true, out totalCount).ToList();
                }
                else
                {
                    ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => u.Title.Contains(condition) || u.Owner.GitHubLogin.Contains(condition), u => u.InsertDate, true, out totalCount).ToList();
                }
            }
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //删除姿势
        [HttpPost]
        public ActionResult BlogDelete(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            BlogDataSvc.DeleteByID(id);

            SysMsg msg = new SysMsg();
            msg.Date = DateTime.Now;
            msg.Title = "你的" + Enum.GetName(typeof(EnumObjectType), 1) + "被删除";
            msg.Msg = blog.Title.MaxByteLength(30);
            string key = MyRedisKeys.Pre_SysMsg + blog.OwnerID;
            MyRedisDB.SetAdd(key, msg);

            return Json(new { msg = "done" });
        }

        //编辑姿势
        [HttpPost]
        public ActionResult BlogEidt()
        {
            Guid key = Guid.NewGuid();
            MyRedisDB.RedisDB.StringSet("MBEditKey", key.ToString(), new TimeSpan(0, 0, 10));
            return Json(new { key = key.ToString() });
        }

        //NewBee列表
        [HttpPost]
        public ActionResult NewBeePage(int pageSize, int pageNum = 1, string condition = "")
        {
            int totalCount;
            if (string.IsNullOrEmpty(condition))
            {
                ViewBag.NewBeeList = NewBeeDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => true, u => u.InsertDate, true, out totalCount).ToList();
            }
            else
            {
                Guid id = Guid.Empty;
                if (Guid.TryParse(condition, out id))
                {
                    ViewBag.NewBeeList = NewBeeDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => u.ID == id, u => u.InsertDate, true, out totalCount).ToList();
                }
                else
                {
                    ViewBag.NewBeeList = NewBeeDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => u.Title.Contains(condition) || u.Owner.GitHubLogin.Contains(condition), u => u.InsertDate, true, out totalCount).ToList();
                }
            }
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //删除NewBee
        [HttpPost]
        public ActionResult NewBeeDelete(Guid id)
        {
            NewBee newBee = NewBeeDataSvc.GetByID(id);
            NewBeeDataSvc.DeleteByID(id);

            SysMsg msg = new SysMsg();
            msg.Date = DateTime.Now;
            msg.Title = "你的" + Enum.GetName(typeof(EnumObjectType), 2) + "被删除";
            msg.Msg = newBee.Title.MaxByteLength(30);
            string key = MyRedisKeys.Pre_SysMsg + newBee.OwnerID;
            MyRedisDB.SetAdd(key, msg);

            return Json(new { msg = "done" });
        }

        //置顶NewBee
        [HttpPost]
        public ActionResult NewBeeTop(Guid id)
        {
            NewBee newBee = NewBeeDataSvc.GetByID(id);
            newBee.Top = newBee.Top ? false : true;
            NewBeeDataSvc.Update(newBee);

            return Json(new { msg = "done" });
        }
        #endregion
    }
}