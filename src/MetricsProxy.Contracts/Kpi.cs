using System;

namespace MetricsProxy.Contracts
{
    /// <summary>
    /// A metric representation in this system
    /// </summary>
    /// <param name="Key">The metric key</param>
    /// <param name="UnitOrValue">Unit or value of the metric</param>
    /// <param name="Source">The source service name of the metric. May be null - the system will ensure that this is correctly set</param>
    /// <param name="CreatedOn">The time when this metric was created in this system. May be null - the system will ensure that this is correctly set</param>
    public record Kpi(string Key, string UnitOrValue, string Source = null, DateTime? CreatedOn = null);
}