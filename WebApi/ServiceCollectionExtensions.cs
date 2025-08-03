using System.Globalization;
using Configurations;
using FastEndpoints.Swagger;
using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using OpenApiExample = NSwag.OpenApiExample;
using OpenApiParameter = NSwag.OpenApiParameter;

namespace WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurations(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services
            .Configure<Auth>(configuration.GetSection(nameof(Auth)))
            .Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)))
            .Configure<Cors>(configuration.GetSection(nameof(Cors)))
            .Configure<IdentityOptions>(configuration.GetSection($"{nameof(Auth)}:{nameof(Auth.IdentityOptions)}"))
            .Configure<Localization>(configuration.GetSection(nameof(Localization)))
            .Configure<Swagger>(configuration.GetSection(nameof(Swagger)));

        return services;
    }

    public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var swagger = configuration.GetRequiredSection(nameof(Swagger)).Get<Swagger>();
        ArgumentNullException.ThrowIfNull(swagger);

        var localization = configuration.GetRequiredSection(nameof(Localization)).Get<Localization>();
        ArgumentNullException.ThrowIfNull(localization);

        var supportedCultures = localization.SupportedCultures.Count == 0
            ? [localization.DefaultCulture]
            : localization.SupportedCultures;

        var acceptLanguageOperationProcessor = new OperationProcessor(context =>
        {
            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                IsRequired = false,
                Type = JsonObjectType.String,
                Schema = JsonSchema.FromType<string>(),
                Kind = OpenApiParameterKind.Header,
                Description = "Preferred language for the response",
                Examples = supportedCultures.ToDictionary(
                    s => s,
                    s => new OpenApiExample
                    {
                        Value = s,
                    }),
            });
            return true;
        });

        if (!swagger.DocumentOptions.Any())
        {
            services.SwaggerDocument(options =>
            {
                options.DocumentSettings = settings =>
                {
                    settings.OperationProcessors.Add(acceptLanguageOperationProcessor);
                };
            });

            return services;
        }

        foreach (var documentOption in swagger.DocumentOptions)
        {
            services.SwaggerDocument(options =>
            {
                options.AutoTagPathSegmentIndex = documentOption.AutoTagPathSegmentIndex;
                options.DocumentSettings = settings =>
                {
                    settings.DocumentName = documentOption.DocumentSettings.DocumentName;
                    settings.Version = documentOption.DocumentSettings.Version;
                    settings.OperationProcessors.Add(acceptLanguageOperationProcessor);
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

    public static IServiceCollection AddLocalizationAndConfigure(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var localization = configuration.GetRequiredSection(nameof(Localization)).Get<Localization>();
        ArgumentNullException.ThrowIfNull(localization);

        services
            .AddLocalization(options => options.ResourcesPath = "Resources")
            .Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = localization.SupportedCultures
                    .Select(culture => new CultureInfo(culture))
                    .ToList();

                if (supportedCultures.Count == 0)
                {
                    supportedCultures.Add(new CultureInfo(localization.DefaultCulture));
                }

                options.DefaultRequestCulture = new RequestCulture(localization.DefaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

        return services;
    }

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var connectionStrings = configuration.GetRequiredSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
        ArgumentNullException.ThrowIfNull(connectionStrings);

        var defaultConnection = connectionStrings.DefaultConnection;
        services.AddHealthChecks()
            .AddApplicationStatus()
            .AddSqlite(defaultConnection);

        return services;
    }

    public static IServiceCollection AddAppCors(this IServiceCollection services, ConfigurationManager configuration)
    {
        var cors = configuration.GetRequiredSection(nameof(Cors)).Get<Cors>();
        ArgumentNullException.ThrowIfNull(cors);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins = cors.AllowedOrigins.ToArray();
                // var policyBuilder = allowedOrigins.Length > 0 && allowedOrigins is not ["*"]
                //     ? policy.WithOrigins(allowedOrigins)
                //     : policy.AllowAnyOrigin();

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(
                        TimeSpan.FromMinutes(cors.PreflightMaxAgeMinutes)); // Cache preflight for 10 minutes
            });
        });

        return services;
    }
}