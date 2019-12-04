using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace AzureServicebusExporter.Middleware
{
    public class PrometheusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PrometheusMiddleware> _logger;
        private readonly IQueueService _queueService;
        private readonly ITopicService _topicService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAzureAuthenticationService _authenticationService;
        private readonly IOptions<AzureServicebusExporterConfig> _config;
        private IServiceBusNamespace _namespace;

        public PrometheusMiddleware(
            RequestDelegate next, 
            ILogger<PrometheusMiddleware> logger, 
            IQueueService queueService, 
            ITopicService topicService, 
            ISubscriptionService subscriptionService,
            IAzureAuthenticationService authenticationService,
            IOptions<AzureServicebusExporterConfig> config)
        {
            
            _next = next;
            _logger = logger;
            _queueService = queueService;
            _topicService = topicService;
            _subscriptionService = subscriptionService;
            _authenticationService = authenticationService;
            _config = config;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value == "/metrics")
            {
                _logger.LogTrace("Start scraping");

                var status = 1;
                try
                {
                    GuardClauses.IsNotNullOrEmpty(_config.Value.ClientId, nameof(_config.Value.ClientId));
                    GuardClauses.IsNotNullOrEmpty(_config.Value.ClientSecret, nameof(_config.Value.ClientSecret));
                    GuardClauses.IsNotNullOrEmpty(_config.Value.TenantId, nameof(_config.Value.TenantId));
                    GuardClauses.IsNotNullOrEmpty(_config.Value.SubscriptionId, nameof(_config.Value.SubscriptionId));
                    GuardClauses.IsNotNullOrEmpty(_config.Value.ResourceGroupName, nameof(_config.Value.ResourceGroupName));
                    GuardClauses.IsNotNullOrEmpty(_config.Value.ResourceName, nameof(_config.Value.ResourceName));

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    var gaugeModels = new List<GaugeModel>();
                    
                    if (_namespace == null)
                    {
                        var serviceBusManager = _authenticationService.Authenticate(_config.Value.ClientId, _config.Value.ClientSecret, _config.Value.TenantId, _config.Value.SubscriptionId);
                        _namespace = serviceBusManager.Namespaces.GetByResourceGroup(_config.Value.ResourceGroupName, _config.Value.ResourceName);
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
                }
                catch (Exception e)
                {
                    status = 0;
                    _logger.LogError(e.Message);
                }
                finally
                {
                    var gauge = Metrics.CreateGauge("azureservicebusexporter_up","The status if the scrape was successful", new GaugeConfiguration());
                    gauge.Set(status);

                    _logger.LogTrace($"End scraping");
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
