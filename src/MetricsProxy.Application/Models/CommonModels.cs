using System;
using System.Collections.Generic;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Models
{
    public enum ReportStatus
    {
        Unknown,
        Success,
        Failure
    }
    public record ReportTargetModel(string SinkName, DateTime? SentOn, ReportStatus Status, string StatusDescription);
    public record ReportedKpi(Kpi Kpi, string Sink, bool Success, string ErrorMessage);
    public record KpiToReport(Kpi Kpi, string[] Sinks);
    public record KpiModel(string SourceName, string Key, string Value, DateTime ReceivedOn, List<ReportTargetModel> Targets);

    public record FailedStat(Kpi Kpi, string Sink, string Error);
    public record KpiStats(int TotalSent, int TotalSucceeded, int TotalFailed, List<Kpi> DistinctReportedKpis,
        List<FailedStat> Failed);
}