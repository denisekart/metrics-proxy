using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Contracts
{
    public interface IDataSourceQueryService
    {
        Task<IEnumerable<Kpi>> Query();
    }
}