using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Faker.Faker.Core
{
    public class Faker : IFaker
    {
        private Dictionary<Type, IValueGenerator> _generators;
        private Stack<Type> _circleDepend = new Stack<Type>();
        private FakerConfiguration _configuration = null;
        public Faker(FakerConfiguration Config)
        {
            _generators = new Dictionary<Type, IValueGenerator>();  
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (IsRequiredType(t, typeof(Generator<>)))
                {
                    if ((t.BaseType.GetGenericArguments().Count() > 0) && (t.Namespace == "Faker.Faker.Core"))
                        _generators.Add(t.BaseType.GetGenericArguments()[0], (IValueGenerator)Activator.CreateInstance(t));
                }
            }
            _generators.Add(typeof(List<>), new ListGenerator());
            ScanPlugins(AppDomain.CurrentDomain.BaseDirectory + "\\Plugins");
            this._configuration = Config;
        }

        private bool IsRequiredType(Type GeneratorType, Type RequiredType)
        {
            Type localType = GeneratorType;
            while ((localType != null) && (localType != typeof(object)))
            {
                Type buffer;
                if (localType.IsGenericType)
                    buffer = localType.GetGenericTypeDefinition();
                else
                    buffer = localType;
                if (RequiredType == buffer)
                    return true;
                localType = localType.BaseType;
            }
            return false;
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        public object Create(Type type)
        {
            if (_circleDepend.Where(_circleType => _circleType == type).Count() >= 5)
            {
                Console.WriteLine("Circular Dependency");
                return GetDefaultValue(type);
            }
            _circleDepend.Push(type);
            Faker faker = new Faker(_configuration);
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            GeneratorContext context = new GeneratorContext(new Random(seed), type, faker);

            IValueGenerator generator = FindGenerator(type);

            if (generator != null)
            {
                _circleDepend.Pop();
                return generator.Generate(context);
            }

            var obj = CreateObject(type);
            obj = FillObject(obj);
            _circleDepend.Pop();
            return obj;
        }

        private void ScanPlugins(string directory)
        {
          
        }

        private object FillObject(object obj)
        {
            return null;
        }

        private bool IsValueSet(MemberInfo member, object obj)
        {
            return false;
        }

        private object GetDefaultValue(Type t)
        {
            return null;
        }

        private object CreateObject(Type type)
        {
            return null;
        }

        public IValueGenerator FindGenerator(Type type)
        {
            return null;
        }
    }
}
