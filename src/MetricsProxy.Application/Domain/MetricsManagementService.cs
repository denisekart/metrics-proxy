using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Models;
using MetricsProxy.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Domain
{
    public class MetricsManagementService : IMetricsManagementService
    {
        private readonly IDataSourceQueryService _queryService;
        private readonly IDataSinkReportingService _reportingService;
        private readonly IKpiRepository _kpiRepository;

        public MetricsManagementService(
            IDataSourceQueryService queryService,
            IDataSinkReportingService reportingService,
            IKpiRepository kpiRepository)
        {
            _queryService = queryService;
            _reportingService = reportingService;
            _kpiRepository = kpiRepository;
        }

        public async Task QueryAndReport(CancellationToken cancellationToken)
        {
            await QueryAndStoreData();
            var dataToReport = (await FetchDataToReport()).ToList();

            if (dataToReport.Any())
                await ReportAndUpdateStoredData(dataToReport);
        }

        private async Task ReportAndUpdateStoredData(List<KpiToReport> dataToReport)
        {
            var reportedKpis = await _reportingService.Report(dataToReport);
            var models = reportedKpis
                .GroupBy(x => x.Kpi)
                .Select(MapKpiModel);

            if (models.Any())
                await _kpiRepository.Upsert(models);
        }

        private static KpiModel MapKpiModel(IGrouping<Kpi, ReportedKpi> x)
        {
            return new(
                x.Key.Source,
                x.Key.Key,
                x.Key.UnitOrValue,
                x.Key.CreatedOn ?? DateTime.Now,
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
                    ?.Where(x => x.Status == ReportStatus.Success)
                    .Select(x => x.SinkName) ?? Enumerable.Empty<string>();
                var sinksToReport = availableSinks.Except(reportedSinks);

                dataToReport.Add(new KpiToReport(new Kpi(item.Key, item.Value, item.SourceName, item.ReceivedOn), sinksToReport.ToArray()));
            }

            return dataToReport;
        }

        private async Task QueryAndStoreData()
        {
            var data = (await _queryService.Query())
                .Select(x => new KpiModel(x.Source, x.Key, x.UnitOrValue, x.CreatedOn ?? DateTime.Now, null))
                .ToList();

            if (data.Any())
                await _kpiRepository.Upsert(data);
        }
    }
}