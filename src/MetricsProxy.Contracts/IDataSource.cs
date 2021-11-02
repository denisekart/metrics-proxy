using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetricsProxy.Contracts
{
    public interface IDataSource : INamedService
    {
        Task<IEnumerable<Kpi>> Query();
    }
}
