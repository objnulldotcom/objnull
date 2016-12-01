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
        public IUserStarDataSvc UserStarDataSvc { get; set; }
        public INewBeeDataSvc NewBeeDataSvc { get; set; }
        public INewBeeFloorDataSvc NewBeeFloorDataSvc { get; set; }
        public INewBeeFloorReplyDataSvc NewBeeFloorReplyDataSvc { get; set; }
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

        #region UserInfo

        //个人主页
        public ActionResult UserProfile(string id = null)
        {
            Guid userID = id == null ? CurrentUser.ID : Guid.Parse(id);
            ViewBag.Owner = userID == CurrentUser.ID;
            ViewBag.User = NullUserDataSvc.GetByID(userID);
            ViewBag.Token = CurrentUser.GitHubAccessToken;
            return View();
        }

        //用户姿势
        [HttpPost]
        public ActionResult UserBlogPage(Guid uid, int pageSize, int pageNum = 1)
        {
            ViewBag.Owner = uid == CurrentUser.ID;
            int totalCount;
            ViewBag.UserBlogs = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == uid, b => b.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //word收藏
        [HttpPost]
        public ActionResult UserStarPage(int type, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.UserStars = UserStarDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == CurrentUser.ID && b.Type == type, b => b.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.Type = type;
            return View();
        }

        //word消息
        [HttpPost]
        public ActionResult UserMsgPage(string type, int pageSize, int pageNum = 1)
        {
            int totalCount = 0;
            switch (type)
            {
                case "replyme":
                    ViewBag.ReplyMes = BlogCommentReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.ToUserID == CurrentUser.ID, b => b.InsertDate, true, out totalCount).ToList();
                    break;
                case "myreply":
                    ViewBag.MyReplys = BlogCommentReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == CurrentUser.ID, b => b.InsertDate, true, out totalCount).ToList();
                    break;
                case "mycomment":
                    ViewBag.MyComments = BlogCommentDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == CurrentUser.ID, b => b.InsertDate, true, out totalCount).ToList();
                    break;
            }
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.Type = type;
            return View();
        }

        #endregion

        #region 姿势blog

        //新姿势 needlogin
        public ActionResult NewBlog()
        {
            string key = MyRedisKeys.Pre_BlogDraft + CurrentUser.ID;
            string draftval = MyRedisDB.StringGet(key);
            if (!string.IsNullOrEmpty(draftval))
            {
                Blog draft = JsonConvert.DeserializeObject<Blog>(draftval);
                ViewBag.DraftBlog = draft;
            }
            return View();
        }

        //保存草稿 needlogin
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

        //发表新姿势 needlogin
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
            //禁止脚本
            mdTxt = mdTxt.Replace("<script", "&lt;script").Replace("</script", "&lt;/script");
            mdValue = mdValue.Replace("<script", "&lt;script").Replace("</script", "&lt;/script");
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

        //编辑姿势 needlogin
        public ActionResult EditBlog(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            if(blog.OwnerID != CurrentUser.ID)
            {
                RedirectToAction("Error");
            }
            ViewBag.Blog = blog;
            return View();
        }

        //完成编辑 needlogin
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditBlog(Guid id, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }
            int txtlength = Encoding.Default.GetByteCount(mdTxt);
            if (txtlength > 50000)
            {
                return Json(new { msg = "参数太长" });
            }

            //禁止脚本
            mdTxt = mdTxt.Replace("<script", "&lt;script").Replace("</script", "&lt;/script");
            mdValue = mdValue.Replace("<script", "&lt;script").Replace("</script", "&lt;/script");
            Blog nblog = BlogDataSvc.GetByID(id);
            nblog.MDText = mdTxt;
            nblog.MDValue = mdValue;
            BlogDataSvc.Update(nblog);
            return Json(new { msg = "done", url = Url.Action("BlogView", new { id = nblog.ID }) });
        }

        //姿势列表
        public ActionResult BlogList()
        {
            ViewBag.Login = CurrentUser != null;
            return View();
        }

        //姿势分页
        [HttpPost]
        public ActionResult BlogPage(string order, int pageSize, int pageNum = 1)
        {
            DateTime validDate = DateTime.Now.AddDays(-3);
            int totalCount = 0;
            if(order == "new")
            {
                ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => true, it => it.InsertDate, true, out totalCount).ToList();
            }
            else if(order == "view")
            {
                ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.InsertDate > validDate, it => it.ViewCount, true, out totalCount).ToList();
            }
            else if(order == "pro")
            {
                ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.InsertDate > validDate, it => it.ProCount, true, out totalCount).ToList();
            }
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
                //查看次数
                string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
                IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
                if (userRecords.Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = (int)EnumRecordType.查看 });
                    MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                    blog.ViewCount += 1;
                    BlogDataSvc.Update(blog);
                }
                else if (userRecords.Where(r => r.TargetID == blog.ID && r.type == (int)EnumRecordType.查看).Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = (int)EnumRecordType.查看 });
                    blog.ViewCount += 1;
                    BlogDataSvc.Update(blog);
                }
                //点赞
                if (userRecords.Where(r => r.TargetID == blog.ID && r.type == (int)EnumRecordType.点赞).Count() == 0)
                {
                    ViewBag.ShowPro = true;
                }
                //收藏
                ViewBag.ShowStar = true;
                string starKey = MyRedisKeys.Pre_UserStarCache + CurrentUser.ID;
                IEnumerable<UserStarCache> userStarCaches = MyRedisDB.GetSet<UserStarCache>(starKey);
                if (userStarCaches.Count() == 0)
                {
                    IEnumerable<UserStar> userStars = UserStarDataSvc.GetByCondition(s => s.OwnerID == CurrentUser.ID);
                    if (userStars.Count() > 0)
                    {
                        if (userStars.Where(s => s.TargetID == blog.ID && s.Type == (int)EnumStarType.姿势).Count() > 0)
                        {
                            ViewBag.ShowStar = false;
                        }
                        foreach (UserStar star in userStars)//添加收藏缓存
                        {
                            MyRedisDB.SetAdd(starKey, new UserStarCache() { TargetID = blog.ID, type = star.Type });
                        }
                        MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
                    }
                }
                else if (userStarCaches.Where(s => s.TargetID == blog.ID && s.type == (int)EnumStarType.姿势).Count() > 0)
                {
                    ViewBag.ShowStar = false;
                }
            }

            return View();
        }

        //点赞 needlogin
        [HttpPost]
        public ActionResult ProBlog(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
            IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
            if (userRecords.Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = (int)EnumRecordType.点赞 });
                MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                blog.ProCount += 1;
                BlogDataSvc.Update(blog);
            }
            else if (userRecords.Where(r => r.TargetID == blog.ID && r.type == (int)EnumRecordType.点赞).Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { TargetID = blog.ID, type = (int)EnumRecordType.点赞 });
                blog.ProCount += 1;
                BlogDataSvc.Update(blog);
            }
            return Json(new { msg = "done", count = blog.ProCount });
        }

        //收藏 needlogin
        [HttpPost]
        public ActionResult StarBlog(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            bool add = false;
            string starKey = MyRedisKeys.Pre_UserStarCache + CurrentUser.ID;
            IEnumerable<UserStarCache> userStarCaches = MyRedisDB.GetSet<UserStarCache>(starKey);
            if (userStarCaches.Count() == 0)
            {
                IEnumerable<UserStar> userStars = UserStarDataSvc.GetByCondition(s => s.OwnerID == CurrentUser.ID);
                if (userStars.Count() > 0)
                {
                    //添加收藏缓存
                    foreach (UserStar star in userStars)
                    {
                        MyRedisDB.SetAdd(starKey, new UserStarCache() { TargetID = blog.ID, type = star.Type });
                    }
                    MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
                    //添加收藏
                    if (userStars.Where(s => s.TargetID == blog.ID && s.Type == (int)EnumStarType.姿势).Count() == 0)
                    {
                        add = true;
                    }
                }
                else
                {
                    add = true;
                }
            }
            else if (userStarCaches.Where(s => s.TargetID == blog.ID && s.type == (int)EnumStarType.姿势).Count() == 0)
            {
                add = true;
            }
            if (add)
            {
                //添加收藏
                UserStar star = new UserStar();
                star.OwnerID = CurrentUser.ID;
                star.TargetID = blog.ID;
                star.Title = blog.Title;
                star.Type = (int)EnumStarType.姿势;
                UserStarDataSvc.Add(star);
                MyRedisDB.SetAdd(starKey, new UserStarCache() { TargetID = blog.ID, type = star.Type });
            }
            return Json(new { msg = "done", count = blog.ProCount });
        }

        //添加评论 needlogin
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBlogComment(Guid blogID, string mdTxt, string mdValue)
        {
            //禁止脚本
            mdTxt = mdTxt.Replace("<script", "&lt;script").Replace("</script", "&lt;/script");
            mdValue = mdValue.Replace("<script", "&lt;script").Replace("</script", "&lt;/script");
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

            if (blog.OwnerID != CurrentUser.ID)
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
            ViewBag.Login = CurrentUser != null;
            ViewBag.BlogCommentList = BlogCommentDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogID == blogID, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //添加评论回复 needlogin
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBlogCommentReply(Guid commentID, Guid toUserID, string txt)
        {
            txt = HttpUtility.HtmlEncode(txt);
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

            if (toUserID != CurrentUser.ID)
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

        #region NewBee

        public ActionResult NewBeeList()
        {
            ViewBag.Login = CurrentUser != null;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewNewBee(string title, string mdTxt, string mdValue)
        {
            NewBee nb = new NewBee();
            nb.OwnerID = CurrentUser.ID;
            nb.Title = title;
            nb.FloorCount = 1;
            NewBeeDataSvc.Add(nb);

            NewBeeFloor nbf = new NewBeeFloor();
            nbf.MDText = mdTxt;
            nbf.MDValue = mdValue;
            nbf.NewBeeID = nb.ID;
            nbf.Order = 1;
            nbf.OwnerID = CurrentUser.ID;
            NewBeeFloorDataSvc.Add(nbf);

            return Json(new { msg = "done" });
        }
        
        //姿势分页
        [HttpPost]
        public ActionResult NewBeePage(int pageSize, int pageNum = 1)
        {
            int totalCount = 0;
            ViewBag.NewBeeList = NewBeeDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => true, it => it.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        public ActionResult NewBeeView(Guid id)
        {
            ViewBag.NewBee = NewBeeDataSvc.GetByID(id);
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