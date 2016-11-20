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
        public IBlogCommentDataSvc BlogCommentDataSvc { get; set; }
        public IBlogCommentReplyDataSvc BlogCommentReplyDataSvc { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        #region 姿势blog

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
            ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => true, it => it.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            return View();
        }

        public ActionResult BlogView(Guid id)
        {
            ViewBag.Blog = BlogDataSvc.GetByID(id);
            ViewBag.Login = CurrentUser != null;
            return View();
        }
        
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBlogComment(Guid blogID, string mdTxt, string mdValue)
        {
            BlogComment comment = new BlogComment();
            comment.BlogID = blogID;
            comment.MDText = mdTxt;
            comment.MDValue = mdValue;
            comment.OwnerID = CurrentUser.ID;
            BlogCommentDataSvc.Add(comment);
            Blog blog = BlogDataSvc.GetByID(blogID);
            blog.CommentCount += 1;
            BlogDataSvc.Update(blog);
            return Json(new { msg = "done" });
        }

        [HttpPost]
        public ActionResult BlogCommentPage(Guid blogID, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogCommentList = BlogCommentDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogID == blogID, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            return View();
        }

        [HttpPost]
        public ActionResult AddBlogCommentReply(Guid commentID, Guid toUserID, string txt)
        {
            BlogCommentReply reply = new BlogCommentReply();
            reply.BlogCommentID = commentID;
            reply.Content = txt;
            reply.ToUserID = toUserID;
            reply.OwnerID = CurrentUser.ID;
            BlogCommentReplyDataSvc.Add(reply);
            BlogComment comment = BlogCommentDataSvc.GetByID(commentID);
            comment.ReplyCount += 1;
            BlogCommentDataSvc.Update(comment);
            return Json(new { msg = "done" });
        }

        [HttpPost]
        public ActionResult BlogCommentReplyPage(Guid commentID, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogCommentReplyList = BlogCommentReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogCommentID == commentID, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CommentID = commentID;
            return View();
        }

        #endregion

        public ActionResult Error()
        {
            return View();
        }
    }
}