using Mediator;

namespace TeduMarket.Application.Course.Queries.ListLessons;

/// <summary>Admin hoặc Instructor sở hữu khóa xem danh sách Bài học theo thứ tự kèm cờ xem thử (FR-20, FR-45).</summary>
public sealed record ListLessonsQuery(Guid CourseId) : IQuery<IReadOnlyList<LessonDto>>;
