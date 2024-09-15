using Database.Context;
using Database.Seeders;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class DbSeedAppBuilderExtensions
{
    public static IApplicationBuilder UseDbSeed(this IApplicationBuilder builder, string[] args)
    {
        if (builder is not WebApplication app)
        {
            Console.WriteLine("UseDbSeed failed: IApplicationBuilder is not WebApplication.");
            return builder;
        }

        if (app.Environment.IsProduction())
        {
            Console.WriteLine("UseDbSeed failed: Should run the app in not production environment.");
            return builder;
        }

        var seed = args.Any(arg => arg == "seed");
        if (!seed) return builder;

        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ArgumentNullException.ThrowIfNull(context);

        if (seed) _ = new DatabaseSeeder(context);

        return builder;
    }
}