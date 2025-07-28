using System.Text.Json;
using System.Text.Json.Serialization;
using Configurations;
using Database.Context;
using Database.Seeders;
using FastEndpoints.Security;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Model.Entities;
using WebApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddConfigurations(builder.Configuration)
    .AddAuthenticationJwtBearer(o => o.SigningKey = builder.Configuration[$"{nameof(Auth)}:{nameof(Auth.SigningKey)}"])
    .AddAuthorization()
    .AddFastEndpoints()
    .AddSwaggerDocuments()
    .AddResponseCaching()
    .AddDbContext<AppDbContext>()
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<AppDbContext>();

var connectionString = builder.Configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection));
builder.Services.AddHealthChecks()
    .AddApplicationStatus()
    .AddSqlite(connectionString ?? throw new InvalidOperationException());

var app = builder.Build();

app
    .UseHsts()
    .UseHttpsRedirection()
    .UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    await app.EnsureMigrateAsync();
    app.UseDbSeed<DatabaseSeeder>(args);
    app.UseApiDocumentations();
}

app
    .UseAuthentication()
    .UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Storage", "App", "Public")),
    RequestPath = "/Storage",
});

app.MapGroup("api")
    .WithTags("Identity")
    .MapIdentityApi<User>();

var healthCheckJsonOptions = new JsonSerializerOptions
{
    Converters = { new JsonStringEnumConverter() },
};
app.MapHealthChecks("/healthz", new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(report, healthCheckJsonOptions));
    }
});

app
    .UseDefaultExceptionHandler()
    .UseFastEndpoints(config =>
    {
        config.Endpoints.RoutePrefix = "api";
        config.Versioning.Prefix = "v";
        config.Versioning.DefaultVersion = 1;
        config.Versioning.PrependToRoute = true;
        config.Errors.UseProblemDetails();
    });

app.Run();