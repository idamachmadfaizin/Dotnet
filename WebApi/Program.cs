using Configurations;
using Database.Context;
using Database.Seeders;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Model.Entities;
using WebApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddConfigurations(builder.Configuration)
    .AddAuthenticationJwtBearer(o => o.SigningKey = builder.Configuration[$"{nameof(Auth)}:{nameof(Auth.SigningKey)}"])
    .AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    })
    .AddFastEndpoints()
    .AddSwaggerDocuments()
    .AddResponseCaching()
    .AddDbContext<AppDbContext>()
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app
    .UseHsts()
    .UseHttpsRedirection()
    .UseStaticFiles();

if (app.Environment.IsDevelopment())
{
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

app.MapIdentityApi<User>();

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