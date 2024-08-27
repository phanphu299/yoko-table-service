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
    public class UpdateTable : UpsertTagCommand, IRequest<UpdateTableDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid AssetId { get; set; }
        public List<BaseColumn> Columns { get; set; }

        public static readonly Func<UpdateTable, Domain.Entity.Table> Converter = Projection.Compile();
        public static Expression<Func<UpdateTable, Domain.Entity.Table>> Projection
        {
            get
            {
                return command => new Domain.Entity.Table
                    (command.Columns.Select(x => UpdateColumn.Create(command.Id, x)))
                {
                    Id = command.Id,
                    Name = command.Name,
                    Description = command.Description,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                };
            }
        }

        public static Domain.Entity.Table Create(UpdateTable command)
        {
            return Converter(command);
        }

        /// <summary>
        /// Get all columns are marked as action "add"
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseColumn> GetAddColumns()
        {
            var addColumns = Columns.Where(x => x.Action == ColumnAction.ADD);
            return addColumns;
        }

        /// <summary>
        /// Get all columns are marked as action "update"
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseColumn> GetUpdateColumns()
        {
            var updateColumns = Columns.Where(x => x.Action == ColumnAction.UPDATE);
            return updateColumns;
        }

        /// <summary>
        /// Get all columns are marked as action "delete"
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseColumn> GetDeleteColumns()
        {
            var deleteColumns = Columns.Where(x => x.Action == ColumnAction.DELETE);
            return deleteColumns;
        }
    }

    public class UpdateColumn : BaseColumn
    {
        public static readonly Func<Guid, BaseColumn, Domain.Entity.Column> Converter = Projection.Compile();
        public static Expression<Func<Guid, BaseColumn, Domain.Entity.Column>> Projection
        {
            get
            {
                return (tableId, command) => new Domain.Entity.Column
                {
                    TableId = tableId,
                    Id = command.Action != ColumnAction.ADD ? command.Id : 0,
                    Name = command.Name,
                    IsPrimary = command.IsPrimary,
                    TypeCode = command.TypeCode,
                    TypeName = command.TypeName,
                    DefaultValue = command.DefaultValue ?? string.Empty,
                    ColumnOrder = command.Order,
                    AllowNull = command.AllowNull
                };
            }
        }

        public static Domain.Entity.Column Create(Guid tableId, BaseColumn command)
        {
            return Converter(tableId, command);
        }
    }
}
