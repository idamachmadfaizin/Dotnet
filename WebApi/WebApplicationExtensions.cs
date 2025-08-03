using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using WebApi.Configurations;
using WebApi.Database.Context;

namespace WebApi;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiDocumentations(this WebApplication app)
    {
        var swaggerConfig = app.Services.GetRequiredService<IOptions<Swagger>>().Value;

        app.UseSwaggerGen();

        app.MapScalarApiReference(options =>
            {
                options
                    .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                    .WithFavicon("https://scalar.com/favicon.svg");

                if (!swaggerConfig.DocumentOptions.Any())
                    return;

                foreach (var documentOption in swaggerConfig.DocumentOptions)
                {
                    options.AddDocument(documentOption.DocumentSettings.DocumentName,
                        documentOption.DocumentSettings.Version);
                }
            })
            .AllowAnonymous();

        app.MapGet("/", () => Results.Redirect("/swagger"))
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }

    public static Task EnsureMigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Database.MigrateAsync();
    }
}