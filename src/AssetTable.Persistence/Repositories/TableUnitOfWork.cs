using AHI.Infrastructure.Repository;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AssetTable.Application.Repository;
using AssetTable.Domain.Entity;
using AssetTable.Persistence.Context;

namespace AssetTable.Persistence.Repository
{
    public class TableUnitOfWork : BaseUnitOfWork, ITableUnitOfWork
    {
        private TableDbContext _dbContext;

        public ITableRepository Table { get; }
        public IEntityTagRepository<EntityTagDb> EntityTag { get; }

        public TableUnitOfWork(
            TableDbContext context,
            ITableRepository tableRepository,
            IEntityTagRepository<EntityTagDb> entityTagRepository) : base(context)
        {
            _dbContext = context;
            Table = tableRepository;
            EntityTag = entityTagRepository;
        }
    }
}