using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetricsProxy.Contracts;

namespace DataSink.Databox
{
    public class DataboxDataSink : IDataSink
    {
        private readonly IConfigurationAccessor<DataboxDataSink> _configuration;

        public DataboxDataSink(IConfigurationAccessor<DataboxDataSink> configuration)
        {
            _configuration = configuration;
        }
        public string Name => "Databox";
        public Task Report(IEnumerable<Kpi> items)
        {
            throw new NotImplementedException();
        }
    }
}
