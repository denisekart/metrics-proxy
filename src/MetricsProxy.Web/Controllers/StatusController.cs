using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MetricsProxy.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        public record Status(string BackgroundServiceStatus, KpiStats ReportingStatistics);
        
        private readonly ILogger<StatusController> _logger;
        private readonly IBackgroundServiceTracker _backgroundServiceTracker;
        private readonly IKpiRepository _kpiRepository;

        public StatusController(ILogger<StatusController> logger, IBackgroundServiceTracker backgroundServiceTracker, IKpiRepository kpiRepository)
        {
            _logger = logger;
            _backgroundServiceTracker = backgroundServiceTracker;
            _kpiRepository = kpiRepository;
        }

        [HttpGet]
        public async Task<Status> GetStatus()
        {
            return new Status(_backgroundServiceTracker.Query(), await _kpiRepository.GetKpiStats());
        }
    }
}
