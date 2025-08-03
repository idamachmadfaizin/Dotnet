using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints.Security;
using Microsoft.Extensions.FileProviders;
using Serilog;
using WebApi;
using WebApi.App.Model.Entities;
using WebApi.Configurations;
using WebApi.Database.Context;
using WebApi.Database.Seeders;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

// ReSharper disable once StructuredMessageTemplateProblem
Log.Information("Starting Server {Application}");

try
{
    builder.Services
        .AddSerilog((services, config) => config
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services))
        .AddConfigurations(builder.Configuration)
        .AddAuthenticationJwtBearer(o =>
        {
            var signingKey = builder.Configuration.GetRequiredSection(nameof(Auth)).Get<Auth>()?.SigningKey;
            ArgumentException.ThrowIfNullOrWhiteSpace(signingKey);
            o.SigningKey = signingKey;
        })
        .AddAuthorization()
        .AddFastEndpoints()
        .AddResponseCaching()
        .AddLocalizationAndConfigure(builder.Configuration)
        .AddAppCors(builder.Configuration)
        .AddAppHealthChecks(builder.Configuration)
        .AddDbContext<AppDbContext>()
        .AddIdentityApiEndpoints<User>()
        .AddEntityFrameworkStores<AppDbContext>();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddSwaggerDocuments(builder.Configuration);
    }

    var app = builder.Build();

    app.UseRequestLocalization()
        .UseDefaultExceptionHandler()
        .UseHttpsRedirection()
        .UseCors();
    if (app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseSerilogRequestLogging()
        .UseStaticFiles()
        .UseResponseCaching();

    if (app.Environment.IsDevelopment())
    {
        await app.EnsureMigrateAsync();
        app.UseDbSeed<DatabaseSeeder>(args);
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

    app.UseFastEndpoints(config =>
    {
        config.Endpoints.RoutePrefix = "api";
        config.Versioning.Prefix = "v";
        config.Versioning.DefaultVersion = 1;
        config.Versioning.PrependToRoute = true;
        config.Errors.UseProblemDetails();
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseApiDocumentations();
    }

    // ReSharper disable once StructuredMessageTemplateProblem
    Log.Information("Server {Application} started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
    // ReSharper disable once StructuredMessageTemplateProblem
    Log.Information("Shutting down server {Application}");
    Log.CloseAndFlush();
}