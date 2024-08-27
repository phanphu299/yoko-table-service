using AssetTable.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetTable.Persistence.Configuration
{
    public class ColumnConfiguration : IEntityTypeConfiguration<Column>
    {
        public void Configure(EntityTypeBuilder<Column> builder)
        {
            // configure the model.
            builder.ToTable("columns");
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Name).HasColumnName("name");
            builder.Property(e => e.IsPrimary).HasColumnName("is_primary");
            builder.Property(e => e.TypeCode).HasColumnName("type_code");
            builder.Property(e => e.TypeName).HasColumnName("type_name");
            builder.Property(e => e.DefaultValue).HasColumnName("default_value");
            builder.Property(e => e.AllowNull).HasColumnName("allow_null");
            builder.Property(e => e.TableId).HasColumnName("table_id");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(e => e.ColumnOrder).HasColumnName("column_order");
            builder.Property(e => e.IsSystemColumn).HasColumnName("is_system_column");
            builder.HasOne(x => x.Table).WithMany(x => x.Columns).HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
