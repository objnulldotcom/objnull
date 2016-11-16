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

    public class RecruitMapping : EntityTypeConfiguration<Recruit>
    {
        public RecruitMapping()
        {
            ToTable("Recruit");
            HasKey(r => r.ID);

            HasRequired(r => r.Owner).WithMany(n => n.Recruits).HasForeignKey(r => r.OwnerID);
        }
    }
}
