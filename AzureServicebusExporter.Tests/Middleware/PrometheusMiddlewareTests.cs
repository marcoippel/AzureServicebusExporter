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
        [Fact]
        public async Task Invoke()
        {
            var requestDelegateMock = new Mock<RequestDelegate>();
            var loggerMock = new Mock<ILogger<PrometheusMiddleware>>();
            var queueServiceMock = new Mock<IQueueService>();
            var topicServiceMock = new Mock<ITopicService>();
            var subscriptionServiceMock = new Mock<ISubscriptionService>();
            var optionsMock = new Mock<IOptions<AzureServicebusExporterConfig>>();
            var azureAuthenticationServiceMock = new Mock<IAzureAuthenticationService>();
            var servicebusNameSpace = new Mock<IServiceBusNamespace>();
            var serviceBusManagerMock = new Mock<IServiceBusManager>();
            
            serviceBusManagerMock
                .Setup(x => x.Namespaces.GetByResourceGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(servicebusNameSpace.Object);

            queueServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>());
            topicServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>());
            subscriptionServiceMock.Setup(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>())).ReturnsAsync(new List<GaugeModel>());

            optionsMock.Setup(x => x.Value).Returns(new AzureServicebusExporterConfig()
            {
                ClientId = "Id",
                ClientSecret = "Secret",
                SubscriptionId = "SubscriptionId",
                TenantId = "TenantId",
                ResourceGroupName = "RG",
                ResourceName = "RN"
            });

            azureAuthenticationServiceMock.Setup(x =>
                x.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(serviceBusManagerMock.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = new PathString("/metrics");

            PrometheusMiddleware prometheusMiddleware = new PrometheusMiddleware(
                requestDelegateMock.Object, 
                loggerMock.Object,
                queueServiceMock.Object,
                topicServiceMock.Object,
                subscriptionServiceMock.Object,
                azureAuthenticationServiceMock.Object,
                optionsMock.Object);

            await prometheusMiddleware.Invoke(context);

            queueServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
            topicServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);
            subscriptionServiceMock.Verify(x => x.CreateMetricsAsync(It.IsAny<IServiceBusNamespace>()), Times.Once);

            loggerMock.Verify(m => m.Log(It.Is<LogLevel>(l => l == LogLevel.Trace), 0, It.Is<FormattedLogValues>(v => v.ToString() == "Start scraping"), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            loggerMock.Verify(m => m.Log(It.Is<LogLevel>(l => l == LogLevel.Trace), 0, It.Is<FormattedLogValues>(v => v.ToString() == "End scraping"), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }
    }
}
