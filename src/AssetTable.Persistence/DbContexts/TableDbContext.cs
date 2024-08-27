using AHI.Infrastructure.Service.Tag.PostgreSql.Configuration;
using AssetTable.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AssetTable.Persistence.Context
{
    public class TableDbContext : DbContext
    {
        public DbSet<Domain.Entity.Table> Tables { get; set; }
        public DbSet<Domain.Entity.EntityTagDb> EntityTags { get; set; }

        public TableDbContext(DbContextOptions<TableDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TableConfiguration());
            modelBuilder.ApplyConfiguration(new ColumnConfiguration());
            modelBuilder.ApplyConfiguration(new EntityTagConfiguration<Domain.Entity.EntityTagDb>());
        }
    }
}
