using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Models;
using MetricsProxy.Contracts;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace MetricsProxy.Application.Peripherals
{
    public class InMemoryKpiRepository : IKpiRepository
    {
        private List<KpiModel> _store = new List<KpiModel>();

        public async Task<IEnumerable<KpiModel>> GetUnreportedData(IReadOnlyList<string> availableSinks, bool includeKpisWhereReportingFailed)
        {
            return _store.Where(x =>
                availableSinks.Any(s => (x.Targets == null || !x.Targets.Select(t => t.SinkName).Contains(s)))
                || includeKpisWhereReportingFailed
                && x.Targets.Any(t => availableSinks.Contains(t.SinkName) && t.Status == ReportStatus.Failure));
        }

        public async Task Upsert(IEnumerable<KpiModel> models)
        {
            foreach (var kpiModel in models)
            {
                _store.RemoveAll(x => x.Key == kpiModel.Key && x.SourceName == kpiModel.SourceName && x.ReceivedOn == kpiModel.ReceivedOn);
            }
            _store.AddRange(models);
        }

        public async Task<KpiStats> GetKpiStats()
        {
            var all = _store
                .SelectMany(x => x.Targets ?? Enumerable.Empty<ReportTargetModel>())
                .Count(x => x.Status != ReportStatus.Unknown);
            var success = _store
                .SelectMany(x => x.Targets ?? Enumerable.Empty<ReportTargetModel>())
                .Count(x => x.Status == ReportStatus.Success);
            var failed = _store
                .SelectMany(x => x.Targets ?? Enumerable.Empty<ReportTargetModel>())
                .Count(x => x.Status == ReportStatus.Failure);
            var distinct = _store
                .Select(x => new { x.Key, x.SourceName })
                .Distinct()
                .Select(x => new Kpi(x.Key, null, x.SourceName))
                .ToList();
            var errors = _store
                .SelectMany(x => (x.Targets ?? Enumerable.Empty<ReportTargetModel>()).Select(t => new { target = t, model = x }))
                .Where(x => x.target.Status == ReportStatus.Failure)
                .Select(x => (new Kpi(x.model.Key, x.model.Value, x.model.SourceName), x.target.SinkName, x.target.StatusDescription))
                .ToList();

            return new(all, success, failed, distinct, errors);
        }
    }
}