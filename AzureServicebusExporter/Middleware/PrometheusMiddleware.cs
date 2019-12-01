using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace AzureServicebusExporter.Middleware
{
    public static class PrometheusMiddleware
    {
        private static string _clientId = "b2be284a-e99e-4b29-b4f0-8855558e5334";
        private static string _clientSecret = "Welkom01";
        private static string _tenantId = "208cebd9-57bb-4455-9d1c-478abebe72b6";
        private static string _subscriptionId = "331e081a-586a-497a-befd-ee049a31d234";
        private static string _resourceGroupName = "Swarm";
        private static string _resourceName = "dockerswarm";

        public static Func<HttpContext, Func<Task>, Task> Get(IApplicationBuilder app)
        {
            Metrics.SuppressDefaultMetrics();

            return (context, next) =>
            {
                if (!context.Request.Path.HasValue || context.Request.Path.Value != "/metrics")
                {
                    return next();
                }

                var gaugeModels = new List<GaugeModel>();
                var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
                var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _subscriptionId);

                var @namespace = serviceBusManager.Namespaces.GetByResourceGroup(_resourceGroupName, _resourceName);

                var queueService = app.ApplicationServices.GetService<IQueueService>();
                gaugeModels.AddRange(queueService.CreateMetricsAsync(@namespace).GetAwaiter().GetResult());

                var topicService = app.ApplicationServices.GetService<ITopicService>();
                gaugeModels.AddRange(topicService.CreateMetricsAsync(@namespace).GetAwaiter().GetResult());

                var subscriptionService = app.ApplicationServices.GetService<ISubscriptionService>();
                gaugeModels.AddRange(subscriptionService.CreateMetricsAsync(@namespace).GetAwaiter().GetResult());

                foreach (var gaugeModel in gaugeModels)
                {
                    var gauge = Metrics.CreateGauge(gaugeModel.Name, gaugeModel.Help, new GaugeConfiguration()
                    {
                        LabelNames = gaugeModel.Labels
                    });
                    gauge.WithLabels(gaugeModel.LabelValues).Set(gaugeModel.Value);
                }

                return next();
            };
        }
    }
}
