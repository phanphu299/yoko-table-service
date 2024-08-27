using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using AssetTable.Application.Constant;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class ArchiveAssetTableDto
    {
        public IEnumerable<ArchiveTableDto> Tables { get; set; }
        public IEnumerable<ArchiveAssetDto> AssetTables { get; set; }
    }

    public class ArchiveTableDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
        public string Description { get; set; }
        public string Script { get; set; }
        public Guid? AssetId { get; set; }
        public bool Deleted { get; set; }
        public string AssetName { get; set; }
        public string ResourcePath { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public IEnumerable<ArchiveColumnDto> Columns { get; set; }

        static Func<Domain.Entity.Table, ArchiveTableDto> DtoConverter = DtoProjection.Compile();
        public static Expression<Func<Domain.Entity.Table, ArchiveTableDto>> DtoProjection
        {
            get
            {
                return (model) => new ArchiveTableDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    OldName = model.OldName,
                    Description = model.Description,
                    Script = model.Script,
                    AssetId = model.AssetId,
                    AssetName = model.AssetName,
                    Deleted = model.Deleted,
                    ResourcePath = model.ResourcePath,
                    CreatedUtc = model.CreatedUtc,
                    UpdatedUtc = model.UpdatedUtc,
                    Columns = model.Columns.Select(ArchiveColumnDto.CreateDto)
                };
            }
        }
        public static ArchiveTableDto CreateDto(Domain.Entity.Table model)
        {
            return DtoConverter(model);
        }

        static Func<ArchiveTableDto, string, Domain.Entity.Table> EntityConverter = EntityProjection.Compile();
        public static Expression<Func<ArchiveTableDto, string, Domain.Entity.Table>> EntityProjection
        {
            get
            {
                return (model, upn) => new Domain.Entity.Table
                {
                    Id = model.Id,
                    Name = model.Name,
                    OldName = model.OldName,
                    Script = model.Script,
                    AssetId = model.AssetId,
                    AssetName = model.AssetName,
                    Deleted = model.Deleted,
                    ResourcePath = model.ResourcePath,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow,
                    CreatedBy = upn,
                    AssetCreatedBy = upn,
                    Description = model.Description,
                    Columns = model.Columns.Select(ArchiveColumnDto.CreateEntity).ToList()
                };
            }
        }

        public static Domain.Entity.Table CreateEntity(ArchiveTableDto model, string upn)
        {
            return EntityConverter(model, upn);
        }
    }

    public class ArchiveColumnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public bool AllowNull { get; set; }
        public Guid TableId { get; set; }
        public int ColumnOrder { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }

        static Func<Domain.Entity.Column, ArchiveColumnDto> DtoConverter = DtoProjection.Compile();
        public static Expression<Func<Domain.Entity.Column, ArchiveColumnDto>> DtoProjection
        {
            get
            {
                return model => new ArchiveColumnDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    IsPrimary = model.IsPrimary,
                    TypeCode = model.TypeCode,
                    TypeName = model.TypeName,
                    DefaultValue = GetDefaultValue(model.TypeCode, model.DefaultValue),
                    AllowNull = model.AllowNull,
                    TableId = model.TableId,
                    ColumnOrder = model.ColumnOrder,
                    CreatedUtc = model.CreatedUtc,
                    UpdatedUtc = model.UpdatedUtc
                };
            }
        }
        public static ArchiveColumnDto CreateDto(Domain.Entity.Column model)
        {
            if (model != null)
            {
                return DtoConverter(model);
            }
            return null;
        }

        static Func<ArchiveColumnDto, Domain.Entity.Column> EntityConverter = EntityProjection.Compile();
        public static Expression<Func<ArchiveColumnDto, Domain.Entity.Column>> EntityProjection
        {
            get
            {
                return model => new Domain.Entity.Column
                {
                    Id = model.Id,
                    Name = model.Name,
                    IsPrimary = model.IsPrimary,
                    TypeCode = model.TypeCode,
                    TypeName = model.TypeName,
                    DefaultValue = GetDefaultValue(model.TypeCode, model.DefaultValue),
                    AllowNull = model.AllowNull,
                    TableId = model.TableId,
                    ColumnOrder = model.ColumnOrder,
                    CreatedUtc = model.CreatedUtc,
                    UpdatedUtc = model.UpdatedUtc
                };
            }
        }
        public static Domain.Entity.Column CreateEntity(ArchiveColumnDto model)
        {
            if (model != null)
            {
                return EntityConverter(model);
            }
            return null;
        }

        private static string GetDefaultValue(string typeCode, string defaultValue)
        {
            if (typeCode == PostgresDataTypeMapping.DATETIME
                && !DateTime.TryParseExact(defaultValue, AHI.Infrastructure.SharedKernel.Extension.Constant.DefaultDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                DateTime dateTime;
                return DateTime.TryParse(defaultValue, out dateTime) ? dateTime.ToString(AHI.Infrastructure.SharedKernel.Extension.Constant.DefaultDateTimeFormat) : null;
            }
            return defaultValue;
        }
    }

    public class ArchiveAssetDto
    {
        public string TableName { get; set; }
        public IEnumerable<dynamic> Data { get; set; }
    }
}
