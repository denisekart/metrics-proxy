using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MetricsProxy.Application.Models;
using MetricsProxy.Application.Peripherals;
using MetricsProxy.Application.Peripherals.Ef;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MetricsProxy.Tests.Application
{
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
}