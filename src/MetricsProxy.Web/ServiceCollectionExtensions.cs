using System;
using MetricsProxy.Application.Domain;
using MetricsProxy.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using MetricsProxy.Application.Application;
using MetricsProxy.Application.Contracts;
using MetricsProxy.Application.Peripherals;
using MetricsProxy.Application.Peripherals.Ef;
using MetricsProxy.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace MetricsProxy.Web
{
    public static class ServiceCollectionExtensions
    {
        public static bool UseEfCoreDatabase(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("DatabaseProvider").StartsWith("EfCore", StringComparison.OrdinalIgnoreCase);
        }

        public static bool UseEfCoreDatabaseReset(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("DatabaseProvider").Equals("EfCore_Reset", StringComparison.OrdinalIgnoreCase);
        }


        public static bool UseInMemoryDatabase(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("DatabaseProvider") switch
            {
                var x when 
                    x==null || 
                    x.Trim() == string.Empty || 
                    x.ToLower() == "inmemory"
                    => true,
                    _ => false
            };
        }

        public static IServiceCollection AddCoreMetricsProxyServices(this IServiceCollection services, IConfiguration configuration)
        {
            // add the background service
            services.AddOptions<QueryServiceOptions>()
                .Configure(opts => configuration.GetSection("QueryService").Bind(opts));
            services.AddHostedService<MetricsQueryBackgroundService>();

            // add processing service
            services.AddScoped<IMetricsManagementService, MetricsManagementService>();
            services.AddScoped<IDataSourceQueryService, DataSourceQueryService>();
            services.AddScoped<IDataSinkReportingService, DataSinkReportingService>();

            // add data access services
            if(configuration.UseEfCoreDatabase())
                services.AddDbContext<MetricsContext>(opts => opts.UseSqlite($"Data Source={MetricsContext.DbPath}"));

            _=configuration switch
            {
                var c when c.UseInMemoryDatabase() => services.AddScoped<IKpiRepository, InMemoryKpiRepository>(),
                var c when c.UseEfCoreDatabase() => services.AddScoped<IKpiRepository, EfCoreKpiRepository>(),
                _ => throw new ArgumentException("Invalid database type specified in configuration.")
            };

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

        public static IServiceCollection AddDataSinksFromAssembly<TTypeInAssembly>(this IServiceCollection services)
        {
            var assembly = typeof(TTypeInAssembly).Assembly;
            var dataSourceTypes = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(IDataSink)));
            foreach (var dataSourceType in dataSourceTypes
                .Where(x => x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null))
            {
                services.AddSingleton(typeof(IDataSink), dataSourceType);
            }

            return services;
        }
    }
}
