using System;
using System.Collections.Generic;
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
        private readonly ITopicService _topicService;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(ITopicService topicService, ILogger<SubscriptionService> logger)
        {
            _topicService = topicService;
            _logger = logger;
        }

        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            var topics = await _topicService.GetTopicsAsync(serviceBusNamespace);
            List<GaugeModel> gaugeModels = new List<GaugeModel>();

            foreach (var topic in topics)
            {
                var subscriptions = await topic.Subscriptions.ListAsync();
                foreach (var subscription in subscriptions)
                {
                    _logger.LogTrace($"{DateTime.Now:o} - Create gaugemodel for subscription: {subscription.Name} in topic: {topic.Name}");
                    
                    gaugeModels.Add(GaugeHelper.Create("servicebus_subscription_active_messages", "The number of active messages in the subscription.", new[] { "name" }, new[] { subscription.Name }, subscription.ActiveMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_scheduled_messages", "The number of messages sent to the subscription that are yet to be released for consumption.", new[] { "name" }, new[] { subscription.Name }, subscription.ScheduledMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_dead_letter_messages", "The number of messages in the dead-letter subscription.", new[] { "name" }, new[] { subscription.Name }, subscription.DeadLetterMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_messages", "The number of messages transferred to another queue, topic, or subscription.", new[] { "name" }, new[] { subscription.Name }, subscription.TransferMessageCount));
                    gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_dead_letter_messages", "The number of messages transferred into dead letters.", new[] { "name" }, new[] { subscription.Name }, subscription.TransferDeadLetterMessageCount));
                }
            }

            return gaugeModels;
        }
    }
}