using System.Threading;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Contracts
{
    /// <summary>
    /// The entry service used to proxy data source Kpis to data sinks
    /// </summary>
    public interface IMetricsManagementService
    {
        /// <summary>
        /// Executes the query and report loop.
        /// Queries the data from upstream data sources and
        /// reports the data to downstream data sinks
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        Task QueryAndReport(CancellationToken cancellationToken);
    }
}