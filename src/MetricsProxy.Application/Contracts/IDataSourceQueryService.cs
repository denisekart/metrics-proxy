using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Contracts
{
    /// <summary>
    /// Used to query various data sources
    /// </summary>
    public interface IDataSourceQueryService
    {
        /// <summary>
        /// Returns and aggregation of the results from all available data sources
        /// </summary>
        Task<IEnumerable<Kpi>> Query();
    }
}