using Bogus;

namespace Database.Factories;

public class Factory<TEntity> : Faker<TEntity>
    where TEntity : class
{
}