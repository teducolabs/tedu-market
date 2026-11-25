using TeduMarket.Domain.Identity;

namespace TeduMarket.Api.Common;

/// <summary>Policy theo vai trò cho endpoint (FR-05); kiểm tra sở hữu làm tiếp trong Handler (NFR-08).</summary>
public static class AuthorizationPolicies
{
    public const string Student = "Student";
    public const string Instructor = "Instructor";
    public const string Admin = "Admin";
    public const string InstructorOrAdmin = "InstructorOrAdmin";

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Student, policy => policy.RequireRole(RoleNames.Student))
            .AddPolicy(Instructor, policy => policy.RequireRole(RoleNames.Instructor))
            .AddPolicy(Admin, policy => policy.RequireRole(RoleNames.Admin))
            .AddPolicy(InstructorOrAdmin, policy => policy.RequireRole(RoleNames.Instructor, RoleNames.Admin));
        return services;
    }
}
