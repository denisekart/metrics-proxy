using System;
using System.Collections.Generic;
using MetricsProxy.Contracts;

namespace MetricsProxy.Application.Models
{
    /// <summary>
    /// The current state of a kpi in the system
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Invaid status
        /// </summary>
        Unknown,
        /// <summary>
        /// The KPI was reported
        /// </summary>
        Success,
        /// <summary>
        /// The KPI reporting failed
        /// </summary>
        Failure
    }

    /// <summary>
    /// The report target model
    /// </summary>
    /// <param name="SinkName">The sink name</param>
    /// <param name="SentOn">The time that the data was sent to the target sink</param>
    /// <param name="Status">The status of the data</param>
    /// <param name="StatusDescription">The status description. Usually contains the error message in case of failure</param>
    public record ReportTargetModel(
        string SinkName, DateTime? SentOn, ReportStatus Status, string StatusDescription);
    
    /// <summary>
    /// The reported KPI
    /// </summary>
    /// <param name="Kpi">The kpi</param>
    /// <param name="Sink">The sink where the KPI was reported to</param>
    /// <param name="Success">True if reporting succeeded</param>
    /// <param name="ErrorMessage">Contains error message in case of an error when sending</param>
    public record ReportedKpi(Kpi Kpi, string Sink, bool Success, string ErrorMessage);

    /// <summary>
    /// The KPI to report
    /// </summary>
    /// <param name="Kpi">The kpi</param>
    /// <param name="Sinks">The sinks to report to</param>
    public record KpiToReport(Kpi Kpi, string[] Sinks);

    /// <summary>
    /// The KPI model
    /// </summary>
    /// <param name="SourceName">The KPI source system</param>
    /// <param name="Key">The KPI key</param>
    /// <param name="Value">The KPI value as string. (no units).</param>
    /// <param name="ReceivedOn">The time that the KPI was received</param>
    /// <param name="Targets">A list of targets that the sending was already initiated at least once</param>
    public record KpiModel(string SourceName, string Key, string Value, DateTime ReceivedOn, List<ReportTargetModel> Targets);

    /// <summary>
    /// A statistic on a failed KPI delivery
    /// </summary>
    /// <param name="Kpi">The KPI that failed delivery</param>
    /// <param name="Sink">The sink that failed delivery</param>
    /// <param name="Error">An error that occurred during delivery</param>
    public record FailedStat(Kpi Kpi, string Sink, string Error);

    /// <summary>
    /// A statistic on the KPIs in the system
    /// </summary>
    /// <param name="TotalSent">The total KPIs sent</param>
    /// <param name="TotalSucceeded">Total success count out of <paramref name="TotalSent"/></param>
    /// <param name="TotalFailed">Total failure count out of <paramref name="TotalSent"/></param>
    /// <param name="DistinctReportedKpis">All distinct KPIs in the system</param>
    /// <param name="Failed">Description of failed KPIs</param>
    public record KpiStats(int TotalSent, int TotalSucceeded, int TotalFailed, List<Kpi> DistinctReportedKpis,
        List<FailedStat> Failed);
}