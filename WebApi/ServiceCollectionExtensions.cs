using Configurations;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurations(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services
            .Configure<Auth>(configuration.GetSection(nameof(Auth)))
            .Configure<IdentityOptions>(configuration.GetSection($"{nameof(Auth)}:{nameof(Auth.IdentityOptions)}"))
            .Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)))
            .Configure<Swagger>(configuration.GetSection(nameof(Swagger)));

        return services;
    }

    public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var swaggerConfig = provider.GetRequiredService<IOptions<Swagger>>().Value;

        if (provider.GetRequiredService<IWebHostEnvironment>().IsDevelopment() != swaggerConfig.IsDevelopment)
            return services;

        if (!swaggerConfig.DocumentOptions.Any())
        {
            services.SwaggerDocument();

            return services;
        }

        foreach (var documentOption in swaggerConfig.DocumentOptions)
        {
            services.SwaggerDocument(options =>
            {
                options.AutoTagPathSegmentIndex = documentOption.AutoTagPathSegmentIndex;
                options.DocumentSettings = s =>
                {
                    s.DocumentName = documentOption.DocumentSettings.DocumentName;
                    s.Version = documentOption.DocumentSettings.Version;
                };
                options.EnableGetRequestsWithBody = documentOption.EnableGetRequestsWithBody;
                options.EnableJWTBearerAuth = documentOption.EnableJWTBearerAuth;
                options.ExcludeNonFastEndpoints = documentOption.ExcludeNonFastEndpoints;
                options.FlattenSchema = documentOption.FlattenSchema;
                options.MaxEndpointVersion = documentOption.MaxEndpointVersion;
                options.MinEndpointVersion = documentOption.MinEndpointVersion;
                options.ReleaseVersion = documentOption.ReleaseVersion;
                options.ShowDeprecatedOps = documentOption.ShowDeprecatedOps;
                options.RemoveEmptyRequestSchema = documentOption.RemoveEmptyRequestSchema;
                options.ShortSchemaNames = documentOption.ShortSchemaNames;
                options.TagCase = documentOption.TagCase;
                options.TagStripSymbols = documentOption.TagStripSymbols;
                options.TagDescriptions = s =>
                {
                    if (documentOption.TagDescriptions is null) return;
                    
                    foreach (var (key, value) in documentOption.TagDescriptions)
                        s[key] = value;
                };
                options.UsePropertyNamingPolicy = documentOption.UsePropertyNamingPolicy;
                options.UseOneOfForPolymorphism = documentOption.UseOneOfForPolymorphism;
            });
        }

        return services;
    }
}