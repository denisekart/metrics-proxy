using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MetricsProxy.Application.Domain;
using MetricsProxy.Application.Models;
using MetricsProxy.Contracts;
using Moq;
using NUnit.Framework;

namespace MetricsProxy.Tests.Application
{
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
}