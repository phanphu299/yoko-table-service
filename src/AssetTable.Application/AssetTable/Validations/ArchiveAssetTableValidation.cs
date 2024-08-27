using FluentValidation;
using AHI.Infrastructure.Exception;
using AssetTable.Application.AssetTable.Command.Model;
using System;

namespace AssetTable.Application.AssetTable.Validation
{
    public class ArchiveAssetTableValidation : AbstractValidator<ArchiveAssetTableDto>
    {
        public ArchiveAssetTableValidation()
        {
            RuleForEach(x => x.Tables).ChildRules(x =>
            {
                x.RuleFor(x => x.Id).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                x.RuleFor(x => x.Name).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                x.RuleFor(x => x.AssetId)
                    .Must(x => x != Guid.Empty).WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID)
                    .When(x => x != null);
                x.RuleFor(x => x.Columns).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                x.RuleForEach(x => x.Columns).ChildRules(x =>
                {
                    x.RuleFor(c => c.Name).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                    x.RuleFor(c => c.TypeCode).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                    x.RuleFor(c => c.TableId).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                });
            });

            RuleForEach(x => x.AssetTables).ChildRules(x =>
            {
                x.RuleFor(x => x.TableName).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
                x.RuleFor(x => x.Data).NotNull().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
            });
        }
    }
}
