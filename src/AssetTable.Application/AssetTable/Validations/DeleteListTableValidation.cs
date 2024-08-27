using AssetTable.Application.AssetTable.Command;
using FluentValidation;
using AHI.Infrastructure.Exception;

namespace AssetTable.Application.AssetTable.Validation
{
    public class DeleteListTableValidation : AbstractValidator<DeleteListTable>
    {
        public DeleteListTableValidation()
        {
            RuleFor(x => x.Ids).NotEmpty().WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
        }
    }
}
