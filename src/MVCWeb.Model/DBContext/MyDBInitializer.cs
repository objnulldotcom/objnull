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
            
            for(int i = 0; i < 50; i++)
            {
                Blog newBlog = new Blog();
                newBlog.MDText = "测试水电费";
                newBlog.Title = "规范化图书馆电饭锅萨芬电辅热阿斯达Greg阿斯顿发松岛枫" + i;
                newBlog.Type = (i % 5) + 1;
                newBlog.MDValue = "<h4>sgasdf s是大法官阿士大夫</h4><p>s然后电饭锅和返回dfalksdfksd撒旦法</p>";
            }

            context.SaveChanges();
        }
    }
}
