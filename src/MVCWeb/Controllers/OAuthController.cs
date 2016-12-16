using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using MVCWeb.DataSvc.Svc;
using MVCWeb.Model.Models;
using MVCWeb.Redis.Base;

namespace MVCWeb.Controllers
{
    public class OAuthController : BaseController
    {
        public INullUserDataSvc NullUserDataSvc { get; set; }
        public IMyRedisDB MyRedisDB { get; set; }

        //用户信息
        public ActionResult UserInfo()
        {
            if (CurrentUser != null)
            {
                ViewBag.User = CurrentUser;
                ViewBag.MsgCount = MyRedisDB.RedisDB.SetLength(MyRedisKeys.Pre_CMsg + CurrentUser.ID) 
                    + MyRedisDB.RedisDB.SetLength(MyRedisKeys.Pre_RMsg + CurrentUser.ID) + MyRedisDB.RedisDB.SetLength(MyRedisKeys.Pre_SysMsg + CurrentUser.ID);
                if (string.IsNullOrEmpty(HttpContext.ReadCookie("LastLogin")))
                {
                    NullUser user = NullUserDataSvc.GetByID(CurrentUser.ID);
                    user.LastLoginDate = DateTime.Now;
                    user.LastLoginIP = HttpContext.Request.UserHostAddress;
                    NullUserDataSvc.Update(user);
                    HttpContext.WriteCookie("LastLogin", DateTime.Now.ToString(), DateTime.Now.AddDays(1).Date);
                }
            }
            return PartialView();
        }
        
        [HttpPost]
        //更新账号
        public ActionResult UpdateInfo()
        {
            NullUser user = NullUserDataSvc.GetByID(CurrentUser.ID);
            if (UpdateUserInfo("github", user.GitHubAccessToken))
            {
                return Json(new { msg = "done" });
            }
            else
            {
                return Json(new { msg = "error" });
            }
        }

        //退出
        public ActionResult LogOut()
        {
            HttpContext.WriteCookie("UID", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("UName", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("UAvatar", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("LoginType", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("GLogin", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("SKEY", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("LastLogin", "", DateTime.Now.AddDays(-1));
            HttpContext.WriteCookie("Role", "", DateTime.Now.AddDays(-1));
            return RedirectToAction("Index", "Home");
        }

        //添加用户或更新用户信息
        public bool UpdateUserInfo(string loginType, string token)
        {
            if(loginType == "github")
            {
                GitHubUser githubUser = GitHub.GetGitHubUser(token);
                if(githubUser.id == 0)
                {
                    return false;
                }
                NullUser user = NullUserDataSvc.GetByCondition(u => u.GitHubID == githubUser.id).FirstOrDefault();
                if (user == null)
                {
                    user = new NullUser();
                    user.LoginType = "github";
                    user.GitHubAccessToken = token;
                    user.Name = githubUser.name;
                    user.AvatarUrl = githubUser.avatar_url;
                    user.GitHubLogin = githubUser.login;
                    user.GitHubID = githubUser.id;
                    user.Role = (int)EnumUserRole.普通;
                    NullUserDataSvc.Add(user);
                }
                else
                {
                    user.Name = githubUser.name;
                    user.AvatarUrl = githubUser.avatar_url;
                    user.GitHubID = githubUser.id;
                    user.GitHubLogin = githubUser.login;
                    user.GitHubAccessToken = token;
                    NullUserDataSvc.Update(user);
                }
                
                HttpContext.WriteCookie("UID", user.ID.ToString(), DateTime.Now.AddYears(3));
                HttpContext.WriteCookie("UName", githubUser.name, DateTime.Now.AddYears(3));
                HttpContext.WriteCookie("UAvatar", githubUser.avatar_url, DateTime.Now.AddYears(3));
                HttpContext.WriteCookie("LoginType", user.LoginType, DateTime.Now.AddYears(3));
                HttpContext.WriteCookie("GLogin", githubUser.login, DateTime.Now.AddYears(3));
                HttpContext.WriteCookie("SKEY", Utils.RijndaelEncrypt(user.ID.ToString()), DateTime.Now.AddYears(3));
                HttpContext.WriteCookie("Role", Utils.RijndaelEncrypt(user.ID.ToString() + ";" + user.Role.ToString()), DateTime.Now.AddYears(3));
            }
            return true;
        }

        #region GitHub

        public ActionResult GitHubLogin(string code, string state)
        {
            //获取token
            GitHubAccessToken token;
            using (HttpClient hc = new HttpClient())
            {
                FormUrlEncodedContent postData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", "b89774f9a3a874e349ce"),
                    new KeyValuePair<string, string>("client_secret", "a225b95faff4466fbf7948c3d1f4c229e5fccdd9"),
                    new KeyValuePair<string, string>("code", code)
                });
                hc.DefaultRequestHeaders.Add("Accept", "application/json");
                HttpResponseMessage response = hc.PostAsync("https://github.com/login/oauth/access_token", postData).Result;
                token = JsonConvert.DeserializeObject<GitHubAccessToken>(response.Content.ReadAsStringAsync().Result);
            }
            if(token.access_token == null)
            {
                throw new Exception("获取token失败");
            }
            //添加用户或更新用户信息
            UpdateUserInfo("github", token.access_token);

            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}