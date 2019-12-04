using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Middleware;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AzureServicebusExporter.Tests.Middleware
{
    public class PrometheusMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _requestDelegateMock;
        private readonly Mock<ILogger<PrometheusMiddleware>> _loggerMock;
        private readonly Mock<IQueueService> _queueServiceMock;
        private readonly Mock<ITopicService> _topicServiceMock;
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<IOptions<AzureServicebusExporterConfig>> _optionsMock;
        private readonly Mock<IAzureAuthenticationService> _azureAuthenticationServiceMock;
        private readonly Mock<IServiceBusNamespace> _servicebusNameSpace;
        private readonly Mock<IServiceBusManager> _serviceBusManagerMock;

        public PrometheusMiddlewareTests()
        {
            _requestDelegateMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<PrometheusMiddleware>>();
            _queueServiceMock = new Mock<IQueueService>();
            _topicServiceMock = new Mock<ITopicService>();
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _optionsMock = new Mock<IOptions<AzureServicebusExporterConfig>>();
            _azureAuthenticationServiceMock = new Mock<IAzureAuthenticationService>();
            _servicebusNameSpace = new Mock<IServiceBusNamespace>();
            _serviceBusManagerMock = new Mock<IServiceBusManager>();
        }

        [Fact]
        public async Task Invoke()
        {
            

            _serviceBusManagerMock
                .Setup(x => x.Namespaces.GetByResourceGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(_servicebusNameSpace.Object);

            _queueServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>());
            _topicServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>());
            _subscriptionServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>());

            _optionsMock.Setup(x => x.Value).Returns(new AzureServicebusExporterConfig()
            {
                ClientId = "Id",
                ClientSecret = "Secret",
                SubscriptionId = "SubscriptionId",
                TenantId = "TenantId",
                ResourceGroupName = "RG",
                ResourceName = "RN"
            });

            _azureAuthenticationServiceMock.Setup(x =>
                x.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_serviceBusManagerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = new PathString("/metrics");

            PrometheusMiddleware prometheusMiddleware = new PrometheusMiddleware(
                _requestDelegateMock.Object, 
                _loggerMock.Object,
                _queueServiceMock.Object,
                _topicServiceMock.Object,
                _subscriptionServiceMock.Object,
                _azureAuthenticationServiceMock.Object,
                _optionsMock.Object);

            await prometheusMiddleware.Invoke(context);

            _queueServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
            _topicServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);

            _loggerMock.Verify(m => m.Log(It.Is<LogLevel>(l => l == LogLevel.Trace), 0, It.Is<FormattedLogValues>(v => v.ToString() == "Start scraping"), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(m => m.Log(It.Is<LogLevel>(l => l == LogLevel.Trace), 0, It.Is<FormattedLogValues>(v => v.ToString() == "End scraping"), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }
    }
}
