using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Application.Models;

namespace MetricsProxy.Application.Contracts
{
    public interface IDataSinkReportingService
    {
        Task<IEnumerable<ReportedKpi>> Report(IEnumerable<KpiToReport> dataToReport);
        IReadOnlyList<string> GetSinkNames();
    }
}