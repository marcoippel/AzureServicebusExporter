using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServicebusExporter.Helpers;
using AzureServicebusExporter.Interfaces;
using AzureServicebusExporter.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;

namespace AzureServicebusExporter.Services
{
    public class QueueService : IQueueService
    {
        private readonly ILogger<QueueService> _logger;

        public QueueService(ILogger<QueueService> logger)
        {
            _logger = logger;
        }
        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            List<GaugeModel> gaugeModels = new List<GaugeModel>();
            var queues = await GetQueuesAsync(serviceBusNamespace);
            foreach (var queue in queues)
            {
                _logger.LogTrace($"{DateTime.Now:o} - Create gaugemodel for queue: {queue.Name}");

                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_active_messages", "The number of messages in the queue.", new[] { "name" }, new[] { queue.Name }, queue.MessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_scheduled_messages", "The number of messages sent to the queue that are yet to be released for consumption.", new[] { "name" }, new[] { queue.Name }, queue.ScheduledMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_dead_letter_messages", "The number of messages in the dead-letter queue.", new[] { "name" }, new[] { queue.Name }, queue.DeadLetterMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_size_bytes", "The current size of the queue, in bytes.", new[] { "name" }, new[] { queue.Name }, queue.CurrentSizeInBytes));
                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_transfer_dead_letter_messages", "The number of messages transferred into dead letters.", new[] { "name" }, new[] { queue.Name }, queue.TransferDeadLetterMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_transfer_messages", "The number of messages transferred to another queue, topic, or subscription.", new[] { "name" }, new[] { queue.Name }, queue.TransferMessageCount));
            }

            return gaugeModels;
        }

        private async Task<IPagedCollection<IQueue>> GetQueuesAsync(IServiceBusNamespace serviceBusNamespace)
        {
            return await serviceBusNamespace.Queues.ListAsync();
        }
    }
}