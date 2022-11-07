using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faker.Faker.Core
{
    public interface IValueGenerator
    {
        object Generate(GeneratorContext context);
    }
}
