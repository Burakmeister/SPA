using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace drawing
{
    public class IoC : IIoC
    {
        public TInterface? Inject<TInterface>()
            where TInterface : class =>
            _factory.TryGetValue(typeof(TInterface), out var func)
                ? func() as TInterface
                : null;

        public IIoC RegisterAsSingleton<TInterface, TImplementation>(TInterface? instance = null)
            where TInterface : class
            where TImplementation : class
        {
            TInterface singleton = instance
                ?? Construct<TInterface, TImplementation>()
                ?? throw new NullReferenceException(nameof(TImplementation));
            _factory[typeof(TInterface)] = () => singleton;
            return this;
        }


        private TInterface? Construct<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            var ctors = typeof(TImplementation)
                .GetConstructors();

            if (typeof(TImplementation).IsValueType ||
                typeof(TImplementation) == typeof(string) ||
                ctors.FirstOrDefault(i => i.GetParameters().Length == 0) is ConstructorInfo info)
            {
                return Activator.CreateInstance<TImplementation>() as TInterface;
            }

            for (int i = 0; i < ctors.Length; i++)
            {
                var ctor = ctors[i];
                var parameters = ctor.GetParameters();
                object[] objects = new object[parameters.Length];

                for (int j = 0; j < objects.Length; j++)
                {
                    if (_factory.TryGetValue(parameters[j].ParameterType, out var func))
                        objects[j] = func();
                }

                return ctor.Invoke(objects) as TInterface;
            }

            throw new NotImplementedException();
        }

        public IIoC RegisterAsTransient<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            _factory[typeof(TInterface)] = () =>
            Construct<TInterface, TImplementation>()
                ?? throw new NullReferenceException(nameof(TImplementation));
            return this;
        }

        private readonly Dictionary<Type, Func<object>> _factory = new();
    }
}
