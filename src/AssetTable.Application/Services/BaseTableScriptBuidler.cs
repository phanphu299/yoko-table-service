using System.Linq;
using System.Collections.Generic;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.Exception.Helper;
using AssetTable.Domain.Entity;
using AssetTable.Application.AssetTable.Command.Model;

namespace AssetTable.Application.Service
{
    public class BaseTableScriptBuidler
    {
        private readonly TableDto _table;
        private IList<(string FieldName, string Message)> _errors;

        public BaseTableScriptBuidler(TableDto table)
        {
            _table = table;
            _errors = new List<(string FieldName, string Message)>();
        }

        protected void ValidateTable()
        {
            ValidateTableName();
            ValidatePrimaryKey();
            ValidateColumns();
        }

        private void ValidateTableName()
        {
            if (!_table.HasValidName())
            {
                throw ValidationExceptionHelper.GenerateInvalidValidation(nameof(Domain.Entity.Table.Name));
            }
        }

        private void ValidatePrimaryKey()
        {
            if (!_table.HasOnePrimaryKey())
            {
                _errors.Add((nameof(Column.IsPrimary), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED));
            }
        }

        private void ValidateColumns()
        {
            foreach (var column in _table.Columns.Where(col => !col.HasDeleted()))
            {
                if (!column.HasValidName())
                {
                    _errors.Add(($"Column.{nameof(Column.Name)}", ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID));
                    break;
                }

                if (!column.HasValidDataType())
                {
                    _errors.Add((nameof(Column.TypeCode), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID));
                    break;
                }

                if (!column.HasValidDefaultValue())
                {
                    _errors.Add((nameof(Column.DefaultValue), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID));
                    break;
                }
            }

            if (_errors.Any())
            {
                throw EntityValidationExceptionHelper.GenerateException(
                    _errors.Select(x => new FluentValidation.Results.ValidationFailure(x.FieldName, x.Message)).ToList()
                );
            }
        }
    }
}