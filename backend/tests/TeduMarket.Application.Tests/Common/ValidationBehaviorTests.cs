using FluentValidation;
using Mediator;
using TeduMarket.Application.Common.Behaviors;
using TeduMarket.Application.Common.Exceptions;

namespace TeduMarket.Application.Tests.Common;

public sealed class ValidationBehaviorTests
{
    private sealed record TestMessage(string? Name, int Age) : IMessage;

    private sealed class NameValidator : AbstractValidator<TestMessage>
    {
        public NameValidator() => RuleFor(message => message.Name).NotEmpty().WithMessage("Tên trống");
    }

    private sealed class AgeValidator : AbstractValidator<TestMessage>
    {
        public AgeValidator() => RuleFor(message => message.Age).GreaterThan(0).WithMessage("Tuổi sai");
    }

    private static ValueTask<string> Next(TestMessage message, CancellationToken cancellationToken) =>
        ValueTask.FromResult("ok");

    [Fact]
    public async Task Handle_NoValidators_CallsNextAsync()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestMessage, string>([]);

        // Act
        var result = await behavior.Handle(new TestMessage(null, 0), Next, CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ValidMessage_CallsNextAsync()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestMessage, string>([new NameValidator(), new AgeValidator()]);

        // Act
        var result = await behavior.Handle(new TestMessage("An", 20), Next, CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_InvalidFieldsAcrossValidators_ThrowsWithCamelCaseKeysAsync()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestMessage, string>([new NameValidator(), new AgeValidator()]);

        // Act
        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            async () => await behavior.Handle(new TestMessage("", 0), Next, CancellationToken.None));

        // Assert
        Assert.Equal(["Tên trống"], exception.Errors["name"]);
        Assert.Equal(["Tuổi sai"], exception.Errors["age"]);
    }
}
