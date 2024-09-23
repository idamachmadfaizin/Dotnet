using Configurations;
using Database.Context;
using Database.Seeders;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Model.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthenticationJwtBearer(o => o.SigningKey = builder.Configuration[$"{nameof(Auth)}:{nameof(Auth.SigningKey)}"])
    .AddAuthorization()
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
app.UseAuthentication()
    .UseAuthorization()
    .UseDefaultExceptionHandler()
    .UseFastEndpoints(config => config.Errors.UseProblemDetails());

if (!app.Environment.IsProduction())
{
    app.UseDbSeed<DatabaseSeeder>(args)
        .UseSwaggerGen(uiConfig: settings => settings.DeActivateTryItOut());

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.MapIdentityApi<User>();

app.Run();