using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCWeb
{
    public class GitHubAccessToken
    {
        public string access_token { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
    }
}