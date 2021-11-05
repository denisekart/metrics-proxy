using Microsoft.EntityFrameworkCore;
using System;

namespace MetricsProxy.Application.Peripherals.Ef
{
    public class MetricsContext : DbContext
    {
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricTarget> MetricTargets { get; set; }

        public MetricsContext(DbContextOptions options) : base(options)
        {
            
        }

        public static string DbPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create)}{System.IO.Path.DirectorySeparatorChar}metricsProxy.db";
    }
}
