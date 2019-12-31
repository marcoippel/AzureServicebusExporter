using AzureServicebusExporter.Configuration;
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
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddDockerSecrets("C:\\temp\\secrets");
            
                Configuration = builder.Build();
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
