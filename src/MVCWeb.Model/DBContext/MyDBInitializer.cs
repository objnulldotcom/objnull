using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCWeb.Model.Models;

namespace MVCWeb.Model.DBContext
{
    public class MyDBInitializer : DropCreateDatabaseIfModelChanges<MyDBContext> /*DropCreateDatabaseAlways<MyDBContext>*/
    {
        protected override void Seed(MyDBContext context)
        {
            base.Seed(context);

            NullUser user = new NullUser();
            user.LoginType = "github";
            user.Name = "CodeFarmer";
            user.AvatarUrl = "https://avatars.githubusercontent.com/u/4160160?v=3";
            user.GitHubID = 4160160;
            user.GitHubLogin = "631320085";
            user.GitHubAccessToken = "ded3178d2ff138f0a2e53baa8e5d1deae6af1dda";
            context.Users.Add(user);


            context.SaveChanges();
        }
    }
}
