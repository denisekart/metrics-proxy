using DataSink.Multiple;
using DataSource.Multiple;
using MetricsProxy.Application.Peripherals.Ef;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MetricsProxy.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MetricsProxy.Web", Version = "v1" });
            });

            // configure all metrics proxy services
            services
                .AddCoreMetricsProxyServices(Configuration)
                .AddExternalServicesConfigurationFactory()
                .AddDataSourcesFromAssembly<GithubDataSource>()
                .AddDataSinksFromAssembly<DataboxDataSink>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetricsProxy.Web v1"));
            }

            // Configure entity framework based on configurations
            // WARNING: Please don't ever use this code in a production system!!
            if (Configuration.UseEfCoreDatabase())
            {
                using var efScope = app.ApplicationServices.CreateScope();
                var db = efScope.ServiceProvider.GetRequiredService<MetricsContext>();

                // Delete old instance of the database when configured in that way
                if (Configuration.UseEfCoreDatabaseReset())
                    db.Database.EnsureDeleted();

                // always ensure that the database exists
                db.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // at this point, we missed all of our middlewares. Just write something funny.
            app.Run(ctx => ctx.Response.WriteAsync("Whoops, you missed an endpoint! <br/> See '/README.md' on what to do next. or visit the /swagger endpoint for API documentation."));
        }
    }
}
