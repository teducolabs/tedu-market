namespace TeduMarket.Domain.Common;

/// <summary>Chốt các thay đổi của phía ghi trong một transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
