using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetTable.Persistence.Configuration
{
    public class TableConfiguration : IEntityTypeConfiguration<Domain.Entity.Table>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.Table> builder)
        {
            // configure the model.
            builder.ToTable("tables");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Name).HasColumnName("name");
            builder.Property(e => e.OldName).HasColumnName("old_name");
            builder.Property(e => e.Description).HasColumnName("description");
            builder.Property(e => e.Script).HasColumnName("script");
            builder.Property(e => e.AssetId).HasColumnName("asset_id");
            builder.Property(e => e.AssetName).HasColumnName("asset_name");
            builder.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            builder.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(e => e.Deleted).HasColumnName("deleted");
            builder.Property(e => e.CreatedBy).HasColumnName("created_by");
            builder.Property(e => e.AssetCreatedBy).HasColumnName("asset_created_by");
            builder.Property(e => e.ResourcePath).HasColumnName("resource_path");
            builder.HasMany(x => x.Columns).WithOne(x => x.Table).HasForeignKey(x => x.TableId);
            builder.HasMany(x => x.EntityTags).WithOne(x => x.Table).HasForeignKey(x => x.EntityIdGuid).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
