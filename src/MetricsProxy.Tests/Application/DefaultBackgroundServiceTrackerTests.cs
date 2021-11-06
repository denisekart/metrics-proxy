using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MetricsProxy.Application.Application;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Domain;
using MetricsProxy.Application.Models;
using MetricsProxy.Application.Peripherals;
using MetricsProxy.Application.Peripherals.Ef;
using MetricsProxy.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MetricsProxy.Tests.Application
{
    public class DefaultBackgroundServiceTrackerTests
    {
        [Test]
        public void ShouldKeepTrackOfState_WhenQueried()
        {
            // Arrange
            var sut = new DefaultBackgroundServiceTracker();
            const string state = "test state";

            // Act
            sut.Report(state);

            // Assert
            sut.Query().Should().Be(state);
        }
    }

    public class DataSourceQueryServiceTests
    {

        [Test]
        public void ShouldLogErrorAndNotThrow_WhenExceptionIsRaised()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DataSourceQueryService>>();
            var dataSourceMock = new Mock<IDataSource>();
            dataSourceMock.Setup(x => x.Query()).Throws(new Exception());
            var sut = new DataSourceQueryService(new IDataSource[] { dataSourceMock.Object }, loggerMock.Object);

            // Act
            sut.Invoking(s => s.Query()).Should().NotThrowAsync();
            loggerMock.VerifyAll();
        }

        [Test]
        public async Task ShouldCallAllDataSources_WhenAnyDataSourceThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DataSourceQueryService>>();
            var dataSourceMock = new Mock<IDataSource>();
            dataSourceMock.Setup(x => x.Query()).Throws(new Exception());
            var dataSourceMock2 = new Mock<IDataSource>();
            var sut = new DataSourceQueryService(new IDataSource[] { dataSourceMock.Object, dataSourceMock2.Object }, loggerMock.Object);

            // Act
            await sut.Query();

            // Assert
            dataSourceMock.Verify(x => x.Query(), Times.Once);
            dataSourceMock2.Verify(x => x.Query(), Times.Once);
        }

        [Test]
        public async Task ShouldAggregateAllDataSourceResponses()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DataSourceQueryService>>();
            var dataSourceMock = new Mock<IDataSource>();
            dataSourceMock.Setup(x => x.Query()).Returns(() => Task.FromResult(new List<Kpi> { new Kpi("k1", null) }.AsEnumerable()));
            var dataSourceMock2 = new Mock<IDataSource>();
            dataSourceMock2.Setup(x => x.Query()).Returns(() => Task.FromResult(new List<Kpi> { new Kpi("k2", null) }.AsEnumerable()));

            var sut = new DataSourceQueryService(new[] { dataSourceMock.Object, dataSourceMock2.Object }, loggerMock.Object);

            // Act
            var actual = (await sut.Query())?.ToList();

            // Assert
            actual.Should().NotBeNullOrEmpty();
            actual.Should().HaveCount(2);
        }
    }

    public class DataSinkReportingServiceTests
    {
        [Test]
        public async Task ShouldAttemptSendingAllData_WhenAnyDataSinkThrows()
        {
            // Arrange
            var dataSinkMock = new Mock<IDataSink>();
            dataSinkMock.SetupGet(x => x.Name).Returns("s1");
            dataSinkMock.Setup(x => x.Report(It.IsAny<IEnumerable<Kpi>>())).Throws<Exception>();
            var dataSinkMock2 = new Mock<IDataSink>();
            dataSinkMock2.SetupGet(x => x.Name).Returns("s2");
            dataSinkMock2.Setup(x => x.Report(It.IsAny<IEnumerable<Kpi>>()))
                .Returns(() => Task.FromResult(new List<ReportedKpi>()));
            var sut = new DataSinkReportingService(new[] { dataSinkMock.Object, dataSinkMock2.Object });

            // Act
            var actual = await sut.Report(
                new List<KpiToReport>()
                {
                    new KpiToReport(new Kpi("k1", null), new[] {"s1"}),
                    new KpiToReport(new Kpi("k2", null), new[] {"s2"}),
                });

            // Assert
            actual.Should().NotBeNullOrEmpty();
            actual.Should().HaveCount(2);
            actual.First().Success.Should().BeFalse();
            actual.Last().Success.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnAllApplicableSinkNames()
        {
            // Arrange
            var dataSinkMock = new Mock<IDataSink>();
            dataSinkMock.SetupGet(x => x.Name).Returns("s1");
            var dataSinkMock2 = new Mock<IDataSink>();
            dataSinkMock2.SetupGet(x => x.Name).Returns("s2");
            var sut = new DataSinkReportingService(new[] { dataSinkMock.Object, dataSinkMock2.Object });

            // Act
            var actual = sut.GetSinkNames();

            // Assert
            actual.Should().NotBeNullOrEmpty();
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(new List<string> { "s1", "s2" });
        }
    }

    public class MetricsManagementServiceTests
    {
        [Test]
        public async Task ShouldNotAttemptToStoreQueriedData_WhenThereIsNoData()
        {
            // Arrange
            var queryServiceMock = new Mock<IDataSourceQueryService>();
            queryServiceMock.Setup(x => x.Query()).Returns(() => Task.FromResult(new List<Kpi>().AsEnumerable()));
            var reportingServiceMock = new Mock<IDataSinkReportingService>();
            var repositoryMock = new Mock<IKpiRepository>();
            var sut = new MetricsManagementService(queryServiceMock.Object, reportingServiceMock.Object, repositoryMock.Object);

            // Act
            await sut.QueryAndReport(CancellationToken.None);

            // Assert
            repositoryMock.Verify(x => x.Upsert(It.IsAny<IEnumerable<KpiModel>>()), Times.Never);
        }

        [Test]
        public async Task ShouldAttemptToStoreQueriedData()
        {
            // Arrange
            var queryServiceMock = new Mock<IDataSourceQueryService>();
            queryServiceMock.Setup(x => x.Query()).Returns(() => Task.FromResult(new List<Kpi> { new Kpi("k1", null) }.AsEnumerable()));
            var reportingServiceMock = new Mock<IDataSinkReportingService>();
            var repositoryMock = new Mock<IKpiRepository>();
            var sut = new MetricsManagementService(queryServiceMock.Object, reportingServiceMock.Object, repositoryMock.Object);

            // Act
            await sut.QueryAndReport(CancellationToken.None);

            // Assert
            repositoryMock.Verify(x => x.Upsert(It.IsAny<IEnumerable<KpiModel>>()), Times.Exactly(1));
        }

        [Test]
        public async Task ShouldAttemptToStoreReportedData()
        {
            // Arrange
            var queryServiceMock = new Mock<IDataSourceQueryService>();
            queryServiceMock.Setup(x => x.Query())
                .Returns(() => Task.FromResult(new List<Kpi> { new Kpi("k1", null) }.AsEnumerable()));
            var reportingServiceMock = new Mock<IDataSinkReportingService>();
            reportingServiceMock.Setup(x => x.GetSinkNames()).Returns(() => new List<string> { "s1" });
            reportingServiceMock.Setup(x => x.Report(It.IsAny<IEnumerable<KpiToReport>>()))
                .Returns(() =>
                    Task.FromResult(new List<ReportedKpi> { new ReportedKpi(new Kpi("k1", null), "s1", true, null) }
                        .AsEnumerable()));
            var repositoryMock = new Mock<IKpiRepository>();
            repositoryMock.Setup(x => x.GetUnreportedData(It.IsAny<IReadOnlyList<string>>(), It.IsAny<bool>()))
                .Returns(() => Task.FromResult(new List<KpiModel>
                {
                    new KpiModel("s1", "k1", null, DateTime.Now, new List<ReportTargetModel>())
                }.AsEnumerable()));
            var sut = new MetricsManagementService(queryServiceMock.Object, reportingServiceMock.Object,
                repositoryMock.Object);

            // Act
            await sut.QueryAndReport(CancellationToken.None);

            // Assert
            repositoryMock.Verify(x => x.Upsert(It.IsAny<IEnumerable<KpiModel>>()), Times.Exactly(2));
        }
    }

    public class InMemoryKpiRepositoryTests
    {
        [Test]
        public async Task UpsertShouldStoreNewData()
        {
            // Arrange
            var sut = new InMemoryKpiRepository();
            var data = new KpiModel("t1", "t2", "0", DateTime.Now, null);

            // Act
            await sut.Upsert(new[] { data });

            // Assert
            var actual = await sut.GetUnreportedData(new List<string> { "s1" }.AsReadOnly(), true);
            actual.Should().HaveCount(1);
            actual.First().Key.Should().Be(data.Key);
        }

        [Test]
        public async Task UpsertShouldReturCorrectStatistics()
        {
            // Arrange
            var sut = new InMemoryKpiRepository();
            var basicTarget = new ReportTargetModel("s1", DateTime.Now, ReportStatus.Unknown, null);
            var basicModel = new KpiModel("src", "", "0", DateTime.Now, null);

            // Act
            await sut.Upsert(new[]
            {
                basicModel with
                {
                    Key = "success",
                    Targets = new List<ReportTargetModel>
                    {
                        basicTarget with{Status = ReportStatus.Success}
                    }
                },
                basicModel with
                {
                    Key = "failure",
                    Targets = new List<ReportTargetModel>
                    {
                        basicTarget with{Status = ReportStatus.Failure}
                    }
                },
                basicModel with
                {
                    Key = "new",
                }
            });

            // Assert
            var actual = await sut.GetKpiStats();
            actual.Should().NotBeNull();
            actual.TotalFailed.Should().Be(1);
            actual.TotalSent.Should().Be(2);
            actual.TotalSucceeded.Should().Be(1);
            actual.Failed.Should().NotBeNullOrEmpty();
            actual.Failed.Should().HaveCount(1);
            actual.Failed.First().Kpi.Key.Should().Be("failure");
            actual.DistinctReportedKpis.Should().NotBeNullOrEmpty();
            actual.DistinctReportedKpis.Should().HaveCount(3);
        }
    }

    public class EfCoreKpiRepositoryTests
    {
        private static EfCoreKpiRepository SystemUnderTest()
        {
            var inMemoryEfContext = new MetricsContext(
                new DbContextOptionsBuilder()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

            return new EfCoreKpiRepository(inMemoryEfContext);
        }

        [Test]
        public async Task UpsertShouldStoreNewData()
        {
            // Arrange
            var sut = SystemUnderTest();
            var data = new KpiModel("t1", "t2", "0", DateTime.Now, null);

            // Act
            await sut.Upsert(new[] { data });

            // Assert
            var actual = await sut.GetUnreportedData(new List<string> { "s1" }.AsReadOnly(), true);
            actual.Should().HaveCount(1);
            actual.First().Key.Should().Be(data.Key);
        }

        [Test]
        public async Task UpsertShouldReturCorrectStatistics()
        {
            // Arrange
            var sut = SystemUnderTest();
            var basicTarget = new ReportTargetModel("s1", DateTime.Now, ReportStatus.Unknown, null);
            var basicModel = new KpiModel("src", "", "0", DateTime.Now, null);

            // Act
            await sut.Upsert(new[]
            {
                basicModel with
                {
                    Key = "success",
                    Targets = new List<ReportTargetModel>
                    {
                        basicTarget with{Status = ReportStatus.Success}
                    }
                },
                basicModel with
                {
                    Key = "failure",
                    Targets = new List<ReportTargetModel>
                    {
                        basicTarget with{Status = ReportStatus.Failure}
                    }
                },
                basicModel with
                {
                    Key = "new",
                }
            });

            // Assert
            var actual = await sut.GetKpiStats();
            actual.Should().NotBeNull();
            actual.TotalFailed.Should().Be(1);
            actual.TotalSent.Should().Be(2);
            actual.TotalSucceeded.Should().Be(1);
            actual.Failed.Should().NotBeNullOrEmpty();
            actual.Failed.Should().HaveCount(1);
            actual.Failed.First().Kpi.Key.Should().Be("failure");
            actual.DistinctReportedKpis.Should().NotBeNullOrEmpty();
            actual.DistinctReportedKpis.Should().HaveCount(3);
        }
    }

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
