using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.UserContext.Abstraction;
using AssetTable.Application.Event;
using AssetTable.Application.Service.Abstraction;

namespace AssetTable.Application.Service
{
    public class FileEventService : IFileEventService
    {
        private readonly IDomainEventDispatcher _dispatcher;
        private readonly ITenantContext _tenantContext;
        private readonly IUserContext _userContext;

        public FileEventService(
            IDomainEventDispatcher serviceProvider,
            ITenantContext tenantContext,
            IUserContext userContext)
        {
            _dispatcher = serviceProvider;
            _tenantContext = tenantContext;
            _userContext = userContext;
        }

        public Task SendExportEventAsync(Guid activityId, Guid tableId, string tableName, string filter)
        {
            var exportEvent = new FileExportEvent(
                activityId,
                tableId,
                tableName,
                filter,
                _tenantContext,
                _userContext
            );
            return _dispatcher.SendAsync(exportEvent);
        }

        public Task SendImportEventAsync(Guid tableId, string tableName, IEnumerable<string> data)
        {
            var importEvent = new FileImportEvent(
                tableId,
                tableName,
                data,
                _tenantContext,
                _userContext
            );
            return _dispatcher.SendAsync(importEvent);
        }
    }
}
