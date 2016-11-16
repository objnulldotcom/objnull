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
        public IRecruitDataSvc RecruitDataSvc { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PostNew(int pt)
        {
            if(pt < 1 || pt > 5)
            {
                pt = 5;
            }
            switch (pt)
            {
                case 1:
                    ViewBag.PostName = "发车";
                    break;
                case 2:
                    ViewBag.PostName = "发车";
                    break;
                case 3:
                    ViewBag.PostName = "发车";
                    break;
                case 4:
                    ViewBag.PostName = "发车";
                    break;
                case 5:
                    ViewBag.PostName = "MyTree";
                    break;
            }
            ViewBag.PostType = pt;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PostNew(int pt, string title, string mdTxt, string mdValue)
        {
            switch (pt)
            {
                case 1:
                    Recruit rec = new Recruit();
                    rec.Title = title;
                    rec.MDText = mdTxt;
                    rec.MDValue = mdValue;
                    rec.OwnerID = CurrentUser.ID;
                    RecruitDataSvc.Add(rec);
                    break;
                case 2:
                    ViewBag.PostName = "发车";
                    break;
                case 3:
                    ViewBag.PostName = "发车";
                    break;
                case 4:
                    ViewBag.PostName = "发车";
                    break;
                case 5:
                    ViewBag.PostName = "MyTree";
                    break;
            }
            return Json(new { msg = "done", url = Url.Action("BCar") });
        }

        public ActionResult BCar()
        {
            return View();
        }


    }
}