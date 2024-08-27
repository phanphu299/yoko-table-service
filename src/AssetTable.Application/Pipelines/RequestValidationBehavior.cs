using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using MediatR;

namespace AssetTable.Pipeline
{
    public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<FluentValidation.IValidator<TRequest>> _validators;

        public RequestValidationBehavior(IEnumerable<FluentValidation.IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var context = new FluentValidation.ValidationContext(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new EntityValidationException(failures);
            }

            return next();
        }
    }
}
