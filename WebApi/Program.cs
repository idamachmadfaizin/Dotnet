using Configurations;
using Database.Context;
using Database.Seeders;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Model.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthenticationJwtBearer(o => o.SigningKey = builder.Configuration[$"{nameof(Auth)}:{nameof(Auth.SigningKey)}"])
    .AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    })
    .AddFastEndpoints()
    .AddResponseCaching()
    .AddDbContext<AppDbContext>();

builder.Services
    .Configure<Auth>(builder.Configuration.GetSection(nameof(Auth)))
    .Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));

builder.Services
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

if (!builder.Environment.IsProduction())
    builder.Services.SwaggerDocument(o =>
    {
        o.DocumentSettings = settings =>
        {
            settings.DocumentName = "v0";
            settings.Version = "0.0.0";
        };
        o.FlattenSchema = true;
    });

var app = builder.Build();

app
    .UseHsts()
    .UseHttpsRedirection()
    .UseStaticFiles();

if (!app.Environment.IsProduction())
{
    app.UseDbSeed<DatabaseSeeder>(args);
    app.UseSwaggerGen(uiConfig: settings => settings.DeActivateTryItOut());
}

app
    .UseAuthentication()
    .UseAuthorization()
    .UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "Storage", "App", "Public")),
        RequestPath = "/Storage"
    })
    .UseDefaultExceptionHandler()
    .UseFastEndpoints(config => config.Errors.UseProblemDetails());

if (!app.Environment.IsProduction())
{
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.MapIdentityApi<User>();

app.Run();