using Database.Context;

namespace Database.Seeders;

public class DatabaseSeeder : Seeder
{
    public DatabaseSeeder(AppDbContext dbContext) : base(dbContext)
    {
        dbContext.SaveChanges();
    }
}