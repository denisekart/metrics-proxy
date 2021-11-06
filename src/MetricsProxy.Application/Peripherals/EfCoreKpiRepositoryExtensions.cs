using System;
using System.Linq;
using MetricsProxy.Application.Models;
using MetricsProxy.Application.Peripherals.Ef;

namespace MetricsProxy.Application.Peripherals
{
    public static class EfCoreKpiRepositoryExtensions
    {
        public static KpiModel Map(this Metric metric) => new KpiModel(
            metric.SourceName,
            metric.Key,
            metric.Value,
            metric.ReceivedOn,
            metric.MetricTargets
                ?.Select(x => Map((MetricTarget) x))
                .ToList());

        public static Metric Map(this KpiModel model) => new Metric
        {
            Key = model.Key,
            SourceName = model.SourceName,
            ReceivedOn = model.ReceivedOn,
            Value = model.Value,
            MetricTargets = (model.Targets ?? Enumerable.Empty<ReportTargetModel>())
                .Select(t => t.Map())
                .ToList()
        };

        public static ReportTargetModel Map(this MetricTarget target) => new ReportTargetModel(
            target.SinkName,
            target.SentOn,
            target.Status switch
            {
                EfReportStatus.Unknown => ReportStatus.Unknown,
                EfReportStatus.Success => ReportStatus.Success,
                EfReportStatus.Failure => ReportStatus.Failure,
                _ => throw new ArgumentOutOfRangeException()
            },
            target.StatusDescription);

        public static MetricTarget Map(this ReportTargetModel target) => new MetricTarget
        {
            Status = target.Status switch
            {
                ReportStatus.Unknown => EfReportStatus.Unknown,
                ReportStatus.Success => EfReportStatus.Success,
                ReportStatus.Failure => EfReportStatus.Failure,
                _ => throw new ArgumentOutOfRangeException()
            },
            SinkName = target.SinkName,
            SentOn = target.SentOn,
            StatusDescription = target.StatusDescription
        };
    }
}