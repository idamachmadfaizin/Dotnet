using Configurations;
using FastEndpoints.Swagger;

namespace WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigure(this IServiceCollection services, IConfigurationManager configuration)
    {
        services
            .Configure<Auth>(configuration.GetSection(nameof(Auth)))
            .Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

        return services;
    }
    
    public static IServiceCollection AddSwaggerDocuments(this IServiceCollection services)
    {
        services.SwaggerDocument();
        // However, if you want to add multiple versions of the Swagger document, you can do so by using the following code:
        // services.SwaggerDocument(options =>
        // {
        //     options.MaxEndpointVersion = 2;
        //     options.DocumentSettings = s =>
        //     {
        //         s.DocumentName = "v2";
        //         s.Version = "v2";
        //     };
        // });

        return services;
    }
}