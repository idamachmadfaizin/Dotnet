using Libs;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class DbSeedAppBuilderExtensions
{
    public static IApplicationBuilder UseDbSeed<T>(this IApplicationBuilder builder, string[] args)
        where T : Seeder
    {
        var seed = args.Any(arg => arg == "seed");

        if (!seed) return builder;

        using var scope = builder.ApplicationServices.CreateScope();

        var type = typeof(T);

        var constructor = type.GetConstructors().MaxBy(c => c.GetParameters().Length);
        if (constructor is null) throw new InvalidOperationException("No constructor found for the given type.");

        var parameters = constructor.GetParameters()
            .Select(p => scope.ServiceProvider.GetRequiredService(p.ParameterType))
            .ToArray();

        var seederService = constructor.Invoke(parameters);

        if (seederService is not T) throw new InvalidOperationException("No factory found for the given type.");

        return builder;
    }
}