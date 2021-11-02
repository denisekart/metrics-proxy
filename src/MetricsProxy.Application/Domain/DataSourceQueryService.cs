using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Domain
{
    public enum ReportStatus
    {
        Unknown,
        Success,
        Failure
    }
    public record ReportTargetModel(string SinkName, DateTime? SentOn, ReportStatus Status, string StatusDescription);
    public record KpiModel(string SourceName, string Key, string Value, DateTime ReceivedOn, List<ReportTargetModel> Targets);

    public class InMemoryKpiRepository : IKpiRepository
    {
        private List<KpiModel> _store = new List<KpiModel>();

        public async Task<IEnumerable<KpiModel>> GetUnreportedData(IReadOnlyList<string> availableSinks, bool includeKpisWhereReportingFailed)
        {
            return _store.Where(x => 
                availableSinks.Any(s => !x.Targets.Select(t => t.SinkName).Contains(s))
                || includeKpisWhereReportingFailed 
                && x.Targets.Any(t => availableSinks.Contains(t.SinkName) && t.Status == ReportStatus.Failure));
        }

        public Task Upsert(IEnumerable<KpiModel> models)
        {
            throw new NotImplementedException();
        }
    }

    public interface IKpiRepository
    {
        Task<IEnumerable<KpiModel>> GetUnreportedData(IReadOnlyList<string> availableSinks, bool includeKpisWhereReportingFailed);
        Task Upsert(IEnumerable<KpiModel> models);
    }

    public class DataSourceQueryService : IDataSourceQueryService
    {
        private readonly IEnumerable<IDataSource> _dataSources;

        public DataSourceQueryService(IEnumerable<IDataSource> dataSources)
        {
            _dataSources = dataSources;
        }

        public async Task<IEnumerable<Kpi>> Query()
        {
            var result = Enumerable.Empty<Kpi>();
            foreach (var dataSource in _dataSources)
            {
                result = result.Concat(await dataSource.Query());
            }

            return result;
        }
    }

    public interface IDataSourceQueryService
    {
        Task<IEnumerable<Kpi>> Query();
    }

    public record KpiToReport(Kpi Kpi, string[] Sinks);
    public record ReportedKpi(Kpi Kpi, string Sink, bool Success, string ErrorMessage);
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

    public interface IDataSinkReportingService
    {
        Task<IEnumerable<ReportedKpi>> Report(IEnumerable<KpiToReport> dataToReport);
        IReadOnlyList<string> GetSinkNames();
    }

    public interface IMetricsManagementService
    {
        Task QueryAndReport(CancellationToken cancellationToken);
    }
    public class MetricsManagementService : IMetricsManagementService
    {
        private readonly IDataSourceQueryService _queryService;
        private readonly IDataSinkReportingService _reportingService;
        private readonly IKpiRepository _kpiRepository;

        public MetricsManagementService(IDataSourceQueryService queryService, IDataSinkReportingService reportingService, IKpiRepository kpiRepository)
        {
            _queryService = queryService;
            _reportingService = reportingService;
            _kpiRepository = kpiRepository;
        }
        public async Task QueryAndReport(CancellationToken cancellationToken)
        {
            await QueryAndStoreData();
            var dataToReport = (await FetchDataToReport()).ToList();
            await ReportAndUpdateStoredData(dataToReport);
        }

        private async Task ReportAndUpdateStoredData(List<KpiToReport> dataToReport)
        {
            var reportedKpis = await _reportingService.Report(dataToReport);
            var models = reportedKpis
                .GroupBy(x => x.Kpi)
                .Select(MapKpiModel);

            await _kpiRepository.Upsert(models);
        }

        private static KpiModel MapKpiModel(IGrouping<Kpi, ReportedKpi> x)
        {
            return new KpiModel(
                x.Key.Source, 
                x.Key.Key, 
                x.Key.UnitOrValue, 
                DateTime.Now, 
                MapTargets(x));
        }

        private static List<ReportTargetModel> MapTargets(IGrouping<Kpi, ReportedKpi> x)
        {
            return x.Select(t => new ReportTargetModel(
                t.Sink,
                DateTime.Now, 
                t.Success 
                    ? ReportStatus.Success 
                    : ReportStatus.Failure, 
                t.ErrorMessage)).ToList();
        }

        private async Task<IEnumerable<KpiToReport>> FetchDataToReport()
        {

            var availableSinks = _reportingService.GetSinkNames();
            var unreportedData = await _kpiRepository.GetUnreportedData(availableSinks, false);
            var dataToReport = new List<KpiToReport>();

            foreach (var item in unreportedData)
            {
                var reportedSinks = item.Targets
                    .Where(x => x.Status == ReportStatus.Success)
                    .Select(x => x.SinkName);
                var sinksToReport = availableSinks.Except(reportedSinks);

                dataToReport.Add(new KpiToReport(new Kpi(item.Key, item.Value, item.SourceName), sinksToReport.ToArray()));
            }

            return dataToReport;
        }

        private async Task QueryAndStoreData()
        {
            var data = (await _queryService.Query())
                .Select(x => new KpiModel(x.Source, x.Key, x.UnitOrValue, DateTime.Now, null))
                .ToList();
            await _kpiRepository.Upsert(data);
        }
    }
}
