using TeduMarket.Domain.Instructor;
using TeduMarket.Infrastructure.Persistence;

namespace TeduMarket.Infrastructure.Instructor;

internal sealed class InstructorProfileRepository(AppDbContext dbContext) : IInstructorProfileRepository
{
    public void Add(InstructorProfile instructorProfile) => dbContext.Set<InstructorProfile>().Add(instructorProfile);
}
