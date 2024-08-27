using System;
using AssetTable.Application.AssetTable.Command.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class GetTableById : IRequest<GetTableByIdDto>
    {
        public Guid Id { get; set; }

        public GetTableById(Guid id)
        {
            Id = id;
        }
    }
}
