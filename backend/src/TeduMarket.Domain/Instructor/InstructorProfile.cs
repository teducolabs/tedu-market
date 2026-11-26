namespace TeduMarket.Domain.Instructor;

/// <summary>Hồ sơ Instructor, tạo khi Admin duyệt Instructor (FR-40); nội dung hiển thị do T22 bổ sung.</summary>
public sealed class InstructorProfile
{
    private InstructorProfile()
    {
    }

    public Guid InstructorProfileId { get; private set; }

    /// <summary>Id tài khoản <c>User</c> của Instructor; mỗi tài khoản có tối đa 1 hồ sơ.</summary>
    public Guid UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static InstructorProfile Create(Guid userId, DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new Common.DomainException("INVALID_INSTRUCTOR_USER", "Cần chọn người dùng để duyệt Instructor");
        }

        return new InstructorProfile { InstructorProfileId = Guid.CreateVersion7(), UserId = userId, CreatedAt = createdAt };
    }
}
