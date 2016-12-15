using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Ganss.XSS;
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

        public HtmlSanitizer HtmlST = new HtmlSanitizer();
        
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
            NullUser user = NullUserDataSvc.GetByID(userID);
            ViewBag.User = user;
            NullUser cuser = NullUserDataSvc.GetByID(CurrentUser.ID);
            ViewBag.Token = cuser.GitHubAccessToken;

            ViewBag.ShowPro = false;
            string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
            IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
            if (userRecords.Count() == 0)
            {
                ViewBag.ShowPro = true;
            }
            else if (userRecords.Where(r => r.ObjID == userID && r.type == (int)EnumRecordType.点赞).Count() == 0)
            {
                ViewBag.ShowPro = true;
            }

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

        //用户NewBee
        [HttpPost]
        public ActionResult UserNewBeePage(Guid uid, int pageSize, int pageNum = 1)
        {
            ViewBag.Owner = uid == CurrentUser.ID;
            int totalCount;
            ViewBag.UserNewBees = NewBeeDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == uid, b => b.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //word收藏
        [HttpPost]
        public ActionResult UserStarPage(int type, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.UserStars = UserStarDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == CurrentUser.ID && b.ObjType == type, b => b.InsertDate, true, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.Type = type;
            return View();
        }

        //删除收藏
        [HttpPost]
        public ActionResult StarDelete(Guid id)
        {
            UserStar star = UserStarDataSvc.GetByID(id);
            if (star.OwnerID != CurrentUser.ID)
            {
                return Json(new { msg = "小伙子你想干嘛" });
            }
            UserStarDataSvc.DeleteByID(id);
            string starKey = MyRedisKeys.Pre_UserStarCache + CurrentUser.ID;
            IEnumerable<UserStarCache> userStarCaches = MyRedisDB.GetSet<UserStarCache>(starKey);
            if (userStarCaches.Count() > 0)
            {
                UserStarCache starCache = userStarCaches.Where(s => s.ObjID == star.ObjID).FirstOrDefault();
                if (starCache != null)
                {
                    MyRedisDB.SetRemove(starKey, starCache);
                }
            }
            return Json(new { msg = "done" });
        }

        //word消息
        [HttpPost]
        public ActionResult UserMsgPage(string type, int pageSize, int pageNum = 1)
        {
            int totalCount = 0;
            switch (type)
            {
                case "Breply":
                    ViewBag.Breplys = BlogCommentReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, 
                        b => (b.OwnerID != CurrentUser.ID && b.ToUserID == CurrentUser.ID) || (b.OwnerID == CurrentUser.ID && b.ToUserID != CurrentUser.ID), 
                        b => b.InsertDate, true, out totalCount).ToList();
                    break;
                case "Bcomment":
                    ViewBag.Bcomments = BlogCommentDataSvc.GetPagedEntitys(ref pageNum, pageSize, b => b.OwnerID == CurrentUser.ID, b => b.InsertDate, true, out totalCount).ToList();
                    break;
                case "Nreply":
                    ViewBag.Nreplys = NewBeeFloorReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize,
                        n => (n.OwnerID != CurrentUser.ID && n.ToUserID == CurrentUser.ID) || (n.OwnerID == CurrentUser.ID && n.ToUserID != CurrentUser.ID),
                        n => n.InsertDate, true, out totalCount).ToList();
                    break;
                case "Ncomment":
                    ViewBag.Ncomments = NewBeeFloorDataSvc.GetPagedEntitys(ref pageNum, pageSize, n => n.OwnerID == CurrentUser.ID, n => n.InsertDate, true, out totalCount).ToList();
                    break;
            }
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.Type = type;
            ViewBag.CUserID = CurrentUser.ID;
            return View();
        }
        
        //用户点赞
        [HttpPost]
        public ActionResult UserPro(Guid id)
        {
            if(id == CurrentUser.ID)
            {
                return Json(new { msg = "buxing" });
            }

            string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
            IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
            if (userRecords.Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { ObjID = id, type = (int)EnumRecordType.点赞 });
                MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                ProUser(id);
            }
            else if (userRecords.Where(r => r.ObjID == id && r.type == (int)EnumRecordType.点赞).Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { ObjID = id, type = (int)EnumRecordType.点赞 });
                ProUser(id);
            }
            return Json(new { msg = "done" });
        }

        //用户加赞
        public void ProUser(Guid id)
        {
            NullUser user = NullUserDataSvc.GetByID(id);
            user.ProCount += 1;
            NullUserDataSvc.Update(user);
        }

        #endregion

        #region 姿势blog

        //新姿势
        public ActionResult BlogNew()
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

        //保存草稿
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveDraft(int type, string title, string mdTxt)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(mdTxt))
            {
                return Json(new { msg = "empty" });
            }
            if (title.GetByteCount() > 90 || mdTxt.GetByteCount() > 50000)
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
        public ActionResult BlogNew(int type, string title, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }
            if (title.GetByteCount() > 90 || mdTxt.GetByteCount() > 50000 || mdValue.GetByteCount() > 100000)
            {
                return Json(new { msg = "参数太长" });
            }
            if (type < 0 || type > 4)
            {
                type = 0;
            }

            //内容无害化
            mdValue = HtmlST.Sanitize(mdValue);
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

        //编辑姿势
        public ActionResult BlogEdit(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            if (blog.OwnerID != CurrentUser.ID)
            {
                RedirectToAction("Error");
            }
            ViewBag.Blog = blog;
            return View();
        }

        //完成编辑
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult BlogEdit(Guid id, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }
            if (mdTxt.GetByteCount() > 50000 || mdValue.GetByteCount() > 100000)
            {
                return Json(new { msg = "参数太长" });
            }

            //内容无害化
            mdValue = HtmlST.Sanitize(mdValue);
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
        public ActionResult BlogPage(string order, int pageSize, int pageNum = 1, int days = 3)
        {
            DateTime validDate = DateTime.Now.AddDays(days * -1);
            int totalCount = 0;
            if (order == "new")
            {
                ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => true, it => it.InsertDate, true, out totalCount).ToList();
            }
            else if (order == "view")
            {
                ViewBag.BlogList = BlogDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.InsertDate > validDate, it => it.ViewCount, true, out totalCount).ToList();
            }
            else if (order == "pro")
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

            #region 查看点赞收藏是否显示
            if (CurrentUser != null)
            {
                //查看次数
                string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
                IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
                if (userRecords.Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { ObjID = blog.ID, type = (int)EnumRecordType.查看 });
                    MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                    blog.ViewCount += 1;
                    BlogDataSvc.Update(blog);
                }
                else if (userRecords.Where(r => r.ObjID == blog.ID && r.type == (int)EnumRecordType.查看).Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { ObjID = blog.ID, type = (int)EnumRecordType.查看 });
                    blog.ViewCount += 1;
                    BlogDataSvc.Update(blog);
                }

                //点赞
                ViewBag.ShowPro = false;
                if (userRecords.Where(r => r.ObjID == blog.ID && r.type == (int)EnumRecordType.点赞).Count() == 0)
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
                        if (userStars.Where(s => s.ObjID == blog.ID).Count() > 0)
                        {
                            ViewBag.ShowStar = false;
                        }
                        //添加收藏缓存
                        foreach (UserStar star in userStars)
                        {
                            MyRedisDB.SetAdd(starKey, new UserStarCache() { ObjID = blog.ID, ObjType = star.ObjType });
                        }
                        MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
                    }
                }
                else if (userStarCaches.Where(s => s.ObjID == blog.ID).Count() > 0)
                {
                    ViewBag.ShowStar = false;
                }
            }
            #endregion

            return View();
        }

        //点赞
        [HttpPost]
        public ActionResult ProBlog(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
            IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
            if (userRecords.Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { ObjID = blog.ID, type = (int)EnumRecordType.点赞 });
                MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                blog.ProCount += 1;
                BlogDataSvc.Update(blog);
                ProUser(blog.OwnerID);
            }
            else if (userRecords.Where(r => r.ObjID == blog.ID && r.type == (int)EnumRecordType.点赞).Count() == 0)
            {
                MyRedisDB.SetAdd(key, new UserRecord() { ObjID = blog.ID, type = (int)EnumRecordType.点赞 });
                blog.ProCount += 1;
                BlogDataSvc.Update(blog);
                ProUser(blog.OwnerID);
            }
            return Json(new { msg = "done", count = blog.ProCount });
        }

        //收藏
        [HttpPost]
        public ActionResult StarBlog(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            string starKey = MyRedisKeys.Pre_UserStarCache + CurrentUser.ID;
            IEnumerable<UserStarCache> userStarCaches = MyRedisDB.GetSet<UserStarCache>(starKey);

            bool add = false;
            if (userStarCaches.Count() == 0)
            {
                IEnumerable<UserStar> userStars = UserStarDataSvc.GetByCondition(s => s.OwnerID == CurrentUser.ID);
                if (userStars.Count() > 0)
                {
                    //添加收藏缓存
                    foreach (UserStar star in userStars)
                    {
                        MyRedisDB.SetAdd(starKey, new UserStarCache() { ObjID = blog.ID, ObjType = star.ObjType });
                    }
                    MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
                    //添加收藏
                    if (userStars.Where(s => s.ObjID == blog.ID).Count() == 0)
                    {
                        add = true;
                    }
                }
                else
                {
                    add = true;
                }
            }
            else if (userStarCaches.Where(s => s.ObjID == blog.ID).Count() == 0)
            {
                add = true;
            }

            if (add)
            {
                //添加收藏
                UserStar star = new UserStar();
                star.OwnerID = CurrentUser.ID;
                star.ObjID = blog.ID;
                star.Title = blog.Title;
                star.ObjType = (int)EnumObjectType.姿势;
                UserStarDataSvc.Add(star);
                MyRedisDB.SetAdd(starKey, new UserStarCache() { ObjID = blog.ID, ObjType = star.ObjType });
                MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
            }
            return Json(new { msg = "done" });
        }
        
        //删除
        [HttpPost]
        public ActionResult BlogDelete(Guid id)
        {
            Blog blog = BlogDataSvc.GetByID(id);
            if(blog.OwnerID != CurrentUser.ID)
            {
                return Json(new { msg = "小伙子你想干嘛" });
            }
            BlogDataSvc.DeleteByID(id);
            return Json(new { msg = "done" });
        }

        //添加评论
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBlogComment(Guid blogID, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }
            if (mdTxt.GetByteCount() > 5000 || mdValue.GetByteCount() > 10000)
            {
                return Json(new { msg = "参数太长" });
            }

            Blog blog = BlogDataSvc.GetByID(blogID);
            blog.CommentCount += 1;

            BlogComment comment = new BlogComment();
            comment.BlogID = blogID;
            comment.MDText = mdTxt;
            comment.MDValue = HtmlST.Sanitize(mdValue);
            comment.OwnerID = CurrentUser.ID;
            comment.Order = blog.CommentCount;
            BlogCommentDataSvc.Add(comment);

            BlogDataSvc.Update(blog);

            if (blog.OwnerID != CurrentUser.ID)
            {
                string key = MyRedisKeys.Pre_CMsg + blog.OwnerID;
                CMsg bcmsg = MyRedisDB.GetSet<CMsg>(key).Where(m => m.ObjID == blogID).FirstOrDefault();
                if (bcmsg != null)
                {
                    MyRedisDB.SetRemove(key, bcmsg);
                    bcmsg.Count += 1;
                    MyRedisDB.SetAdd(key, bcmsg);
                }
                else
                {
                    bcmsg = new CMsg();
                    bcmsg.ObjType = (int)EnumObjectType.姿势;
                    bcmsg.ObjID = blogID;
                    bcmsg.Count = 1;
                    bcmsg.Date = DateTime.Now;
                    bcmsg.Order = comment.Order;
                    bcmsg.Title = blog.Title.MaxByteLength(32);
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
            ViewBag.BlogCommentList = BlogCommentDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogID == blogID && !it.Delete, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            return View();
        }

        //添加评论回复
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddBlogCommentReply(Guid commentID, Guid toUserID, string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return Json(new { msg = "参数错误" });
            }
            if (txt.GetByteCount() > 400)
            {
                return Json(new { msg = "参数太长" });
            }

            BlogComment comment = BlogCommentDataSvc.GetByID(commentID);
            comment.ReplyCount += 1;

            BlogCommentReply reply = new BlogCommentReply();
            reply.BlogCommentID = commentID;
            reply.Content = HttpUtility.HtmlEncode(txt);
            reply.ToUserID = toUserID;
            reply.OwnerID = CurrentUser.ID;
            reply.Order = comment.ReplyCount;
            BlogCommentReplyDataSvc.Add(reply);

            BlogCommentDataSvc.Update(comment);

            if (toUserID != CurrentUser.ID)
            {
                string key = MyRedisKeys.Pre_RMsg + toUserID;
                RMsg bcrmsg = new RMsg();
                bcrmsg.ObjType = (int)EnumObjectType.姿势;
                bcrmsg.ObjID = comment.BlogID;
                bcrmsg.Date = DateTime.Now;
                bcrmsg.From = CurrentUser.UserName;
                bcrmsg.COrder = comment.Order;
                bcrmsg.ROrder = reply.Order;
                bcrmsg.Title = txt.MaxByteLength(32);
                MyRedisDB.SetAdd(key, bcrmsg);
            }

            return Json(new { msg = "done" });
        }

        //评论回复分页
        [HttpPost]
        public ActionResult BlogCommentReplyPage(Guid commentID, int corder, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.BlogCommentReplyList = BlogCommentReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.BlogCommentID == commentID && !it.Delete, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.COrder = corder;
            return View();
        }

        #endregion

        #region NewBee

        //TreeNewBee
        public ActionResult NewBeeList()
        {
            ViewBag.Login = CurrentUser != null;
            return View();
        }

        //添加NewBee
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewNewBee(string title, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }

            if (title.GetByteCount() > 100 || mdTxt.GetByteCount() > 2500 || mdValue.GetByteCount() > 5000)
            {
                return Json(new { msg = "参数太长" });
            }

            NewBee nb = new NewBee();
            nb.OwnerID = CurrentUser.ID;
            nb.Title = title;
            nb.FloorCount = 1;
            nb.LastFloorDate = DateTime.Now;
            NewBeeDataSvc.Add(nb);

            NewBeeFloor nbf = new NewBeeFloor();
            nbf.MDText = mdTxt;
            nbf.MDValue = HtmlST.Sanitize(mdValue);
            nbf.NewBeeID = nb.ID;
            nbf.Order = 1;
            nbf.OwnerID = CurrentUser.ID;
            NewBeeFloorDataSvc.Add(nbf);

            return Json(new { msg = "done" });
        }

        //NewBee分页
        [HttpPost]
        public ActionResult NewBeePage(int pageSize, int pageNum = 1)
        {
            int totalCount = 0;
            List<NewBee> NewBeeList = NewBeeDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => true, it => it.LastFloorDate, true, out totalCount).ToList();
            ViewBag.NewBeeList = NewBeeList;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.ShowPager = totalCount > pageSize;

            IEnumerable<Guid> NewBeeIDs = NewBeeList.Select(n => n.ID);
            ViewBag.FirstFloors = NewBeeFloorDataSvc.GetByCondition(f => NewBeeIDs.Contains(f.NewBeeID) && f.Order == 1).ToList();
            return View();
        }

        //NewBee详情
        public ActionResult NewBeeView(Guid id, int co = 0, int ro = 0)
        {
            NewBee newBee = NewBeeDataSvc.GetByID(id);
            ViewBag.NewBee = newBee;
            ViewBag.Login = CurrentUser != null;
            ViewBag.Owner = CurrentUser != null ? CurrentUser.ID == newBee.OwnerID : false;
            ViewBag.COrder = co;
            ViewBag.ROrder = ro;

            if (CurrentUser != null)
            {
                //查看次数
                string key = MyRedisKeys.Pre_UserRecord + CurrentUser.ID;
                IEnumerable<UserRecord> userRecords = MyRedisDB.GetSet<UserRecord>(key);
                if (userRecords.Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { ObjID = newBee.ID, type = (int)EnumRecordType.查看 });
                    MyRedisDB.RedisDB.KeyExpire(key, DateTime.Now.AddDays(1));
                    newBee.ViewCount += 1;
                    NewBeeDataSvc.Update(newBee);
                }
                else if (userRecords.Where(r => r.ObjID == newBee.ID && r.type == (int)EnumRecordType.查看).Count() == 0)
                {
                    MyRedisDB.SetAdd(key, new UserRecord() { ObjID = newBee.ID, type = (int)EnumRecordType.查看 });
                    newBee.ViewCount += 1;
                    NewBeeDataSvc.Update(newBee);
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
                        if (userStars.Where(s => s.ObjID == newBee.ID).Count() > 0)
                        {
                            ViewBag.ShowStar = false;
                        }
                        //添加收藏缓存
                        foreach (UserStar star in userStars)
                        {
                            MyRedisDB.SetAdd(starKey, new UserStarCache() { ObjID = newBee.ID, ObjType = star.ObjType });
                        }
                        MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
                    }
                }
                else if (userStarCaches.Where(s => s.ObjID == newBee.ID).Count() > 0)
                {
                    ViewBag.ShowStar = false;
                }
            }

            return View();
        }

        //收藏
        [HttpPost]
        public ActionResult StarNewBee(Guid id)
        {
            NewBee newBee = NewBeeDataSvc.GetByID(id);
            string starKey = MyRedisKeys.Pre_UserStarCache + CurrentUser.ID;
            IEnumerable<UserStarCache> userStarCaches = MyRedisDB.GetSet<UserStarCache>(starKey);

            bool add = false;
            if (userStarCaches.Count() == 0)
            {
                IEnumerable<UserStar> userStars = UserStarDataSvc.GetByCondition(s => s.OwnerID == CurrentUser.ID);
                if (userStars.Count() > 0)
                {
                    //添加收藏缓存
                    foreach (UserStar star in userStars)
                    {
                        MyRedisDB.SetAdd(starKey, new UserStarCache() { ObjID = newBee.ID, ObjType = star.ObjType });
                    }
                    MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
                    //添加收藏
                    if (userStars.Where(s => s.ObjID == newBee.ID).Count() == 0)
                    {
                        add = true;
                    }
                }
                else
                {
                    add = true;
                }
            }
            else if (userStarCaches.Where(s => s.ObjID == newBee.ID).Count() == 0)
            {
                add = true;
            }

            if (add)
            {
                //添加收藏
                UserStar star = new UserStar();
                star.OwnerID = CurrentUser.ID;
                star.ObjID = newBee.ID;
                star.Title = newBee.Title;
                star.ObjType = (int)EnumObjectType.NewBee;
                UserStarDataSvc.Add(star);
                MyRedisDB.SetAdd(starKey, new UserStarCache() { ObjID = newBee.ID, ObjType = star.ObjType });
                MyRedisDB.RedisDB.KeyExpire(starKey, DateTime.Now.AddHours(3));
            }
            return Json(new { msg = "done" });
        }

        //删除
        [HttpPost]
        public ActionResult NewBeeDelete(Guid id)
        {
            NewBee newBee = NewBeeDataSvc.GetByID(id);
            if (newBee.OwnerID != CurrentUser.ID)
            {
                return Json(new { msg = "小伙子你想干嘛" });
            }
            NewBeeDataSvc.DeleteByID(id);
            return Json(new { msg = "done" });
        }

        //添加NewBeeFloor
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddNewBeeFloor(Guid NewBeeID, string mdTxt, string mdValue)
        {
            if (string.IsNullOrEmpty(mdTxt) || string.IsNullOrEmpty(mdValue))
            {
                return Json(new { msg = "参数错误" });
            }
            if (mdTxt.GetByteCount() > 2500 || mdValue.GetByteCount() > 5000)
            {
                return Json(new { msg = "参数太长" });
            }

            NewBee newBee = NewBeeDataSvc.GetByID(NewBeeID);
            newBee.FloorCount += 1;
            newBee.LastFloorDate = DateTime.Now;

            NewBeeFloor floor = new NewBeeFloor();
            floor.NewBeeID = NewBeeID;
            floor.MDText = mdTxt;
            floor.MDValue = HtmlST.Sanitize(mdValue);
            floor.OwnerID = CurrentUser.ID;
            floor.Order = newBee.FloorCount;
            NewBeeFloorDataSvc.Add(floor);

            NewBeeDataSvc.Update(newBee);

            if (newBee.OwnerID != CurrentUser.ID)
            {
                string key = MyRedisKeys.Pre_CMsg + newBee.OwnerID;
                CMsg bcmsg = MyRedisDB.GetSet<CMsg>(key).Where(m => m.ObjID == NewBeeID).FirstOrDefault();
                if (bcmsg != null)
                {
                    MyRedisDB.SetRemove(key, bcmsg);
                    bcmsg.Count += 1;
                    MyRedisDB.SetAdd(key, bcmsg);
                }
                else
                {
                    bcmsg = new CMsg();
                    bcmsg.ObjType = (int)EnumObjectType.NewBee;
                    bcmsg.ObjID = NewBeeID;
                    bcmsg.Count = 1;
                    bcmsg.Date = DateTime.Now;
                    bcmsg.Order = floor.Order;
                    bcmsg.Title = newBee.Title.MaxByteLength(32);
                    MyRedisDB.SetAdd(key, bcmsg);
                }
            }

            return Json(new { msg = "done", count = newBee.FloorCount });
        }

        //NewBeeFloor分页
        [HttpPost]
        public ActionResult NewBeeFloorPage(Guid nbID, int pageSize, int pageNum = 1)
        {
            int totalCount = 0;
            ViewBag.Login = CurrentUser != null;
            ViewBag.NewBeeFloorList = NewBeeFloorDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.NewBeeID == nbID && !it.Delete, it => it.InsertDate, false, out totalCount).ToList();
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = pageNum;
            ViewBag.ShowPager = totalCount > pageSize;
            return View();
        }

        //添加楼层回复
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddNewBeeFloorReply(Guid floorID, Guid toUserID, string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return Json(new { msg = "参数错误" });
            }
            if (txt.GetByteCount() > 400)
            {
                return Json(new { msg = "参数太长" });
            }

            NewBeeFloor floor = NewBeeFloorDataSvc.GetByID(floorID);
            floor.ReplyCount += 1;

            NewBeeFloorReply reply = new NewBeeFloorReply();
            reply.NewBeeFloorID = floorID;
            reply.Content = HttpUtility.HtmlEncode(txt);
            reply.ToUserID = toUserID;
            reply.OwnerID = CurrentUser.ID;
            reply.Order = floor.ReplyCount;
            NewBeeFloorReplyDataSvc.Add(reply);

            NewBeeFloorDataSvc.Update(floor);

            if (toUserID != CurrentUser.ID)
            {
                string key = MyRedisKeys.Pre_RMsg + toUserID;
                RMsg bcrmsg = new RMsg();
                bcrmsg.ObjType = (int)EnumObjectType.NewBee;
                bcrmsg.ObjID = floor.NewBeeID;
                bcrmsg.Date = DateTime.Now;
                bcrmsg.From = CurrentUser.UserName;
                bcrmsg.COrder = floor.Order;
                bcrmsg.ROrder = reply.Order;
                bcrmsg.Title = txt.MaxByteLength(32);
                MyRedisDB.SetAdd(key, bcrmsg);
            }

            return Json(new { msg = "done" });
        }

        //楼层回复分页
        [HttpPost]
        public ActionResult NewBeeFloorReplyPage(Guid floorID, int corder, int pageSize, int pageNum = 1)
        {
            int totalCount;
            ViewBag.NewBeeFloorReplyList = NewBeeFloorReplyDataSvc.GetPagedEntitys(ref pageNum, pageSize, it => it.NewBeeFloorID == floorID && !it.Delete, it => it.InsertDate, false, out totalCount).ToList();
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
            string Ckey = MyRedisKeys.Pre_CMsg + CurrentUser.ID;
            string Rkey = MyRedisKeys.Pre_RMsg + CurrentUser.ID;
            return (MyRedisDB.RedisDB.SetLength(Ckey) + MyRedisDB.RedisDB.SetLength(Rkey)).ToString();
        }

        //未读消息
        public ActionResult GetMsg()
        {
            string Ckey = MyRedisKeys.Pre_CMsg + CurrentUser.ID;
            string Rkey = MyRedisKeys.Pre_RMsg + CurrentUser.ID;
            ViewBag.NewComments = MyRedisDB.GetSet<CMsg>(Ckey).OrderByDescending(m => m.Date);
            ViewBag.NewReplys = MyRedisDB.GetSet<RMsg>(Rkey).OrderByDescending(m => m.Date);
            return View();
        }

        //清空未读
        [HttpPost]
        public ActionResult ClearMsg()
        {
            string Ckey = MyRedisKeys.Pre_CMsg + CurrentUser.ID;
            string Rkey = MyRedisKeys.Pre_RMsg + CurrentUser.ID;
            MyRedisDB.DelKey(Ckey);
            MyRedisDB.DelKey(Rkey);
            return Json(new { msg = "done" });
        }

        //删除评论消息
        [HttpPost]
        public ActionResult DeleteCMsg(Guid objID)
        {
            string Ckey = MyRedisKeys.Pre_CMsg + CurrentUser.ID;
            MyRedisDB.SetRemove(Ckey, MyRedisDB.GetSet<CMsg>(Ckey).Where(c => c.ObjID == objID).FirstOrDefault());
            return Json(new { msg = "done" });
        }

        //删除回复消息
        [HttpPost]
        public ActionResult DeletRMsg(Guid objID, int co, int ro)
        {
            string Rkey = MyRedisKeys.Pre_RMsg + CurrentUser.ID;
            MyRedisDB.SetRemove(Rkey, MyRedisDB.GetSet<RMsg>(Rkey).Where(r => r.ObjID == objID && r.COrder == co && r.ROrder == ro).FirstOrDefault());
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