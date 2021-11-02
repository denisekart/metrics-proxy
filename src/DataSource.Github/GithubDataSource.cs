using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSource.Github
{
    public class GithubDataSource : IDataSource
    {
        private readonly IConfigurationAccessor<GithubDataSource> _configuration;

        public GithubDataSource(IConfigurationAccessor<GithubDataSource> configuration)
        {
            _configuration = configuration;
        }
        public string Name => "Github";
        public Task<IEnumerable<Kpi>> Query()
        {
            throw new NotImplementedException();
        }
    }
}
