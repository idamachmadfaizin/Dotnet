using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Configurations;
using Database.Context;
using Database.Seeders;
using FastEndpoints.Security;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using Model.Entities;
using Serilog;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

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
            var signingKey = builder.Configuration[$"{nameof(Auth)}:{nameof(Auth.SigningKey)}"];
            ArgumentException.ThrowIfNullOrWhiteSpace(signingKey);
            o.SigningKey = signingKey;
        })
        .AddAuthorization()
        .AddFastEndpoints()
        .AddSwaggerDocuments()
        .AddResponseCaching()
        .AddDbContext<AppDbContext>()
        .AddIdentityApiEndpoints<User>()
        .AddEntityFrameworkStores<AppDbContext>();

    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var localization = builder.Configuration.GetSection(nameof(Localization)).Get<Localization>() ??
                           new Localization();
        var supportedCultures = localization.SupportedCultures
            .Select(culture => new CultureInfo(culture))
            .ToArray();

        options.DefaultRequestCulture = new RequestCulture(localization.DefaultCulture);
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

    var connectionString = builder.Configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection));
    builder.Services.AddHealthChecks()
        .AddApplicationStatus()
        .AddSqlite(connectionString ?? throw new InvalidOperationException());

    var app = builder.Build();

    app.UseRequestLocalization();
    app.UseDefaultExceptionHandler()
        .UseHttpsRedirection();
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

    app.UseFastEndpoints(config =>
    {
        config.Endpoints.RoutePrefix = "api";
        config.Versioning.Prefix = "v";
        config.Versioning.DefaultVersion = 1;
        config.Versioning.PrependToRoute = true;
        config.Errors.UseProblemDetails();
    });

    Log.Information("Server {Application} started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.Information("Shutting down server {Application}");
    Log.CloseAndFlush();
}