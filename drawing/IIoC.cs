using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA
{
    public interface IIoC
    {
        TInterface? Inject<TInterface>()
            where TInterface : class;

        IIoC RegisterAsSingleton<TInterface, TImplementation>(TInterface? instance = null)
            where TInterface : class
            where TImplementation : class;

        IIoC RegisterAsTransient<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class;
    }
}
