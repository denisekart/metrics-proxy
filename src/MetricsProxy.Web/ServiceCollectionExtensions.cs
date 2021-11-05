using System;
using DataSink.Databox;
using MetricsProxy.Application.Domain;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using MetricsProxy.Application.Application;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Peripherals;
using MetricsProxy.Web.Services;

namespace MetricsProxy.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreMetricsProxyServices(this IServiceCollection services, IConfiguration configuration)
        {
            // add the background service
            services.AddOptions<QueryServiceOptions>()
                .Configure(opts => configuration.GetSection("QueryService").Bind(opts));
            services.AddHostedService<MetricsQueryBackgroundService>();

            // add processing service
            services.AddSingleton<IMetricsManagementService, MetricsManagementService>();
            services.AddSingleton<IDataSourceQueryService, DataSourceQueryService>();
            services.AddSingleton<IDataSinkReportingService, DataSinkReportingService>();
            services.AddSingleton<IKpiRepository, InMemoryKpiRepository>();

            services.AddHttpClient();
            services.AddSingleton<IBackgroundServiceTracker, DefaultBackgroundServiceTracker>();

            return services;
        }
        public static IServiceCollection AddExternalServicesConfigurationFactory(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IConfigurationAccessor<>), typeof(DefaultWebConfigurationAccessor<>));

            return services;
        }

        public static IServiceCollection AddDataSourcesFromAssembly<TTypeInAssembly>(this IServiceCollection services)
        {
            var assembly = typeof(TTypeInAssembly).Assembly;
            var dataSourceTypes = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(IDataSource)));
            foreach (var dataSourceType in dataSourceTypes
                .Where(x=>x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null))
            {
                services.AddSingleton(typeof(IDataSource), dataSourceType);
            }

            return services;
        }

        public static IServiceCollection AddDataSinks(this IServiceCollection services)
        {
            services.AddSingleton<IDataSink, DataboxDataSink>();
            //TODO: register an arbitrary data sink type

            return services;
        }
    }
}
