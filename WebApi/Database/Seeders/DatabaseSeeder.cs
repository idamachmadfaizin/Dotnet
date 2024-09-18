using Database.Context;
using Database.Factories;
using Libs;

namespace Database.Seeders;

internal class DatabaseSeeder : Seeder
{
    public DatabaseSeeder(AppDbContext dbContext, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        dbContext.AddRange(Create<RoleFactory>().Generate(2));
        dbContext.SaveChanges();
    }
}