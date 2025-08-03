using Libs;
using WebApi.Database.Context;
using WebApi.Database.Factories;

namespace WebApi.Database.Seeders;

// ReSharper disable once ClassNeverInstantiated.Global
internal class DatabaseSeeder : Seeder
{
    public DatabaseSeeder(AppDbContext dbContext, RoleFactory roleFactory)
    {
        dbContext.AddRange(roleFactory.Generate(2));
        dbContext.SaveChanges();
    }
}