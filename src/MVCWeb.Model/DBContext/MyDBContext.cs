using System.Data.Entity;
using MVCWeb.Model.Models;

namespace MVCWeb.Model.DBContext
{
    public class MyDBContext : DbContext
    {
        //实体集
        public IDbSet<NullUser> Users { get; set; }
        public IDbSet<Blog> Blogs { get; set; }
        public IDbSet<BlogComment> BlogComments { get; set; }
        public IDbSet<BlogCommentReply> BlogCommentReplys { get; set; }
        public IDbSet<NewBee> NewBees { get; set; }
        public IDbSet<NewBeeFloor> NewBeeFloors { get; set; }
        public IDbSet<NewBeeFloorReply> NewBeeFloorReplys { get; set; }
        public IDbSet<UserStar> UserStars { get; set; }
        public IDbSet<Feedback> Feedbacks { get; set; }

        public MyDBContext() : base("MySQLConnection")
        {
            //Database.SetInitializer(new MyDBInitializer());
            Database.SetInitializer<MyDBContext>(null);

            //延迟加载开关，默认开启，注意实体中集合属性必须是virtual类型才生效
            //Configuration.LazyLoadingEnabled = false;
        }

        //建表时配置
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //表映射
            modelBuilder.Configurations.Add(new NullUserMapping());
            modelBuilder.Configurations.Add(new BlogMapping());
            modelBuilder.Configurations.Add(new BlogCommentMapping());
            modelBuilder.Configurations.Add(new BlogCommentReplyMapping());
            modelBuilder.Configurations.Add(new UserStarMapping());
            modelBuilder.Configurations.Add(new NewBeeMapping());
            modelBuilder.Configurations.Add(new NewBeeFloorMapping());
            modelBuilder.Configurations.Add(new NewBeeFloorReplyMapping());
            modelBuilder.Configurations.Add(new FeedbackMapping());
        }
    }
}
