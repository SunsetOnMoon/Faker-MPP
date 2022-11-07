using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faker.Faker.Core
{
    public interface IFaker
    {
        IValueGenerator FindGenerator(Type type);
        object Create(Type type);
    }
}
