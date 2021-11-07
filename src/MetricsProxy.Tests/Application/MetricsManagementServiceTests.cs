using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Domain;
using MetricsProxy.Application.Models;
using MetricsProxy.Contracts;
using Moq;
using NUnit.Framework;

namespace MetricsProxy.Tests.Application
{
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
}