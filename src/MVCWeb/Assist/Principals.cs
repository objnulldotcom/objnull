using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace MVCWeb
{
    public class CurrentUser : IPrincipal
    {
        public IIdentity Identity
        {
            get
            {
                return null;
            }
        }

        public bool IsInRole(string role)
        {
            return true;
        }

        public Guid ID { get; set; }

        public string Name { get; set; }

        public string AvatarUrl { get; set; }

        public string LoginType { get; set; }

        public string GitHubLogin { get; set; }

        public int Role { get; set; }

        public string UserName
        {
            get
            {
                return string.IsNullOrEmpty(Name) ? GitHubLogin : Name;
            }
        }
    }

    public class CurrentManager : IPrincipal
    {
        public IIdentity Identity
        {
            get
            {
                return null;
            }
        }

        public bool IsInRole(string role)
        {
            return true;
        }

        public Guid ID { get; set; }

        public string key { get; set; }

        public int Role { get; set; }
    }
}