using System.Linq;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Configuration;

namespace MetricsProxy.Web.Services
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
            else
            {
                _sectionValue = ttype.Name;
            }
        }
        public T Get<T>(TService instance, string path) where T: class, new()
        {
            return _configuration.GetSection(
                string.Join(":",new[] { _sectionValue, instance.Name, path }.OfType<string>()))
                .Get<T>();
        }
    }
}
