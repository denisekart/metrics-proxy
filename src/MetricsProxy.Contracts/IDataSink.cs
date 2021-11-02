using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetricsProxy.Contracts
{
    public interface IDataSink : INamedService
    {
        Task Report(IEnumerable<Kpi> items);
    }
}