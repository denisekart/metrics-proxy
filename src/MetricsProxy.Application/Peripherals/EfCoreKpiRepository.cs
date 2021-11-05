using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Models;
using MetricsProxy.Application.Peripherals.Ef;
using MetricsProxy.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MetricsProxy.Application.Peripherals
{
    public class EfCoreKpiRepository : IKpiRepository
    {
        private readonly MetricsContext _context;

        public EfCoreKpiRepository(MetricsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KpiModel>> GetUnreportedData(IReadOnlyList<string> availableSinks, bool includeKpisWhereReportingFailed)
        {
            var metrics = (
                    from metric in _context.Metrics.Include(x => x.MetricTargets)
                    where metric.MetricTargets.Count == 0
                    select metric)
                .Union(
                    from target in _context.MetricTargets
                        .Include(x => x.Metric)
                        .ThenInclude(x=>x.MetricTargets)
                    where availableSinks.Contains(target.SinkName) || includeKpisWhereReportingFailed &&
                        availableSinks.Contains(target.SinkName) && target.Status == EfReportStatus.Failure
                    select target.Metric)
                .AsEnumerable()
                .Distinct()
                .Select(x => x.Map());
            
            return metrics;
        }

        public async Task Upsert(IEnumerable<KpiModel> models)
        {
            var modelsList = models.ToList();
            var keys = modelsList.Select(x => x.Key).ToList();

            var applicableMetricsToRemove = (from metric in _context.Metrics.Include(x => x.MetricTargets)
                    where keys.Contains(metric.Key)
                    select metric)
                .AsEnumerable()
                .Where(e => modelsList.Any(x =>
                    x.Key == e.Key && x.SourceName == e.SourceName && x.ReceivedOn == e.ReceivedOn));

            _context.Metrics.RemoveRange(applicableMetricsToRemove);

            await _context.Metrics.AddRangeAsync(modelsList.Select(m => m.Map()));

            await _context.SaveChangesAsync();
        }

        public async Task<KpiStats> GetKpiStats()
        {
            var all = await _context.MetricTargets
                .CountAsync(x => x.Status != EfReportStatus.Unknown);
            var success = await _context.MetricTargets
                .CountAsync(x => x.Status == EfReportStatus.Success);
            var failed = await _context.MetricTargets
                .CountAsync(x => x.Status == EfReportStatus.Failure);
            var distinct = await _context.Metrics
                .Select(x => new { x.Key, x.SourceName })
                .Distinct()
                .Select(x => new Kpi(x.Key, null, x.SourceName, null))
                .ToListAsync();
            var errors = _context.MetricTargets.Include(x => x.Metric)
                .Where(x => x.Status == EfReportStatus.Failure)
                .AsEnumerable()
                .Select(x => new FailedStat(new Kpi(x.Metric.Key, x.Metric.Value, x.Metric.SourceName, x.Metric.ReceivedOn), x.SinkName, x.StatusDescription))
                .ToList();

            return new(all, success, failed, distinct, errors);
        }
    }
}