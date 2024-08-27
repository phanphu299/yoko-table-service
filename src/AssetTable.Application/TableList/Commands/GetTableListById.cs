using System;
using AssetTable.Application.TableList.Command.Model;
using MediatR;

namespace AssetTable.Application.TableList.Command
{
    public class GetTableListById : IRequest<GetTableListDto>
    {
        public Guid Id { get; set; }
        public GetTableListById(Guid id)
        {
            Id = id;
        }
    }
}
