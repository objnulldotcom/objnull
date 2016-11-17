using System.Data.Entity.ModelConfiguration;
using MVCWeb.Model.Models;

namespace MVCWeb.Model.DBContext
{
    public class NullUserMapping : EntityTypeConfiguration<NullUser>
    {
        public NullUserMapping()
        {
            ToTable("NullUser");
            HasKey(n => n.ID);
        }
    }

    public class BlogMapping : EntityTypeConfiguration<Blog>
    {
        public BlogMapping()
        {
            ToTable("Blog");
            HasKey(r => r.ID);

            HasRequired(r => r.Owner).WithMany(n => n.Blogs).HasForeignKey(r => r.OwnerID);
        }
    }
}
