using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Middleware;
using AzureServicebusExporter.Models;
using AzureServicebusExporter.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace AzureServicebusExporter
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
            services.AddLogging();
            services.AddTransient<IQueueService, QueueService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<IAzureAuthenticationService, AzureAuthenticationService>();
            services.Configure<AzureServicebusExporterConfig>(Configuration.GetSection("AzureServicebusExporter"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            Metrics.SuppressDefaultMetrics();

            app.UseHttpsRedirection();
            app.UsePrometheusMiddleware();
            app.UseMetricServer();
        }
    }
}
