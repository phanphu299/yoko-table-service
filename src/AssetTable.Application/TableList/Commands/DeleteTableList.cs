using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using System;

namespace AssetTable.Application.TableList.Command
{
    public class DeleteTableList : IRequest<BaseResponse>
    {
        public Guid[] Ids { get; set; }
        public DeleteTableList(Guid[] ids)
        {
            Ids = ids;
        }
    }
}
