using System;
using AssetTable.Application.TableList.Command.Model;
using MediatR;

namespace AssetTable.Application.TableList.Command
{
    public class FetchTable : IRequest<GetTableListDto>
    {
        public Guid Id { get; set; }
        public FetchTable(Guid id)
        {
            Id = id;
        }
    }
}