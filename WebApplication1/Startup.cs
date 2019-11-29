using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1
{
    public class Startup
    {
        private static string _clientId = "b2be284a-e99e-4b29-b4f0-8855558e5334";
        private static string _clientSecret = "Welkom01";
        private static string _tenantId = "208cebd9-57bb-4455-9d1c-478abebe72b6";
        private static string _subscriptionId = "331e081a-586a-497a-befd-ee049a31d234";
        private static string _resourceGroupName = "Swarm";
        private static string _resourceName = "dockerswarm";

        private IServiceBusNamespace _namespace;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddTransient<IQueueService, QueueService>();
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
            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
            var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _subscriptionId);

            _namespace = serviceBusManager.Namespaces.GetByResourceGroup(_resourceGroupName, _resourceName);
            var _queueService = app.ApplicationServices.GetService<IQueueService>();
            var queueMetrics = _queueService.CreateMetricsAsync(_namespace).GetAwaiter().GetResult();

            

            Metrics.SuppressDefaultMetrics();
            var gauge = Metrics.CreateGauge("PathCounter", "Counts requests to endpoints", new GaugeConfiguration()
            {
                LabelNames = new[] { "method", "endpoint" }
            });

            foreach (var queueMetric in queueMetrics)
            {
                app.Use((context, next) =>
                {
                    CreateGaugeMetric(queueMetric);
                    //gauge.WithLabels(context.Request.Method, context.Request.Path).Inc();
                    return next();
                });

            }
            app.Use((context, next) =>
            {
                gauge.WithLabels(context.Request.Method, context.Request.Path).Inc();
                return next();
            });

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseMetricServer();
        }

        private void CreateGaugeMetric(GaugeModel queueMetric)
        {
            var gauge = Metrics.CreateGauge(queueMetric.Name, queueMetric.Help, new GaugeConfiguration()
            {
                LabelNames = queueMetric.Labels
            });
            gauge.WithLabels(queueMetric.LabelValues).Set(queueMetric.Value);

        }
    }
}
