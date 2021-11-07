using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetricsProxy.Contracts
{
    /// <summary>
    /// A data source used to pull metrics into this system
    /// </summary>
    public interface IDataSource : INamedService
    {
        /// <summary>
        /// Query external system(s) for data
        /// </summary>
        /// <returns>The queried data from external systems</returns>
        Task<IEnumerable<Kpi>> Query();
    }
}
