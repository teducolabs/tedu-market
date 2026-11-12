using FluentValidation;
using Mediator;
using TeduMarket.Application.Common.Exceptions;

namespace TeduMarket.Application.Common.Behaviors;

/// <summary>Chạy mọi <see cref="IValidator{T}"/> của message trước Handler, gom lỗi theo trường.</summary>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IValidator<TMessage>[] _validators = [.. validators];

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Length == 0)
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);
        var results = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var errors = results
            .SelectMany(result => result.Errors)
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return await next(message, cancellationToken);
    }

    private static string ToCamelCase(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(segment =>
            segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
}
