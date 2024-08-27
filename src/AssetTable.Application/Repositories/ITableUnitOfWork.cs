using AHI.Infrastructure.Repository.Generic;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AssetTable.Domain.Entity;

namespace AssetTable.Application.Repository
{
    public interface ITableUnitOfWork : IUnitOfWork
    {
        ITableRepository Table { get; }
        IEntityTagRepository<EntityTagDb> EntityTag { get; }
    }
}