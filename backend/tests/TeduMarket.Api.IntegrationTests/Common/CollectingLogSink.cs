using Serilog.Core;
using Serilog.Events;

namespace TeduMarket.Api.IntegrationTests.Common;

/// <summary>Sink gom log event trong bộ nhớ để test kiểm tra nội dung log.</summary>
public sealed class CollectingLogSink : ILogEventSink
{
    private readonly List<LogEvent> _events = [];
    private readonly Lock _lock = new();

    public IReadOnlyList<LogEvent> Events
    {
        get
        {
            lock (_lock)
            {
                return [.. _events];
            }
        }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (_lock)
        {
            _events.Add(logEvent);
        }
    }
}
