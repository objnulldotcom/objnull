using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;

namespace MVCWeb
{
    public static class GitHub
    {
        public static GitHubUser GetGitHubUser(string token)
        {
            string userInfo;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
                HttpResponseMessage response = hc.GetAsync("https://api.github.com/user?access_token=" + token).Result;
                userInfo = response.Content.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<GitHubUser>(userInfo);
        }

        public static GitHubUser GetGitHubUserByName(string loginName)
        {
            string userInfo;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
                HttpResponseMessage response = hc.GetAsync("https://api.github.com/users/" + loginName).Result;
                userInfo = response.Content.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<GitHubUser>(userInfo);
        }
    }
}