using Mediator;

namespace TeduMarket.Application.Instructor.Queries.GetMyInstructorProfile;

/// <summary>Instructor xem Hồ sơ Instructor của chính mình (không cần biết Id hồ sơ).</summary>
public sealed record GetMyInstructorProfileQuery : IQuery<InstructorProfileDto>;
