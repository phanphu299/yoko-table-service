using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class VerifyAssetTable : IRequest<BaseResponse>
    {
        public string Data { get; set; }
    }
}
