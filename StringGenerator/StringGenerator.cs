using Faker.Core;
namespace StringGenerator
{
    public class StringGenerator : Generator<string>
    {
        public override string ObjectGeneration(Random random)
        {
            byte[] byteArray = new byte[random.Next(10, 20)];

            for (int i = 0; i < byteArray.Length; i++)
                byteArray[i] = (byte)random.Next(90, 255);
            string result = System.Text.Encoding.UTF8.GetString(byteArray);
            return result;
        }
    }
}