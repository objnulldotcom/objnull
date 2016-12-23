using System.Data.Entity.ModelConfiguration;
using MVCWeb.Model.Models;

namespace MVCWeb.Model.DBContext
{
    public class NullUserMapping : EntityTypeConfiguration<NullUser>
    {
        public NullUserMapping()
        {
            ToTable("NullUser");
            HasKey(that => that.ID);
        }
    }

    public class BlogMapping : EntityTypeConfiguration<Blog>
    {
        public BlogMapping()
        {
            ToTable("Blog");
            HasKey(it => it.ID);

            HasRequired(it => it.Owner).WithMany(that => that.Blogs).HasForeignKey(it => it.OwnerID);
        }
    }

    public class BlogCommentMapping : EntityTypeConfiguration<BlogComment>
    {
        public BlogCommentMapping()
        {
            ToTable("BlogComment");
            HasKey(it => it.ID);

            HasRequired(it => it.Owner).WithMany(that => that.BlogComments).HasForeignKey(it => it.OwnerID);
            HasRequired(it => it.Blog).WithMany(that => that.BlogComments).HasForeignKey(it => it.BlogID);
        }
    }

    public class BlogCommentReplyMapping : EntityTypeConfiguration<BlogCommentReply>
    {
        public BlogCommentReplyMapping()
        {
            ToTable("BlogCommentReply");
            HasKey(it => it.ID);

            HasRequired(it => it.Owner).WithMany(that => that.BlogCommentReplys).HasForeignKey(it => it.OwnerID);
            HasRequired(it => it.BlogComment).WithMany(that => that.BlogCommentReplys).HasForeignKey(it => it.BlogCommentID);
            HasRequired(it => it.ToUser).WithMany(that => that.ReceivedBlogCommentReplys).HasForeignKey(it => it.ToUserID);
        }
    }

    public class UserStarMapping : EntityTypeConfiguration<UserStar>
    {
        public UserStarMapping()
        {
            ToTable("UserStar");
            HasKey(that => that.ID);

            HasRequired(it => it.Owner).WithMany(that => that.UserStars).HasForeignKey(it => it.OwnerID);
        }
    }

    public class NewBeeMapping : EntityTypeConfiguration<NewBee>
    {
        public NewBeeMapping()
        {
            ToTable("NewBee");
            HasKey(it => it.ID);

            HasRequired(it => it.Owner).WithMany(that => that.NewBees).HasForeignKey(it => it.OwnerID);
        }
    }

    public class NewBeeFloorMapping : EntityTypeConfiguration<NewBeeFloor>
    {
        public NewBeeFloorMapping()
        {
            ToTable("NewBeeFloor");
            HasKey(it => it.ID);

            HasRequired(it => it.Owner).WithMany(that => that.NewBeeFloors).HasForeignKey(it => it.OwnerID);
            HasRequired(it => it.NewBee).WithMany(that => that.NewBeeFloors).HasForeignKey(it => it.NewBeeID);
        }
    }

    public class NewBeeFloorReplyMapping : EntityTypeConfiguration<NewBeeFloorReply>
    {
        public NewBeeFloorReplyMapping()
        {
            ToTable("NewBeeFloorReply");
            HasKey(it => it.ID);

            HasRequired(it => it.Owner).WithMany(that => that.NewBeeFloorReplys).HasForeignKey(it => it.OwnerID);
            HasRequired(it => it.NewBeeFloor).WithMany(that => that.NewBeeFloorReplys).HasForeignKey(it => it.NewBeeFloorID);
            HasRequired(it => it.ToUser).WithMany(that => that.ReceivedNewBeeFloorReplys).HasForeignKey(it => it.ToUserID);
        }
    }

    public class FeedbackMapping : EntityTypeConfiguration<Feedback>
    {
        public FeedbackMapping()
        {
            ToTable("Feedback");
            HasKey(that => that.ID);
        }
    }
}
