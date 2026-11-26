namespace TeduMarket.Application.Instructor;

/// <summary>Cổng đọc phía Application; Infrastructure cài đặt bằng Dapper.</summary>
public interface IInstructorProfileReader
{
    Task<InstructorProfileDto?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken);

    Task<InstructorProfileDto?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
