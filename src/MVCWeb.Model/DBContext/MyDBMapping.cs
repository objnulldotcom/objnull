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
        }
    }
}
