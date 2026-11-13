using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using TeduMarket.Api.Common;
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
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(ApiPolicies.CorsPolicy);
app.UseRateLimiter();
app.UseOutputCache();
app.MapOpenApi();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(TeduMarket.Infrastructure.DependencyInjection.ReadyTag) });

var api = app.MapGroup("/api/v1");
if (app.Environment.IsDevelopment())
{
    api.MapSampleEndpoints();
}

app.Run();

/// <summary>Điểm neo để Api.IntegrationTests dựng <c>WebApplicationFactory</c>.</summary>
public partial class Program;
