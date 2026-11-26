using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using TeduMarket.Api.Common;
using TeduMarket.Api.Identity;
using TeduMarket.Api.Instructor;
using TeduMarket.Api.Sample;
using TeduMarket.Application;
using TeduMarket.Application.Common.Behaviors;
using TeduMarket.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) =>
{
    logger
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
    var seqServerUrl = context.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        logger.WriteTo.Seq(seqServerUrl, apiKey: context.Configuration["Seq:ApiKey"], formatProvider: CultureInfo.InvariantCulture);
    }
});

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
});
builder.Services.AddApplicationValidators();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddApiPolicies(builder.Configuration);
builder.Services.AddIdentityServer(builder.Configuration, builder.Environment);
builder.Services.AddOpenApi();
// API chỉ nhận request qua Nginx trong mạng compose nên tin header X-Forwarded-* từ proxy đó (OpenIddict cần biết scheme https).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging(options => options.Logger = app.Services.GetRequiredService<Serilog.ILogger>());
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(ApiPolicies.CorsPolicy);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();
app.MapOpenApi();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(TeduMarket.Infrastructure.DependencyInjection.ReadyTag) });

app.MapConnectEndpoints();
app.MapLoginPage();
app.MapGoogleLogin();

var api = app.MapGroup("/api/v1");
api.MapAccountEndpoints();
api.MapInstructorApprovalEndpoints();
api.MapInstructorProfileEndpoints();
if (app.Environment.IsDevelopment())
{
    api.MapSampleEndpoints();
}

app.Run();

/// <summary>Điểm neo để Api.IntegrationTests dựng <c>WebApplicationFactory</c>.</summary>
public partial class Program;
