using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AssetTable.Application.Constant;
using AssetTable.Application.Extension;

namespace AssetTable.Application.AssetTable.Command.Model
{
    public class TableDto
    {
        static Func<Domain.Entity.Table, bool, TableDto> Converter = Projection.Compile();
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string OldName { get; private set; }
        public Guid? AssetId { get; private set; }
        public bool HasData { get; set; }
        public string ResourcePath { get; set; }
        public string CreatedBy { get; set; }
        public IEnumerable<ColumnDto> Columns { get; private set; }

        public static Expression<Func<Domain.Entity.Table, bool, TableDto>> Projection
        {
            get
            {
                return (model, hasData) => new TableDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    OldName = model.OldName,
                    AssetId = model.AssetId,
                    HasData = hasData,
                    ResourcePath = model.ResourcePath,
                    CreatedBy = model.CreatedBy,
                    Columns = model.Columns.Select(ColumnDto.Create)
                };
            }
        }
        static Func<UpdateTable, TableDto> UpdateConverter = ProjectionUpdateTable.Compile();
        public static Expression<Func<UpdateTable, TableDto>> ProjectionUpdateTable
        {
            get
            {
                return model => new TableDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    AssetId = model.AssetId,
                    Columns = model.Columns.Select(ColumnDto.CreateUpdateColumn)
                };
            }
        }

        public static TableDto Create(Domain.Entity.Table model, bool hasData = false)
        {
            if (model != null)
            {
                return Converter(model, hasData);
            }
            return null;
        }

        public static TableDto CreateUpdateTable(UpdateTable model)
        {
            if (model != null)
            {
                var table = UpdateConverter(model);
                AddSystemColumn(table);
                return table;
            }
            return null;
        }

        public static void AddSystemColumn(TableDto table)
        {
            var columnList = table.Columns.ToList();
            columnList.Add(ColumnDto.CreateUpdateColumn(new BaseColumn
            {
                Name = SystemColumn.CREATED_BY,
                IsPrimary = false,
                TypeCode = PostgresDataTypeMapping.VA255,
                DefaultValue = null,
                AllowNull = false,
                Order = 0
            }));

            columnList.Add(ColumnDto.CreateUpdateColumn(new BaseColumn
            {
                Name = SystemColumn.CREATED_UTC,
                IsPrimary = false,
                TypeCode = PostgresDataTypeMapping.DATETIME,
                DefaultValue = null,
                AllowNull = false,
                Order = 0,
            }));

            columnList.Add(ColumnDto.CreateUpdateColumn(new BaseColumn
            {
                Name = SystemColumn.UPDATED_UTC,
                IsPrimary = false,
                TypeCode = PostgresDataTypeMapping.DATETIME,
                DefaultValue = null,
                AllowNull = false,
                Order = 0,
            }));

            table.Columns = columnList;
        }

        public bool HasValidName()
        {
            return Name.IsValidName();
        }

        public bool HasOnePrimaryKey()
        {
            return Columns.Count(x => x.IsPrimary) == 1;
        }
    }

    public class ColumnDto
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsPrimary { get; private set; }
        public string TypeCode { get; private set; }
        public string DefaultValue { get; private set; }
        public bool AllowNull { get; private set; }
        public string Action { get; private set; }
        public string TempName => $"{Name}_temp";

        static Func<Domain.Entity.Column, ColumnDto> Converter = Projection.Compile();
        public static Expression<Func<Domain.Entity.Column, ColumnDto>> Projection
        {
            get
            {
                return model => new ColumnDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    IsPrimary = model.IsPrimary,
                    TypeCode = model.TypeCode,
                    DefaultValue = model.DefaultValue,
                    AllowNull = model.AllowNull,
                    Action = ColumnAction.NO
                };
            }
        }
        static Func<BaseColumn, ColumnDto> UpdateConverter = ProjectionUpdateColumn.Compile();
        public static Expression<Func<BaseColumn, ColumnDto>> ProjectionUpdateColumn
        {
            get
            {
                return model => new ColumnDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    IsPrimary = model.IsPrimary,
                    TypeCode = model.TypeCode,
                    DefaultValue = model.DefaultValue,
                    AllowNull = model.AllowNull,
                    Action = !string.IsNullOrEmpty(model.Action) ? model.Action : ColumnAction.NO
                };
            }
        }

        public static ColumnDto Create(Domain.Entity.Column model)
        {
            if (model != null)
            {
                return Converter(model);
            }
            return null;
        }

        public static ColumnDto CreateUpdateColumn(BaseColumn model)
        {
            if (model != null)
            {
                return UpdateConverter(model);
            }
            return null;
        }

        public bool EqualsName(ColumnDto targetColumn)
        {
            if (targetColumn == null)
                return false;

            return Name == targetColumn.Name;
        }

        public bool EqualsTypeCode(ColumnDto targetColumn)
        {
            if (targetColumn == null)
                return false;

            return TypeCode == targetColumn.TypeCode;
        }

        public bool EqualsDefaultValue(ColumnDto targetColumn)
        {
            if (targetColumn == null)
                return false;

            var defaultRequestColumnValue = !string.IsNullOrEmpty(DefaultValue) ? DefaultValue : null;
            var defaultTargetColumnValue = !string.IsNullOrEmpty(targetColumn.DefaultValue) ? targetColumn.DefaultValue : null;

            return DefaultValue == targetColumn.DefaultValue;
        }

        public bool EqualsAllowNull(ColumnDto targetColumn)
        {
            if (targetColumn == null)
                return false;

            return AllowNull == targetColumn.AllowNull;
        }

        public bool HasAdded()
        {
            return Action.IsAddAction();
        }

        public bool HasUpdated()
        {
            return Action.IsUpdateAction();
        }

        public bool HasDeleted()
        {
            return Action.IsDeleteAction();
        }

        public bool HasSpecialDataType()
        {
            return TypeCode.IsSpecialType();
        }

        public bool HasValidName()
        {
            return Name.IsValidName();
        }

        public bool HasValidDataType()
        {
            return TypeCode.IsValidTypeCode();
        }

        public bool HasValidDefaultValue()
        {
            return TypeCode.IsValidDefaultValue(DefaultValue);
        }
    }
}
