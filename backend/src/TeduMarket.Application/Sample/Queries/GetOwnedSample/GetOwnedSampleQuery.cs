using Mediator;

namespace TeduMarket.Application.Sample.Queries.GetOwnedSample;

/// <summary>Truy vấn mẫu minh họa kiểm tra sở hữu: <paramref name="OwnerId"/> là chủ của tài nguyên giả định.</summary>
public sealed record GetOwnedSampleQuery(Guid OwnerId) : IQuery<OwnedSampleDto>;
