namespace Faker.Core
{
    public interface IValueGenerator
    {
        object Generate(GeneratorContext context);
    }
}
