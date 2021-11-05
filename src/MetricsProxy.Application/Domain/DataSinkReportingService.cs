using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Models;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Domain
{
    public class DataSinkReportingService : IDataSinkReportingService
    {
        private readonly IEnumerable<IDataSink> _dataSinks;

        public DataSinkReportingService(IEnumerable<IDataSink> dataSinks)
        {
            _dataSinks = dataSinks;
        }

        public async Task<IEnumerable<ReportedKpi>> Report(IEnumerable<KpiToReport> dataToReport)
        {
            var result = new List<ReportedKpi>();
            var groupedData = dataToReport
                .SelectMany(x => x.Sinks
                    .Select(s => new ReportedKpi(x.Kpi, s, true, null)))
                .GroupBy(x => x.Sink);

            foreach (var kpis in groupedData)
            {
                var sink = _dataSinks.First(x => x.Name == kpis.Key);
                try
                {
                    await sink.Report(kpis.Select(x => x.Kpi));
                    result.AddRange(kpis);
                }
                catch (Exception e)
                {
                    result.AddRange(kpis.Select(x => x with { Success = false, ErrorMessage = e.Message }));
                }
            }

            return result;
        }

        public IReadOnlyList<string> GetSinkNames()
        {
            return (_dataSinks?.Select(x => x.Name) ?? Enumerable.Empty<string>())
                .ToList()
                .AsReadOnly();
        }
    }
}