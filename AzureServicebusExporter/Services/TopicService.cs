using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AzureServicebusExporter.Helpers;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;

namespace AzureServicebusExporter.Services
{
    public class TopicService : ITopicService
    {
        private readonly ILogger<TopicService> _logger;

        public TopicService(ILogger<TopicService> logger)
        {
            _logger = logger;
        }

        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<GaugeModel> gaugeModels = new List<GaugeModel>();
            var topics = await GetTopicsAsync(serviceBusNamespace);

            foreach (var topic in topics)
            {
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_active_messages", "The number of active messages in the topic.", new[] { "name" }, new[] { topic.Name }, topic.ActiveMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_scheduled_messages", "The number of messages sent to the topic that are yet to be released for consumption.", new[] { "name" }, new[] { topic.Name }, topic.ScheduledMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_dead_letter_messages", "The number of messages in the dead-letter topic.", new[] { "name" }, new[] { topic.Name }, topic.DeadLetterMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_messages", "The number of messages transferred to another topic, topic, or subscription.", new[] { "name" }, new[] { topic.Name }, topic.TransferMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_dead_letter_messages", "The number of messages transferred into dead letters.", new[] { "name" }, new[] { topic.Name }, topic.TransferDeadLetterMessageCount));
                
            }

            sw.Stop();
            _logger.Log(LogLevel.Information, $"TopicService: {sw.ElapsedMilliseconds}");

            return gaugeModels;
        }

        public async Task<IPagedCollection<ITopic>> GetTopicsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            return await serviceBusNamespace.Topics.ListAsync();
        }
    }
}
