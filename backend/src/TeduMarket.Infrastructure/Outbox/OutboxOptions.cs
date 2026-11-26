namespace TeduMarket.Infrastructure.Outbox;

/// <summary>Cấu hình mục <c>Outbox</c>.</summary>
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>Tắt để không chạy vòng thăm dò (test cần chạy tay từng lô).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Chu kỳ thăm dò (~2 giây để đạt NFR-02).</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Số message tối đa mỗi lô.</summary>
    public int BatchSize { get; set; } = 20;
}
