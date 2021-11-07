using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Application.Models;

namespace MetricsProxy.Application.Contracts
{
    /// <summary>
    /// Used to report metrics to various data sinks
    /// </summary>
    public interface IDataSinkReportingService
    {
        /// <summary>
        /// Report the data
        /// </summary>
        /// <param name="dataToReport">Kpis to report</param>
        /// <returns>Reported Kpis</returns>
        Task<IEnumerable<ReportedKpi>> Report(IEnumerable<KpiToReport> dataToReport);
        /// <summary>
        /// Gets all available sink names
        /// </summary>
        IReadOnlyList<string> GetSinkNames();
    }
}