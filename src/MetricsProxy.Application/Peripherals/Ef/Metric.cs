using System;
using System.Collections.Generic;

namespace MetricsProxy.Application.Peripherals.Ef
{
    public class Metric
    {
        public Guid MetricId { get; set; }
        public string SourceName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime ReceivedOn { get; set; }

        public List<MetricTarget> MetricTargets { get; set; } = new List<MetricTarget>();
    }
}