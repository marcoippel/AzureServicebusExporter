using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

[assembly: InternalsVisibleTo("AzureServicebusExporter.Tests")]
namespace AzureServicebusExporter.Middleware
{
    public class PrometheusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PrometheusMiddleware> _logger;
        private readonly IQueueService _queueService;
        private readonly ITopicService _topicService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IOptions<AzureServicebusExporterConfig> _config;
        private IServiceBusNamespace _serviceBusNamespace;

        public PrometheusMiddleware(
            RequestDelegate next, 
            ILogger<PrometheusMiddleware> logger, 
            IQueueService queueService, 
            ITopicService topicService, 
            ISubscriptionService subscriptionService,
            IOptions<AzureServicebusExporterConfig> config,
            IServiceBusNamespace serviceBusNamespace = null)
        {
            
            _next = next;
            _logger = logger;
            _queueService = queueService;
            _topicService = topicService;
            _subscriptionService = subscriptionService;
            _config = config;
            _serviceBusNamespace = serviceBusNamespace;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value == "/metrics")
            {
                _logger.LogTrace($"{DateTime.Now:o} - Start scraping");
                
                var status = 1;
                try
                {
                    //todo: validate settings

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    var gaugeModels = new List<GaugeModel>();
                    
                    if (_serviceBusNamespace == null)
                    {
                        var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_config.Value.ClientId, _config.Value.ClientSecret, _config.Value.TenantId,AzureEnvironment.AzureGlobalCloud);
                        var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _config.Value.SubscriptionId);
                        _serviceBusNamespace = serviceBusManager.Namespaces.GetByResourceGroup(_config.Value.ResourceGroupName, _config.Value.ResourceName);
                    }

                    gaugeModels.AddRange(_queueService.CreateMetricsAsync(_serviceBusNamespace).GetAwaiter().GetResult());
                    gaugeModels.AddRange(_topicService.CreateMetricsAsync(_serviceBusNamespace).GetAwaiter().GetResult());
                    gaugeModels.AddRange(_subscriptionService.CreateMetricsAsync(_serviceBusNamespace).GetAwaiter().GetResult());

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
                }
                catch (Exception e)
                {
                    status = 0;
                    _logger.LogError($"{DateTime.Now:o} - {e.Message}");
                }
                finally
                {
                    var gauge = Metrics.CreateGauge("azureservicebusexporter_up",
                        "The status if the scrape was successful", new GaugeConfiguration());
                    gauge.Set(status);

                    _logger.LogTrace($"{DateTime.Now:o} - End scraping");
                }
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
