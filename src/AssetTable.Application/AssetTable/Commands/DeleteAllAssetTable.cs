using System;
using System.Linq.Expressions;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace AssetTable.Application.AssetTable.Command
{
    public class DeleteAllAssetTable : IRequest<BaseResponse>
    {
        public Guid AssetId { get; set; }
        public bool WithTable { get; set; }
        public string Upn { get; set; }
        static Func<DeleteAllAssetTable, Domain.Entity.Table> Converter = Projection.Compile();
        public static Expression<Func<DeleteAllAssetTable, Domain.Entity.Table>> Projection
        {
            get
            {
                return model => new Domain.Entity.Table
                {
                    AssetId = model.AssetId,
                };
            }
        }
        public static Domain.Entity.Table Create(DeleteAllAssetTable model)
        {
            if (model != null)
            {
                return Converter(model);
            }
            return null;
        }
    }
}
