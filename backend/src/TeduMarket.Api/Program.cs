using TeduMarket.Api.Common;
using TeduMarket.Api.Sample;
using TeduMarket.Application;
using TeduMarket.Application.Common.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
});
builder.Services.AddApplicationValidators();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapOpenApi();

var api = app.MapGroup("/api/v1");
if (app.Environment.IsDevelopment())
{
    api.MapSampleEndpoints();
}

app.Run();

/// <summary>Điểm neo để Api.IntegrationTests dựng <c>WebApplicationFactory</c>.</summary>
public partial class Program;
