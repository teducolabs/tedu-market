namespace TeduMarket.Domain.Instructor;

public interface IInstructorProfileRepository
{
    void Add(InstructorProfile instructorProfile);

    Task<InstructorProfile?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken);

    void Remove(InstructorProfile instructorProfile);
}
