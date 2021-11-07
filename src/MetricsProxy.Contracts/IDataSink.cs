using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetricsProxy.Contracts
{
    /// <summary>
    /// A data sink used to send metrics to external services
    /// </summary>
    public interface IDataSink : INamedService
    {
        /// <summary>
        /// Report on the KPIs.
        /// </summary>
        /// <param name="items">The KPIs to report</param>
        Task Report(IEnumerable<Kpi> items);
    }
}