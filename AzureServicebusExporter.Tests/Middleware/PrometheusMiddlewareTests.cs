using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Middleware;
using AzureServicebusExporter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
            //RequestDelegate next,
            //    ILogger< PrometheusMiddleware > logger, 
            //IQueueService queueService,
            //    ITopicService topicService, 
            //ISubscriptionService subscriptionService,
            //    IOptions< AzureServicebusExporterConfig > config

            var requestDelegateMock = new Mock<RequestDelegate>();
            var iLoggerMock = new Mock<ILogger<PrometheusMiddleware>>();
            var iQueueServiceMock = new Mock<IQueueService>();
            var iTopicServiceMock = new Mock<ITopicService>();
            var iSubscriptionServiceMock = new Mock<ISubscriptionService>();
            var iOptionsMock = new Mock<IOptions<AzureServicebusExporterConfig>>();



            PrometheusMiddleware prometheusMiddleware = new PrometheusMiddleware(
                requestDelegateMock.Object, 
                iLoggerMock.Object,
                iQueueServiceMock.Object,
                iTopicServiceMock.Object,
                iSubscriptionServiceMock.Object, 
                iOptionsMock.Object);

            await prometheusMiddleware.Invoke(new DefaultHttpContext());
        }
    }
}
