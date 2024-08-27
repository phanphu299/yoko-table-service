using AssetTable.Application.AssetTable.Command;
using FluentValidation;
using AHI.Infrastructure.Exception;
using AssetTable.Application.Constant;
using System.Linq;
using AHI.Infrastructure.Service.Tag.Model;

namespace AssetTable.Application.AssetTable.Validation
{
    public class UpdateTableValidation : AbstractValidator<UpdateTable>
    {
        public UpdateTableValidation()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
            RuleFor(x => x.Name).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
            RuleFor(x => x.AssetId).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
            RuleFor(x => x.Columns).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
            RuleFor(x => x.Columns).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED)
                                    .ForEach(col =>
                                    {
                                        col.Must(c => !SystemColumn.ALL_COLUMNS.Any(x => x.Equals(c.Name.ToLower())))
                                            .WithName("Name").WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
                                    });

            RuleForEach(x => x.Tags).SetValidator(
                new InlineValidator<UpsertTag> {
                    agValidator => agValidator.RuleFor(x => x.Key)
                                              .NotEmpty()
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED)
                                              .MaximumLength(216)
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_MAX_LENGTH)
                                              .Must(ContainsInvalidChar)
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID),
                    agValidator => agValidator.RuleFor(x => x.Value)
                                              .NotEmpty()
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED)
                                              .MaximumLength(216)
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_MAX_LENGTH)
                                              .Must(ContainsInvalidChar)
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID)
                }
            );
        }

        private bool ContainsInvalidChar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;
            return !input.Contains(':') && !input.Contains(';') && !input.Contains(',');
        }
    }
}
