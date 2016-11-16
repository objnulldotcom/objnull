using System.Data.Entity;
using MVCWeb.Model.Models;

namespace MVCWeb.Model.DBContext
{
    public class MyDBContext : DbContext
    {
        //实体集
        public IDbSet<NullUser> Users { get; set; }
        public IDbSet<Recruit> Recruits { get; set; }

        public MyDBContext() : base("MySQLConnection")
        {
            Database.SetInitializer(new MyDBInitializer());
            //Database.SetInitializer<MyDBContext>(null);

            //延迟加载开关，默认开启，注意实体中集合属性必须是virtual类型才生效
            //Configuration.LazyLoadingEnabled = false;
        }

        //建表时配置
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //表映射
            modelBuilder.Configurations.Add(new NullUserMapping());
            modelBuilder.Configurations.Add(new RecruitMapping());
        }
    }
}
