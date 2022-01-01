using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Vetrina.Shared;

namespace Vetrina.Server.Mediatr.Pipelines
{
    public class CommandsValidationPipeline<TRequest, TResponse> 
        : IPipelineBehavior<TRequest, TResponse>
        where TResponse : CommandResult
    {
        private readonly ILogger<CommandsValidationPipeline<TRequest, TResponse>> logger;
        private readonly IValidator<TRequest> requestValidator;

        public CommandsValidationPipeline(
            ILogger<CommandsValidationPipeline<TRequest, TResponse>> logger,
            IValidator<TRequest> requestValidator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            logger.LogDebug($"Validating '{request.GetType().Name}'...");

            var validationResult = await requestValidator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid)
            {
                return await next();
            }

            logger.LogWarning($"Validation failed for '{request.GetType().Name}'.");

            validationResult.Errors.ToList().ForEach(vf =>
                logger.LogWarning($"Property: '{vf.PropertyName}' -- Message: '{vf.ErrorMessage}'"));

            var responseType = (TResponse) Activator.CreateInstance(typeof(TResponse), validationResult.Errors);

            return responseType;
        }
    }
}
