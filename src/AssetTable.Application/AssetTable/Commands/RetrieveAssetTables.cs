using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class RetrieveAssetTable : IRequest<BaseResponse>
    {
        public string Data { get; set; }
        public string Upn { get; set; }
    }
}
