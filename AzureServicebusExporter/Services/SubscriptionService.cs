using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AzureServicebusExporter.Helpers;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;

namespace AzureServicebusExporter.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILogger<SubscriptionService> _logger;
        private readonly ITopicService _topicService;

        public SubscriptionService(ITopicService topicService, ILogger<SubscriptionService> logger)
        {
            _logger = logger;
            _topicService = topicService;
        }

        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var topics = await _topicService.GetTopicsAsync(serviceBusNamespace);
            _logger.Log(LogLevel.Information, $"SubscriptionService GetTopicsAsync: {sw.ElapsedMilliseconds}");
            List<GaugeModel> gaugeModels = new List<GaugeModel>();

            foreach (var topic in topics)
            {
                var subscriptions = await topic.Subscriptions.ListAsync();
                _logger.Log(LogLevel.Information, $"SubscriptionService Subscriptions.ListAsync: {sw.ElapsedMilliseconds}");
                foreach (var subscription in subscriptions)
                {
                    gaugeModels.Add(GaugeHelper.Create("servicebus_subscription_active_messages", "The number of active messages in the subscription.", new[] { "name" }, new[] { subscription.Name }, subscription.ActiveMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_scheduled_messages", "The number of messages sent to the subscription that are yet to be released for consumption.", new[] { "name" }, new[] { subscription.Name }, subscription.ScheduledMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_dead_letter_messages", "The number of messages in the dead-letter subscription.", new[] { "name" }, new[] { subscription.Name }, subscription.DeadLetterMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_messages", "The number of messages transferred to another queue, topic, or subscription.", new[] { "name" }, new[] { subscription.Name }, subscription.TransferMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_dead_letter_messages", "The number of messages transferred into dead letters.", new[] { "name" }, new[] { subscription.Name }, subscription.TransferDeadLetterMessageCount));
                }
            }

            sw.Stop();
            _logger.Log(LogLevel.Information, $"SubscriptionService: {sw.ElapsedMilliseconds}");

            return gaugeModels;
        }
    }
}