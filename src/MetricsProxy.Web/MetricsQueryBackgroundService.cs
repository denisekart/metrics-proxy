using MetricsProxy.Application.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MetricsProxy.Web
{
    public record QueryServiceOptions()
    {
        public int IntervalInMilliseconds { get; init; } = 10000;
    }

    public class MetricsQueryBackgroundService : BackgroundService
    {
        private readonly IOptions<QueryServiceOptions> _options;
        private readonly IMetricsManagementService _metricsManagementService;
        private readonly ILogger<MetricsQueryBackgroundService> _logger;

        public MetricsQueryBackgroundService(IOptions<QueryServiceOptions> options, IMetricsManagementService metricsManagementService, ILogger<MetricsQueryBackgroundService> logger)
        {
            _options = options;
            _metricsManagementService = metricsManagementService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _metricsManagementService.QueryAndReport(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"An exception occurred in the background service. The current cycle may not have been completed. Will retry in {_options.Value.IntervalInMilliseconds/100} seconds!");
                }
                finally
                {
                    await Task.Delay(_options.Value.IntervalInMilliseconds, stoppingToken);
                }
            }
        }
    }
}