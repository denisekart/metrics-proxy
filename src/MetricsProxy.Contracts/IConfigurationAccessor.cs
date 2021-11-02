using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsProxy.Contracts
{
    public interface IConfigurationAccessor<TService> where TService : INamedService
    {
        T Get<T>(TService instance, string path = null);
    }
}
