using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace MVCWeb.Model.DBContext
{
    public class MyDBInitializer : DropCreateDatabaseIfModelChanges<MyDBContext> /*DropCreateDatabaseAlways<MyDBContext>*/
    {
        protected override void Seed(MyDBContext context)
        {
            base.Seed(context);
            
            context.SaveChanges();
        }
    }
}
