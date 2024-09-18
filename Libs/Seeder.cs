using Microsoft.Extensions.DependencyInjection;

namespace Libs;

public abstract class Seeder(IServiceProvider serviceProvider)
{
    protected TFactory Create<TFactory>()
        where TFactory : class
    {
        var type = typeof(TFactory);

        var constructor = type.GetConstructors().MaxBy(c => c.GetParameters().Length);

        if (constructor is null) throw new InvalidOperationException("No constructor found for the given type.");

        var parameters = constructor.GetParameters()
            .Select(p => serviceProvider.GetRequiredService(p.ParameterType))
            .ToArray();

        var factoryService = constructor.Invoke(parameters);

        if (factoryService is TFactory factory) return factory;

        throw new InvalidOperationException("No factory found for the given type.");
    }
}