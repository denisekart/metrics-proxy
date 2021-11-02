using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Configuration;

namespace MetricsProxy.Web
{
    public class DefaultWebConfigurationAccessor<TService> : IConfigurationAccessor<TService> where TService : INamedService
    {
        private readonly IConfiguration _configuration;
        private readonly string _sectionValue;
        public DefaultWebConfigurationAccessor(IConfiguration configuration)
        {
            _configuration = configuration;
            var ttype = typeof(TService);
            if (ttype.IsAssignableTo(typeof(IDataSource)))
            {
                _sectionValue = "DataSource";
            }
            else if(ttype.IsAssignableTo(typeof(IDataSink)))
            {
                _sectionValue = "DataSink";
            }

            _sectionValue = string.Empty;
        }
        public T Get<T>(TService instance, string path)
        {
            return _configuration.GetValue<T>(string.Join(":",
                new[] {_sectionValue, instance.Name, path}.OfType<string>()));
        }
    }
}
