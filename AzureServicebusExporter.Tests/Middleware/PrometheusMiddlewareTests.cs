using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Middleware;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Prometheus;
using Xunit;

namespace AzureServicebusExporter.Tests.Middleware
{
    public class PrometheusMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _requestDelegateMock;
        private readonly Mock<ILogger<PrometheusMiddleware>> _iLoggerMock;
        private readonly Mock<IQueueService> _iQueueServiceMock;
        private readonly Mock<ITopicService> _iTopicServiceMock;
        private readonly Mock<ISubscriptionService> _iSubscriptionServiceMock;
        private readonly Mock<IOptions<AzureServicebusExporterConfig>> _iOptionsMock;
        private readonly Mock<IServiceBusNamespace> _iServicebusNamespace;


        public PrometheusMiddlewareTests()
        {
            _requestDelegateMock = new Mock<RequestDelegate>();
            _iLoggerMock = new Mock<ILogger<PrometheusMiddleware>>();
            _iQueueServiceMock = new Mock<IQueueService>();
            _iTopicServiceMock = new Mock<ITopicService>();
            _iSubscriptionServiceMock = new Mock<ISubscriptionService>();
            _iOptionsMock = new Mock<IOptions<AzureServicebusExporterConfig>>();
            _iServicebusNamespace = new Mock<IServiceBusNamespace>();
        }

        [Fact]
        public async Task Invoke_Should_Be_Successful()
        {
            
            Metrics.SuppressDefaultMetrics();

            PrometheusMiddleware prometheusMiddleware = new PrometheusMiddleware(
                _requestDelegateMock.Object,
                _iLoggerMock.Object,
                _iQueueServiceMock.Object,
                _iTopicServiceMock.Object,
                _iSubscriptionServiceMock.Object,
                _iOptionsMock.Object,
                _iServicebusNamespace.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/metrics";

            _iQueueServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>()
            {
                new GaugeModel()
                {
                 Name = "Queue",
                 Help = "Queue Help",
                 Labels = new []{"env"},
                 LabelValues = new []{"dev"},
                 Value = 200
                }
            });

            _iSubscriptionServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>()
            {
                new GaugeModel()
                {
                    Name = "Subscription",
                    Help = "Subscription Help",
                    Labels = new []{"env"},
                    LabelValues = new []{"dev"},
                    Value = 300
                }
            });

            _iTopicServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>()
            {
                new GaugeModel()
                {
                    Name = "Topic",
                    Help = "Topic Help",
                    Labels = new []{"env"},
                    LabelValues = new []{"dev"},
                    Value = 400
                }
            });

            await prometheusMiddleware.Invoke(context);

            var lines = await GetMetrics();
            
            Assert.Contains(lines, s => s == "Queue{env=\"dev\"} 200");
            Assert.Contains(lines, s => s == "Subscription{env=\"dev\"} 300");
            Assert.Contains(lines, s => s == "Topic{env=\"dev\"} 400");
            Assert.Contains(lines, s => s == "azureservicebusexporter_up 1");
            Assert.Contains(lines, s => s.StartsWith("scrape_duration_milliseconds "));

            _iQueueServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
            _iSubscriptionServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
            _iTopicServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
        }

        [Fact]
        public async Task Invoke_Should_Be_Fail()
        {
            Metrics.SuppressDefaultMetrics();

            PrometheusMiddleware prometheusMiddleware = new PrometheusMiddleware(
                _requestDelegateMock.Object,
                _iLoggerMock.Object,
                _iQueueServiceMock.Object,
                _iTopicServiceMock.Object,
                _iSubscriptionServiceMock.Object,
                _iOptionsMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/metrics";

            await prometheusMiddleware.Invoke(context);

            var lines = await GetMetrics();
            Assert.Contains(lines, s => s == "azureservicebusexporter_up 0");
        }

        private static async Task<string[]> GetMetrics()
        {
            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream))
            {
                await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return streamReader.ReadToEnd().Split('\n');
            }
        }
    }
}
