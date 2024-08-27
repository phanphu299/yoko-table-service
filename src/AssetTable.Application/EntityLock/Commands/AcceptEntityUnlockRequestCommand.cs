using MediatR;
using AHI.Infrastructure.SharedKernel.Model;
namespace AssetTable.Application.EntityLock.Command
{
    public class AcceptEntityUnlockRequestCommand : BaseEntityLock, IRequest<BaseResponse>
    {
    }
}
