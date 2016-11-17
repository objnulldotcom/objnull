using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCWeb.Model.Models;
using MVCWeb.DataSvc.Svc;

namespace MVCWeb.Controllers
{
    public class HomeController : BaseController
    {
        public IBlogDataSvc BlogDataSvc { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewBlog()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewBlog(int type, string title, string mdTxt, string mdValue)
        {
            Blog nblog = new Blog();
            nblog.Type = type;
            nblog.Title = title;
            nblog.MDText = mdTxt;
            nblog.MDValue = mdValue;
            nblog.OwnerID = CurrentUser.ID;
            BlogDataSvc.Add(nblog);
            return Json(new { msg = "done", url = Url.Action("BlogList") });
        }

        public ActionResult BlogList()
        {
            ViewBag.Login = CurrentUser != null;
            return View();
        }
        
        [HttpPost]
        public ActionResult BlogPage(int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, u => true, u => u.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}