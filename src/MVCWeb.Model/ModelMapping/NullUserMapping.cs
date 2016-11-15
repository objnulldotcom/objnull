using System.Data.Entity.ModelConfiguration;
using MVCWeb.Model.Models;

namespace MVCWeb.Model.ModelMapping
{
    public class NullUserMapping : EntityTypeConfiguration<NullUser>
    {
        public NullUserMapping()
        {
            ToTable("NullUser");
            HasKey(h => h.ID);
        }
    }
}
