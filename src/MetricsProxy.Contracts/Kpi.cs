using System;

namespace MetricsProxy.Contracts
{
    public record Kpi(string Key, string UnitOrValue, string Source = null, DateTime? CreatedOn = null);
}