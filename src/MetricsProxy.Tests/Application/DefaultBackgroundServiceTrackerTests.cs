using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MetricsProxy.Application.Application;
using NUnit.Framework;

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
}
