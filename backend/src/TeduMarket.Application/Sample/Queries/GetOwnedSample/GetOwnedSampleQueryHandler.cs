using Mediator;
using TeduMarket.Application.Identity;

namespace TeduMarket.Application.Sample.Queries.GetOwnedSample;

public sealed class GetOwnedSampleQueryHandler(ICurrentUser currentUser) : IQueryHandler<GetOwnedSampleQuery, OwnedSampleDto>
{
    public ValueTask<OwnedSampleDto> Handle(GetOwnedSampleQuery query, CancellationToken cancellationToken)
    {
        OwnershipGuard.EnsureOwnerOrAdmin(currentUser, query.OwnerId);
        return ValueTask.FromResult(new OwnedSampleDto(query.OwnerId));
    }
}
