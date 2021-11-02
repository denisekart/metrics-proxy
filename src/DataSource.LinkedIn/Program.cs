using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSource.LinkedIn
{
    public class LinkedInDataSource : IDataSource
    {
        private readonly IConfigurationAccessor<LinkedInDataSource> _configuration;

        public LinkedInDataSource(IConfigurationAccessor<LinkedInDataSource> configuration)
        {
            _configuration = configuration;
        }
        public string Name => "Linkedin";
        public Task<IEnumerable<Kpi>> Query()
        {
            throw new NotImplementedException();
        }
    }
}
