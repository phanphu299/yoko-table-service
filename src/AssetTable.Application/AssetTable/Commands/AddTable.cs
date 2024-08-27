using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AssetTable.Application.AssetTable.Command.Model;
using MediatR;
using AssetTable.Application.Constant;
using AHI.Infrastructure.Service.Tag.Model;

namespace AssetTable.Application.AssetTable.Command
{
    public class AddTable : UpsertTagCommand, IRequest<AddTableDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; }
        public string ResourcePath { get; set; }
        public IEnumerable<AddColumn> Columns { get; set; }
        public string AssetCreatedBy { get; set; }
        public string CreatedBy { get; set; }
        public static Expression<Func<AddTable, Domain.Entity.Table>> Projection
        {
            get
            {
                return command => new Domain.Entity.Table
                    (command.Columns.Select(x => AddColumn.Create(x)))
                {
                    Id = Guid.NewGuid(),
                    Name = command.Name,
                    Description = command.Description,
                    AssetId = command.AssetId,
                    AssetName = command.AssetName,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow,
                    CreatedBy = command.CreatedBy,
                    ResourcePath = command.ResourcePath,
                    AssetCreatedBy = command.AssetCreatedBy
                };
            }
        }

        static Func<AddTable, Domain.Entity.Table> Converter = Projection.Compile();

        public static Domain.Entity.Table Create(AddTable command)
        {
            var table = Converter(command);
            AddSystemColumn(table);
            return table;
        }

        public static void AddSystemColumn(Domain.Entity.Table table)
        {
            table.Columns.Add(new Domain.Entity.Column
            {
                Name = SystemColumn.CREATED_BY,
                IsPrimary = false,
                TypeCode = PostgresDataTypeMapping.VA255,
                DefaultValue = null,
                TableId = table.Id,
                AllowNull = false,
                ColumnOrder = 0,
                IsSystemColumn = true
            });

            table.Columns.Add(new Domain.Entity.Column
            {
                Name = SystemColumn.CREATED_UTC,
                IsPrimary = false,
                TypeCode = PostgresDataTypeMapping.DATETIME,
                DefaultValue = null,
                TableId = table.Id,
                AllowNull = false,
                ColumnOrder = 0,
                IsSystemColumn = true
            });

            table.Columns.Add(new Domain.Entity.Column
            {
                Name = SystemColumn.UPDATED_UTC,
                IsPrimary = false,
                TypeCode = PostgresDataTypeMapping.DATETIME,
                DefaultValue = null,
                TableId = table.Id,
                AllowNull = false,
                ColumnOrder = 0,
                IsSystemColumn = true
            });
        }
    }

    public class BaseColumn
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public bool AllowNull { get; set; }
        public string Action { get; set; } = ColumnAction.NO;
        public int Order { get; set; }
    }

    public class AddColumn : BaseColumn
    {
        public static Expression<Func<AddColumn, Domain.Entity.Column>> Projection
        {
            get
            {
                return command => new Domain.Entity.Column
                {
                    Name = command.Name,
                    IsPrimary = command.IsPrimary,
                    TypeCode = command.TypeCode,
                    TypeName = command.TypeName,
                    DefaultValue = command.DefaultValue,
                    CreatedUtc = DateTime.UtcNow,
                    ColumnOrder = command.Order,
                    AllowNull = command.AllowNull,
                    IsSystemColumn = false
                };
            }
        }

        public static Domain.Entity.Column Create(AddColumn command)
        {
            return Projection.Compile().Invoke(command);
        }
    }
}
