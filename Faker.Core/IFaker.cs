namespace Faker.Core
{
    public interface IFaker
    {
        IValueGenerator FindGenerator(Type type);
        object Create(Type type);
    }
}
