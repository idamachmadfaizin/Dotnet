using Bogus;

namespace Libs;

public abstract class Factory<TEntity> : Faker<TEntity>
    where TEntity : class
{
}