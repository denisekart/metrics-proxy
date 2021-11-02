using DataSink.Databox;
using DataSource.Github;
using DataSource.LinkedIn;
using MetricsProxy.Application.Domain;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
        public static IServiceCollection AddExternalServicesConfigurationFactory(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IConfigurationAccessor<>), typeof(DefaultWebConfigurationAccessor<>));

            return services;
        }

        public static IServiceCollection AddDataSources(this IServiceCollection services)
        {
            services.AddSingleton<IDataSource, GithubDataSource>();
            services.AddSingleton<IDataSource, LinkedInDataSource>();
            //TODO: register an arbitrary data source type

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
