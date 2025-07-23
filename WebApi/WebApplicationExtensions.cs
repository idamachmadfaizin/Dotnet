using Configurations;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace WebApi;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseApiDocumentations(this WebApplication app)
    {
        var swaggerConfig = app.Services.GetRequiredService<IOptions<Swagger>>().Value;
        
        if (app.Services.GetRequiredService<IWebHostEnvironment>().IsDevelopment() != swaggerConfig.IsDevelopment)
            return app;
        
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
                    options.AddDocument(documentOption.DocumentSettings.DocumentName, documentOption.DocumentSettings.Version);
                }
            })
            .AllowAnonymous();
        
        app.MapGet("/", () => Results.Redirect("/swagger"))
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }
}