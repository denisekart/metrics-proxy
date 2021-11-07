using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MetricsProxy.Application.Domain;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace MetricsProxy.Tests.Application
{
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
}