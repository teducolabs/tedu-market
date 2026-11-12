using Mediator;

namespace TeduMarket.Application.Sample.Commands.CreateSample;

public sealed class CreateSampleCommandHandler : ICommandHandler<CreateSampleCommand, SampleDto>
{
    public ValueTask<SampleDto> Handle(CreateSampleCommand command, CancellationToken cancellationToken) =>
        ValueTask.FromResult(new SampleDto(command.Title!));
}
