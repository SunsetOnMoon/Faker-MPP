using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Faker.Core
{
    public class FakerService : IFaker
    {
        private Dictionary<Type, IValueGenerator> _generators;
        private Stack<Type> _circleDepend = new Stack<Type>();
        private FakerConfiguration _configuration = null;
        public FakerService(FakerConfiguration Config)
        {
            _generators = new Dictionary<Type, IValueGenerator>();  
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (IsRequiredType(t, typeof(Generator<>)))
                {
                    if ((t.BaseType.GetGenericArguments().Count() > 0) && (t.Namespace == "Faker.Core"))
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
            FakerService faker = new FakerService(_configuration);
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
            foreach (var file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
            {
                var asm = Assembly.LoadFile(file);
                foreach (Type type in asm.GetTypes())
                {
                    if (IsRequiredType(type, typeof(Generator<>)))
                    {
                        if (type.BaseType.GetGenericArguments().Count() > 0)
                            _generators.Add(type.BaseType.GetGenericArguments()[0], (IValueGenerator)Activator.CreateInstance(type));
                    }
                }
            }
        }

        private object FillObject(object obj)
        {
            if (obj != null)
            {
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public); 
                foreach (FieldInfo field in fields)
                {
                    if (IsValueSet(field, obj))
                    {
                        ConfigurationRule configurationRule = null;
                        if (_configuration != null)
                        {
                            foreach (ConfigurationRule rule in _configuration.ConfigurationRules)
                            {
                                if ((rule.FieldName == field.Name) && (rule.FieldType == field.FieldType))
                                    configurationRule = rule;
                            }
                        }

                        if (configurationRule == null)
                            field.SetValue(obj, Create(field.FieldType));
                        else
                        {
                            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
                            FakerService faker = new FakerService(_configuration);
                            GeneratorContext context = new GeneratorContext(new Random(seed), field.FieldType, faker);
                            IValueGenerator test = (IValueGenerator)Activator.CreateInstance(configurationRule.GeneratorName);
                            field.SetValue(obj, test.Generate(context));
                        }
                    }
                }

                foreach(PropertyInfo property in properties)
                {
                    if ((property.CanWrite) && (IsValueSet(property, obj)))
                    {
                        ConfigurationRule configurationRule = null;
                        if (_configuration != null)
                        {
                            foreach (ConfigurationRule rule in _configuration.ConfigurationRules)
                            {
                                if ((rule.FieldName == property.Name) && (rule.FieldType == property.PropertyType))
                                    configurationRule = rule;
                            }
                        }
                        if (configurationRule == null)
                        {
                            property.SetValue(obj, Create(property.PropertyType));
                        }
                        else
                        {
                            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
                            FakerService faker = new FakerService(_configuration);
                            GeneratorContext context = new GeneratorContext(new Random(seed), property.PropertyType, faker);
                            IValueGenerator test = (IValueGenerator)Activator.CreateInstance(configurationRule.GeneratorName);
                            property.SetValue(obj, ((IValueGenerator)Activator.CreateInstance(configurationRule.GeneratorName)).Generate(context));
                        }
                    }
                }
            }
            return obj;
        }

        private bool IsValueSet(MemberInfo member, object obj)
        {
            if (member is FieldInfo field)
            {
                if (GetDefaultValue(field.FieldType) == null) return true;
                if (field.GetValue(obj).Equals(GetDefaultValue(field.FieldType))) return true;
            }
            
            if (member is PropertyInfo property)
            {
                if (GetDefaultValue(property.PropertyType) == null) return true;
                if (property.GetValue(obj).Equals(GetDefaultValue(property.PropertyType))) return true;
            }
            return false;
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            else
                return null;
        }

        private object CreateObject(Type type)
        {
            ConstructorInfo[] bufConstructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            IEnumerable<ConstructorInfo> constructors = bufConstructors.OrderByDescending(constuctor => constuctor.GetParameters().Length);
            
            object obj = null;
            
            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parametersInfo = constructor.GetParameters();
                object[] parameters = new object[parametersInfo.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ConfigurationRule configurationRule = null;
                    if (_configuration != null)
                    {
                        foreach (ConfigurationRule rule in _configuration.ConfigurationRules)
                        {
                            if ((rule.FieldName == parametersInfo[i].Name) && (rule.FieldType == parametersInfo[i].ParameterType))
                                configurationRule = rule;
                        }
                    }
                    if (configurationRule == null)
                        parameters[i] = Create(parametersInfo[i].ParameterType);
                    else
                    {
                        int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
                        FakerService faker = new FakerService(_configuration);
                        GeneratorContext context = new GeneratorContext(new Random(seed), type, faker);
                        IValueGenerator test = (IValueGenerator)Activator.CreateInstance(configurationRule.GeneratorName);
                        parameters[i] = ((IValueGenerator)Activator.CreateInstance(configurationRule.GeneratorName)).Generate(context);
                    }
                }
                try
                {
                    obj = constructor.Invoke(parameters);
                    break;
                }
                catch
                {
                    continue;
                }
            }
            if ((obj == null) && (type.IsValueType))
                obj = Activator.CreateInstance(type);
            return obj;
        }

        public IValueGenerator FindGenerator(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();
            if (_generators.ContainsKey(type))
                return _generators[type];
            else
                return null;
        }
    }
}
