using Faker.Core;
namespace DateTimeGenerator
{
    public class DateTimeGenerator : Generator<DateTime>
    {
        public override DateTime ObjectGeneration(Random random)
        {
            int year = random.Next(1, 9999);
            int month = random.Next(1, 12);
            int day = random.Next(1, 28);
            int hour = random.Next(0, 23);
            int minute = random.Next(0, 59);
            int second = random.Next(0, 59);

            DateTime result = new DateTime(year, month, day, hour, minute, second);
            return result;
        }
    }
}