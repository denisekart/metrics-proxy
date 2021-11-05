using System;

namespace MetricsProxy.Application.Peripherals.Ef
{
    public class MetricTarget
    {
        public Guid MetricTargetId { get; set; }
        public string SinkName { get; set; }
        public DateTime? SentOn { get; set; }
        public EfReportStatus Status { get; set; }
        public string StatusDescription { get; set; }

        public Guid MetricId { get; set; }
        public Metric Metric { get; set; }
    }
}