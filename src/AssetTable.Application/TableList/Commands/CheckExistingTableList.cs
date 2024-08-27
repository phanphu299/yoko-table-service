using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using System;
using System.Collections.Generic;

namespace AssetTable.Application.TableList.Command
{
    public class CheckExistingTableList : IRequest<BaseResponse>
    {
        public IEnumerable<Guid> Ids { get; set; }
        public CheckExistingTableList(IEnumerable<Guid> ids)
        {
            Ids = ids;
        }
    }
}
