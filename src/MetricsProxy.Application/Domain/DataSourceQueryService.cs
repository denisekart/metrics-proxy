using MetricsProxy.Application.Contracts;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Domain
{
    public class DataSourceQueryService : IDataSourceQueryService
    {
        private readonly IEnumerable<IDataSource> _dataSources;
        private readonly ILogger<DataSourceQueryService> _logger;

        public DataSourceQueryService(IEnumerable<IDataSource> dataSources, ILogger<DataSourceQueryService> logger)
        {
            _dataSources = dataSources;
            _logger = logger;
        }

        public async Task<IEnumerable<Kpi>> Query()
        {
            var result = new List<Kpi>();
            foreach (var dataSource in _dataSources)
            {
                try
                {
                    var dsResult = await dataSource.Query();
                    result.AddRange(dsResult.Select(x => x with
                    {
                        Source = dataSource.Name,
                        CreatedOn = x.CreatedOn ?? DateTime.Now
                    }));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to query service '{dataSource.Name}'.");
                }
            }

            return result;
        }
    }
}
