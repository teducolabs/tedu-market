using Mediator;

namespace TeduMarket.Application.Instructor.Queries.GetInstructorProfile;

/// <summary>Chủ hồ sơ hoặc Admin xem một Hồ sơ Instructor.</summary>
public sealed record GetInstructorProfileQuery(Guid InstructorProfileId) : IQuery<InstructorProfileDto>;
