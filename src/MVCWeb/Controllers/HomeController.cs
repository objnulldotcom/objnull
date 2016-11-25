using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Newtonsoft.Json;
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

        public int GetByteLength(string val)
        {
            return Encoding.Default.GetByteCount(val);
        }

        //首页
        public ActionResult Index()
        {
            return View();
        }

        //个人主页
        public ActionResult UserProfile(string id = null)
        {
            Guid userID = id == null ? CurrentUser.ID : Guid.Parse(id);
            ViewBag.Owner = userID == CurrentUser.ID;
            NullUser user = NullUserDataSvc.GetByID(userID);
            ViewBag.GitHubUser = GitHub.GetGitHubUserByName(user.GitHubLogin);
            ViewBag.Token = CurrentUser.GitHubAccessToken;
            return View();
        }

        #region 姿势blog

        //新姿势
        public ActionResult NewBlog()
        {
            string key = MyRedisKeys.Pre_BlogDraft + CurrentUser.ID;
            string draftval = MyRedisDB.StringGet(key);
            if(!string.IsNullOrEmpty(draftval))
            {
                Blog draft = JsonConvert.DeserializeObject<Blog>(draftval);
                ViewBag.DraftBlog = draft;
            }
            return View();
        }
        
        //保存草稿
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveDraft(int type, string title, string mdTxt)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(mdTxt))//内容空不保存草稿
            {
                return Json(new { msg = "empty" });
            }
            int tlength = Encoding.Default.GetByteCount(title);
            int txtlength = Encoding.Default.GetByteCount(mdTxt);
            if (tlength > 90 || txtlength > 50000)
            {
                return Json(new { msg = "参数太长" });
            }

            if (type < 0 || type > 4)
            {
                type = 0;
            }
            Blog nblog = new Blog();
            nblog.Type = type;
            nblog.Title = title;
            nblog.MDText = mdTxt;
            string key = MyRedisKeys.Pre_BlogDraft + CurrentUser.ID;
            MyRedisDB.StringSet(key, JsonConvert.SerializeObject(nblog));
            return Json(new { msg = "done", date = DateTime.Now.ToString("HH:mm") });
        }

        //发表新姿势
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewBlog(int type, string title, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }
            int tlength = Encoding.Default.GetByteCount(title);
            int txtlength = Encoding.Default.GetByteCount(mdTxt);
            if (tlength > 90 || txtlength > 50000)
            {
                return Json(new { msg = "参数太长" });
            }

            if (type < 0 || type > 4)
            {
                type = 0;
            }
            Blog nblog = new Blog();
            nblog.Type = type;
            nblog.Title = title;
            nblog.MDText = mdTxt;
            nblog.MDValue = mdValue;
            nblog.OwnerID = CurrentUser.ID;
            BlogDataSvc.Add(nblog);
            //发布成功删除草稿
            string key = MyRedisKeys.Pre_BlogDraft + CurrentUser.ID;
            MyRedisDB.DelKey(key);
            return Json(new { msg = "done", url = Url.Action("BlogList") });
        }

        //姿势列表
        public ActionResult BlogList()
        {
            ViewBag.Login = CurrentUser != null;
            return View();
        }

        //姿势分页
        [HttpPost]
        public ActionResult BlogPage(int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => true, it => it.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //查看姿势
        public ActionResult BlogView(Guid id, int co = 0, int ro = 0)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            ViewBag.Blog = blog;
            ViewBag.Login = CurrentUser != null;
            ViewBag.Owner = CurrentUser != null ? CurrentUser.ID == blog.OwnerID : false;

            ViewBag.COrder = co;
            ViewBag.ROrder = ro;

            ViewBag.ShowPro = false;
            if (CurrentUser != null)
            {
                string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
                IEnumerable<UserRecord> UserRecords = MyRedisDB.GetSet<UserRecord>(key);
                if(UserRecords.Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = 1 });
                    MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                    blog.ViewCount += 1;
                    BlogDataSvc.Update(blog);
                }
                else if(UserRecords.Where(r => r.TargetID == blog.ID && r.type == 1).Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = 1 });
                    blog.ViewCount += 1;
                    BlogDataSvc.Update(blog);
                }

                if (UserRecords.Where(r => r.TargetID == blog.ID && r.type == 2).Count() == 0)
                {
                    ViewBag.ShowPro = true;
                }
            }

            return View();
        }

        //点赞
        [HttpPost]
        public ActionResult ProBlog(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
            IEnumerable<UserRecord> UserRecords = MyRedisDB.GetSet<UserRecord>(key);
            if (UserRecords.Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = 2 });
                MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                blog.ProCount += 1;
                BlogDataSvc.Update(blog);
            }
            else if (UserRecords.Where(r => r.TargetID == blog.ID && r.type == 2).Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = 2 });
                blog.ProCount += 1;
                BlogDataSvc.Update(blog);
            }
            return Json(new { msg = "done", count = blog.ProCount });
        }

        //添加评论
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBlogComment(Guid blogID, string mdTxt, string mdValue)
        {
            Blog blog = BlogDataSvc.GetByID(blogID);
            blog.CommentCount += 1;

            BlogComment comment = new BlogComment();
            comment.BlogID = blogID;
            comment.MDText = mdTxt;
            comment.MDValue = mdValue;
            comment.OwnerID = CurrentUser.ID;
            comment.Order = blog.CommentCount;
            BlogCommentDataSvc.Add(comment);

            BlogDataSvc.Update(blog);

            if(blog.OwnerID != CurrentUser.ID)
            {
                string key = MyRedisKeys.Pre_NewBCMsg + blog.OwnerID;
                NewBCMsg bcmsg = MyRedisDB.GetSet<NewBCMsg>(key).Where(m => m.BlogID == blogID).FirstOrDefault();
                if (bcmsg != null)
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
                    bcmsg.Order = comment.Order;
                    bcmsg.Title = blog.Title.Length > 15 ? blog.Title.Substring(0, 15) + "……" : blog.Title;
                    MyRedisDB.SetAdd(key, bcmsg);
                }
            }

            return Json(new { msg = "done", count = blog.CommentCount });
        }

        //评论分页
        [HttpPost]
        public ActionResult BlogCommentPage(Guid blogID, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogCommentList = BlogCommentDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogID == blogID, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //添加评论回复
        [HttpPost]
        public ActionResult AddBlogCommentReply(Guid commentID, Guid toUserID, string txt)
        {
            BlogComment comment = BlogCommentDataSvc.GetByID(commentID);
            comment.ReplyCount += 1;
            
            BlogCommentReply reply = new BlogCommentReply();
            reply.BlogCommentID = commentID;
            reply.Content = txt;
            reply.ToUserID = toUserID;
            reply.OwnerID = CurrentUser.ID;
            reply.Order = comment.ReplyCount;
            BlogCommentReplyDataSvc.Add(reply);

            BlogCommentDataSvc.Update(comment);

            if(toUserID != CurrentUser.ID)
            {
                string key = MyRedisKeys.Pre_NewBCRMsg + toUserID;
                NewBCRMsg bcrmsg = new NewBCRMsg();
                bcrmsg.BlogID = comment.BlogID;
                bcrmsg.Date = DateTime.Now;
                bcrmsg.From = CurrentUser.UserName;
                bcrmsg.COrder = comment.Order;
                bcrmsg.ROrder = reply.Order;
                bcrmsg.Title = txt.Length > 15 ? txt.Substring(0, 15) + "……" : txt;
                MyRedisDB.SetAdd(key, bcrmsg);
            }

            return Json(new { msg = "done" });
        }

        //评论回复分页
        [HttpPost]
        public ActionResult BlogCommentReplyPage(Guid commentID, int corder, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogCommentReplyList = BlogCommentReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogCommentID == commentID, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.COrder = corder;
            return View();
        }

        #endregion

        #region Msg

        //未读消息数量
        public string GetMsgCount()
        {
            string BCkey = MyRedisKeys.Pre_NewBCMsg + CurrentUser.ID;
            string BCRkey = MyRedisKeys.Pre_NewBCRMsg + CurrentUser.ID;
            return (MyRedisDB.RedisDB.SetLength(BCkey) + MyRedisDB.RedisDB.SetLength(BCRkey)).ToString();
        }

        //未读消息
        public ActionResult GetMsg()
        {
            string BCkey = MyRedisKeys.Pre_NewBCMsg + CurrentUser.ID;
            string BCRkey = MyRedisKeys.Pre_NewBCRMsg + CurrentUser.ID;
            ViewBag.NewComments = MyRedisDB.GetSet<NewBCMsg>(BCkey).OrderByDescending(m => m.Date);
            ViewBag.NewReplys = MyRedisDB.GetSet<NewBCRMsg>(BCRkey).OrderByDescending(m => m.Date);
            return View();
        }

        //清空未读
        [HttpPost]
        public ActionResult ClearMsg()
        {
            string BCkey = MyRedisKeys.Pre_NewBCMsg + CurrentUser.ID;
            string BCRkey = MyRedisKeys.Pre_NewBCRMsg + CurrentUser.ID;
            MyRedisDB.DelKey(BCkey);
            MyRedisDB.DelKey(BCRkey);
            return Json(new { msg = "done" });
        }

        #endregion

        //错误
        public ActionResult Error()
        {
            return View();
        }
    }
}