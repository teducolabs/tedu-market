using FluentValidation;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Application.Identity.Queries.ListUsers;

public sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    private static readonly string[] _roles = [RoleNames.Student, RoleNames.Instructor, RoleNames.Admin];

    public ListUsersQueryValidator()
    {
        RuleFor(query => query.Role)
            .Must(role => role is null || _roles.Contains(role)).WithMessage("Vai trò không hợp lệ");
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải từ 1");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, ListUsersQuery.MaxPageSize).WithMessage($"Số dòng mỗi trang từ 1 đến {ListUsersQuery.MaxPageSize}");
    }
}
