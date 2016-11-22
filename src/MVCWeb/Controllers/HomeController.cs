using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCWeb.Model.Models;
using MVCWeb.DataSvc.Svc;
using MVCWeb.Redis.Base;
using MVCWeb.Redis.Models;

namespace MVCWeb.Controllers
{
    public class HomeController : BaseController
    {
        public INullUserDataSvc NullUserDataSvc { get; set; }
        public IBlogDataSvc BlogDataSvc { get; set; }
        public IBlogCommentDataSvc BlogCommentDataSvc { get; set; }
        public IBlogCommentReplyDataSvc BlogCommentReplyDataSvc { get; set; }
        public IMyRedisDB MyRedisDB { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserProfile(string id = null)
        {
            Guid userID = id == null ? CurrentUser.ID : Guid.Parse(id);
            NullUser user = NullUserDataSvc.GetByID(userID);
            ViewBag.GitHubUser = GitHub.GetGitHubUserByName(user.GitHubLogin);
            ViewBag.Token = CurrentUser.GitHubAccessToken;
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

            string key = MyRedisKeys.Pre_NewBCMsg + blog.OwnerID;
            NewBCMsg bcmsg = MyRedisDB.GetSet<NewBCMsg>(key).Where(m => m.BlogID == blogID).FirstOrDefault();
            if(bcmsg != null)
            {
                MyRedisDB.SetRemove(key, bcmsg);
                bcmsg.Count += 1;
                MyRedisDB.SetAdd(key, bcmsg);
            }
            else
            {
                bcmsg = new NewBCMsg();
                bcmsg.BlogID = blogID;
                bcmsg.Count = 1;
                bcmsg.Date = DateTime.Now;
                bcmsg.Page = blog.CommentCount % 10 + 1;
                bcmsg.Title = blog.Title.Substring(0, 20);
                MyRedisDB.SetAdd(key, bcmsg);
            }

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

        public string GetMsgCount()
        {
            return (MyRedisDB.RedisDB.SetLength(MyRedisKeys.Pre_NewBCMsg + CurrentUser.ID) + MyRedisDB.RedisDB.SetLength(MyRedisKeys.Pre_NewBCRMsg + CurrentUser.ID)).ToString();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}