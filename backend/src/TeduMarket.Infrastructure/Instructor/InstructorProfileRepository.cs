using Microsoft.EntityFrameworkCore;
using TeduMarket.Domain.Instructor;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Instructor;

internal sealed class InstructorProfileRepository(AppDbContext dbContext) : IInstructorProfileRepository
{
    public void Add(InstructorProfile instructorProfile) => dbContext.Set<InstructorProfile>().Add(instructorProfile);

    public Task<InstructorProfile?> FindAsync(Guid instructorProfileId, CancellationToken cancellationToken) =>
        dbContext.Set<InstructorProfile>().SingleOrDefaultAsync(
            instructorProfile => instructorProfile.InstructorProfileId == instructorProfileId, cancellationToken);

    public void Remove(InstructorProfile instructorProfile) => dbContext.Set<InstructorProfile>().Remove(instructorProfile);
}
