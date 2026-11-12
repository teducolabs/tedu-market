using Mediator;

namespace TeduMarket.Application.Sample.Commands.CreateSample;

/// <summary>Command mẫu để kiểm chứng pipeline; thay bằng command nghiệp vụ thật khi có.</summary>
public sealed record CreateSampleCommand(string? Title) : ICommand<SampleDto>;
