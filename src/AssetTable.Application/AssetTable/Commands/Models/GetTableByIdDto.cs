using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AHI.Infrastructure.Service.Tag.Model;
using AHI.Infrastructure.Service.Tag.Extension;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class GetTableByIdDto : TagDtos
    {
        static Func<Domain.Entity.Table, GetTableByIdDto> Converter = Projection.Compile();
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<GetColumnByIdDto> Columns { get; set; }
        public Guid? AssetId { get; set; }
        public Guid? RootAssetId { get; set; }
        public string AssetName { get; set; }
        public static Expression<Func<Domain.Entity.Table, GetTableByIdDto>> Projection
        {
            get
            {
                return model => new GetTableByIdDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Columns = model.Columns.Select(GetColumnByIdDto.Create).OrderBy(x => x.Order),
                    AssetId = model.AssetId,
                    AssetName = model.AssetName,
                    Tags = model.EntityTags.MappingTagDto()
                };
            }
        }

        public static GetTableByIdDto Create(Domain.Entity.Table model)
        {
            return Converter(model);
        }
    }

    public class GetColumnByIdDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public int Order { get; set; }
        public bool AllowNull { get; set; }
        public bool IsSystemColumn { get; set; }

        static Func<Domain.Entity.Column, GetColumnByIdDto> Converter = Projection.Compile();
        public static Expression<Func<Domain.Entity.Column, GetColumnByIdDto>> Projection
        {
            get
            {
                return model => new GetColumnByIdDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    IsPrimary = model.IsPrimary,
                    TypeCode = model.TypeCode,
                    TypeName = model.TypeName,
                    DefaultValue = model.DefaultValue,
                    Order = model.ColumnOrder,
                    AllowNull = model.AllowNull,
                    IsSystemColumn = model.IsSystemColumn
                };
            }
        }

        public static GetColumnByIdDto Create(Domain.Entity.Column model)
        {
            return Converter(model);
        }
    }
}
