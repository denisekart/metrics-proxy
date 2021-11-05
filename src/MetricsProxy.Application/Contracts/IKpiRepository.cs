using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Application.Models;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Contracts
{
    public interface IKpiRepository
    {
        Task<IEnumerable<KpiModel>> GetUnreportedData(IReadOnlyList<string> availableSinks, bool includeKpisWhereReportingFailed);
        Task Upsert(IEnumerable<KpiModel> models);
        Task<KpiStats> GetKpiStats();
    }
}