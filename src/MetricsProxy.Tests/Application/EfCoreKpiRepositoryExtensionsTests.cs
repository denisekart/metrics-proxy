using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MetricsProxy.Application.Models;
using MetricsProxy.Application.Peripherals;
using MetricsProxy.Application.Peripherals.Ef;
using NUnit.Framework;

namespace MetricsProxy.Tests.Application
{
    public class EfCoreKpiRepositoryExtensionsTests
    {
        [Test]
        public void ShouldCorrectlyMapMetricToKpiModel()
        {
            // Arrange
            var metric = new Metric
            {
                Key = "k1",
                SourceName = "s1",
                ReceivedOn = DateTime.Now,
                Value = "1",
                MetricId = Guid.NewGuid(),
                MetricTargets = new List<MetricTarget>
                {
                    new MetricTarget
                    {
                        SentOn = DateTime.Now,
                        SinkName = "si1",
                        Status = EfReportStatus.Success,
                        StatusDescription = "d1"
                    }
                }
            };

            // Act
            var model = metric.Map();

            // Assert
            Compare(metric, model);
        }

        [Test]
        public void ShouldCorrectlyMapKpiModelToMetric()
        {
            // Arrange
            var model = new KpiModel("s1", "k1", "2", DateTime.Now, new List<ReportTargetModel>
            {
                new("sn1", DateTime.Now, ReportStatus.Failure, "sd1")
            });

            // Act
            var metric = model.Map();

            // Assert
            Compare(metric, model);
        }

        private static void Compare(Metric metric, KpiModel model)
        {
            metric.Should().NotBeNull();
            model.Should().NotBeNull();

            metric.Key.Should().Be(model.Key);
            metric.SourceName.Should().Be(model.SourceName);
            metric.ReceivedOn.Should().Be(model.ReceivedOn);
            metric.Value.Should().Be(model.Value);

            metric.MetricTargets.Should().NotBeNullOrEmpty();
            model.Targets.Should().NotBeNullOrEmpty();
            metric.MetricTargets.Count.Should().Be(model.Targets.Count);

            foreach (var modelTarget in model.Targets)
            {
                var metricTarget = metric.MetricTargets.FirstOrDefault(x =>
                    x.StatusDescription == modelTarget.StatusDescription
                    && x.SentOn == modelTarget.SentOn
                    && x.SinkName == modelTarget.SinkName
                    && x.Status == modelTarget.Status switch
                    {
                        ReportStatus.Unknown => EfReportStatus.Unknown,
                        ReportStatus.Success => EfReportStatus.Success,
                        ReportStatus.Failure => EfReportStatus.Failure,
                        _ => throw new ArgumentOutOfRangeException()
                    });
                metricTarget.Should()
                    .NotBeNull(
                        $"Expected a target with the following properties to exist in the metric: {modelTarget}");
            }
        }
    }
}