using MetricsProxy.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Contracts
{
    /// <summary>
    /// Used to manipulate stored Kpis and their states
    /// </summary>
    public interface IKpiRepository
    {
        /// <summary>
        /// Gets all the data yet to be reported to <param name="availableSinks"></param>
        /// </summary>
        /// <param name="availableSinks">The sinks to reference when searching for the data</param>
        /// <param name="includeKpisWhereReportingFailed">Include failed Kpis as well as unreported ones</param>
        /// <returns></returns>
        Task<IEnumerable<KpiModel>> GetUnreportedData(IReadOnlyList<string> availableSinks, bool includeKpisWhereReportingFailed);
        /// <summary>
        /// Updates or inserts the <param name="models"></param>
        /// </summary>
        /// <param name="models">Models to upsert</param>
        Task Upsert(IEnumerable<KpiModel> models);
        /// <summary>
        /// Returns the statistics of the Kpis stored in the system such as send rate, failure rate, errors, etc.
        /// </summary>
        Task<KpiStats> GetKpiStats();
    }
}