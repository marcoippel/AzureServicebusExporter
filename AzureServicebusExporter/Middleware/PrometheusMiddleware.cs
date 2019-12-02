using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace AzureServicebusExporter.Middleware
{
    public class PrometheusMiddleware
    {
        private readonly string _clientId = "b2be284a-e99e-4b29-b4f0-8855558e5334";
        private readonly string _clientSecret = "Welkom01";
        private readonly string _tenantId = "208cebd9-57bb-4455-9d1c-478abebe72b6";
        private readonly string _subscriptionId = "9a683748-58c6-48fc-a0c1-920a1004270f";
        private readonly string _resourceGroupName = "Swarm";
        private readonly string _resourceName = "dockerswarm";
        private readonly RequestDelegate _next;
        private readonly IQueueService _queueService;
        private readonly ITopicService _topicService;
        private readonly ISubscriptionService _subscriptionService;
        private IServiceBusNamespace _namespace;

        public PrometheusMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IQueueService queueService, ITopicService topicService, ISubscriptionService subscriptionService)
        {
            
            _next = next;
            _queueService = queueService;
            _topicService = topicService;
            _subscriptionService = subscriptionService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value != "/metrics")
            {
                await _next.Invoke(httpContext);
            }
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var gaugeModels = new List<GaugeModel>();

            if (_namespace == null)
            {
                var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
                var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _subscriptionId);
                _namespace = serviceBusManager.Namespaces.GetByResourceGroup(_resourceGroupName, _resourceName);
            }

            gaugeModels.AddRange(_queueService.CreateMetricsAsync(_namespace).GetAwaiter().GetResult());
            gaugeModels.AddRange(_topicService.CreateMetricsAsync(_namespace).GetAwaiter().GetResult());
            gaugeModels.AddRange(_subscriptionService.CreateMetricsAsync(_namespace).GetAwaiter().GetResult());

            gaugeModels.Add(new GaugeModel()
            {
                Name = "scrape_duration_milliseconds",
                Value = (sw.ElapsedMilliseconds),
                Help = "The duration of the scrape in seconds",
            });

            foreach (var gaugeModel in gaugeModels)
            {
                var gauge = Metrics.CreateGauge(gaugeModel.Name, gaugeModel.Help, new GaugeConfiguration()
                {
                    LabelNames = gaugeModel.Labels
                });
                
                gauge.WithLabels(gaugeModel.LabelValues).Set(gaugeModel.Value);
            }

            await _next.Invoke(httpContext);
        }
    }

    public static class PrometheusMiddlewareExtensions
    {
        public static IApplicationBuilder UsePrometheusMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PrometheusMiddleware>();
        }
    }
}
