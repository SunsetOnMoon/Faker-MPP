namespace Faker.Faker.Core
{
    public class IntGenerator : Generator<int>
    {
        public override int ObjectGeneration(Random random)
        {
            int randomValue = random.Next();
            return randomValue;
        }
    }

    public class FloatGenerator : Generator<float>
    {
        public override float ObjectGeneration(Random random)
        {
            float randomValue = (float)(random.NextDouble()) * random.Next();
            return randomValue;
        }
    }

    public class LongGenerator : Generator<long>
    {
        public override long ObjectGeneration(Random random)
        {
            long randomValue = random.Next()<<32 + random.Next();
            return randomValue;
        }
    }

    public class DoubleGenerator : Generator<double>
    {
        public override double ObjectGeneration(Random random)
        {
            double randomValue = (double)(random.NextDouble()) * random.Next();
            return randomValue;
        }
    }

    public class CharGenerator : Generator<char>
    {
        public override char ObjectGeneration(Random random)
        {
            char randomValue = (char)(random.Next(0, 255));
            return randomValue;
        }
    }

    public class BoolGenerator : Generator<bool>
    {
        public override bool ObjectGeneration(Random random)
        {
            bool randomValue = Convert.ToBoolean(random.Next(0, 1));
            return randomValue;
        }
    }
}
