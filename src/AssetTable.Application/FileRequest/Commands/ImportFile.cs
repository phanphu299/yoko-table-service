using System;
using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.FileRequest.Command
{
    public class ImportFile : IRequest<BaseResponse>
    {
        public Guid TableId { get; set; }
        public IEnumerable<string> FileNames { get; set; }
    }
}
