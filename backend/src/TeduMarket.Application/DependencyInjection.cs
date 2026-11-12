using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TeduMarket.Application;

public static class DependencyInjection
{
    /// <summary>Đăng ký mọi validator trong assembly Application. Mediator đăng ký ở Api (source generator).</summary>
    public static IServiceCollection AddApplicationValidators(this IServiceCollection services) =>
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>(includeInternalTypes: true);
}
