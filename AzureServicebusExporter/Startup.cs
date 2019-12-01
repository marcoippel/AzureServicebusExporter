using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Middleware;
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
            services.AddSingleton<IPrometheusMiddlewareService, PrometheusMiddlewareService>();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var prometheusMiddlewareService = app.ApplicationServices.GetService<IPrometheusMiddlewareService>();

            app.Use(prometheusMiddlewareService.Get(app));
            app.UseHttpsRedirection();
            app.UseMetricServer();
        }
    }
}
