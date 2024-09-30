using Libs;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class DbSeedAppBuilderExtensions
{
    public static void UseDbSeed<T>(this IApplicationBuilder builder, string[] args)
        where T : Seeder
    {
        var seed = args.Any(arg => arg == "seed");
        if (!seed) return;

        using var scope = builder.ApplicationServices.CreateScope();

        var seederService = Create(typeof(T), scope);

        if (seederService is not T) throw new InvalidOperationException("No factory found for the given type.");

        Environment.Exit(0);
    }

    private static object Create(Type type, IServiceScope scope)
    {
        var constructor = type.GetConstructors().MaxBy(c => c.GetParameters().Length);
        if (constructor is null)
            throw new InvalidOperationException($"No constructor found for the given type: .{type.Name}");

        var parameters = constructor.GetParameters()
            .Select(p =>
            {
                if (!p.ParameterType.IsClass)
                    return scope.ServiceProvider.GetRequiredService(p.ParameterType);

                var currentType = p.ParameterType;
                while (currentType is not null && currentType != typeof(object))
                {
                    if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(Factory<>))
                        return Create(p.ParameterType, scope);
                    currentType = currentType.BaseType;
                }

                return scope.ServiceProvider.GetRequiredService(p.ParameterType);
            })
            .ToArray();

        return constructor.Invoke(parameters);
    }
}