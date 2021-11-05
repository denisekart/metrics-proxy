using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSink.Multiple
{
    public class SinkOptions
    {
        public string FailureRatePercent { get; set; }
    }

    /// <summary>
    /// This sink doesn't actually do anything but fail on occasion :)
    /// </summary>
    public class RandomlyFailingDataSink : IDataSink
    {
        private readonly IConfigurationAccessor<RandomlyFailingDataSink> _configuration;
        private readonly Random _random;
        public string Name => "RandomlyFailing";

        public RandomlyFailingDataSink(IConfigurationAccessor<RandomlyFailingDataSink> configuration)
        {
            _configuration = configuration;
            _random = new Random((int) DateTime.Now.Ticks % int.MaxValue);
        }
        public Task Report(IEnumerable<Kpi> items)
        {
            var failureRate = int.TryParse(_configuration.Get<SinkOptions>(this)?.FailureRatePercent, out var i ) ? i : 0;
            if (_random.Next(0, 100) < failureRate)
            {
                throw new Exception($"I have just failed randomly and took {items.Count()} victims with me. Don't like these odds? Change the failure rate!");
            }

            return Task.CompletedTask;
        }
    }
}