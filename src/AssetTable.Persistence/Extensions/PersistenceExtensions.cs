using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AssetTable.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using AssetTable.Persistence.Repository;
using AssetTable.Application.Repository;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.Service.Tag.Extension;

namespace AssetTable.Persistence.Extension
{
    public static class PersistenceExtensions
    {
        public static void AddPersistenceService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<TableDbContext>((service, option) =>
            {
                var configuration = service.GetRequiredService(typeof(IConfiguration)) as IConfiguration;
                var tenantContext = service.GetService(typeof(ITenantContext)) as ITenantContext;
                var connectionString = configuration["ConnectionStrings:Default"].BuildConnectionString(configuration, tenantContext.ProjectId);
                option.UseNpgsql(connectionString);

            });
            // add other services like repository, application services
            serviceCollection.AddScoped<ITableUnitOfWork, TableUnitOfWork>();
            serviceCollection.AddScoped<ITableRepository, TablePersistenceRepository>();
            serviceCollection.AddEntityTagRepository(typeof(TableDbContext));

            serviceCollection.AddMemoryCache();
        }
    }
}
