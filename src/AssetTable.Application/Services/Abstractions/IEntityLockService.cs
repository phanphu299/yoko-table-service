using System;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;
using AssetTable.Application.Command.Model;
using AssetTable.Application.EntityLock.Command;

namespace AssetTable.Application.Service.Abstraction
{
    public interface IEntityLockService
    {
        Task<bool> ValidateEntitiesLockedByOtherAsync(ValidateLockEntitiesCommand command, CancellationToken token);
        Task<BaseResponse> AcceptEntityUnlockRequestAsync(AcceptEntityUnlockRequestCommand command, CancellationToken token);
        Task<EntityLockDto> GetLockEntityAsync(Guid entityId, CancellationToken token);
    }
}
