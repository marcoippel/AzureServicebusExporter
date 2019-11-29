using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Servicebus_exporter.Interfaces;
using Servicebus_exporter.Models;

namespace Servicebus_exporter.Services
{
    public class QueueService : IQueueService
    {
        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            List<GaugeModel> gaugeModels = new List<GaugeModel>();
            var queues = await GetQueuesAsync(serviceBusNamespace);
            foreach (var queue in queues)
            {
                gaugeModels.Add(CreateGauge("servicebus_queue_active_messages", "The number of messages in the queue.", new[] { "name" }, new[] { queue.Name }, queue.MessageCount));
                gaugeModels.Add(CreateGauge("servicebus_queue_dead_letter_messages", "The number of messages in the dead-letter queue.", new[] { "name" }, new[] { queue.Name }, queue.DeadLetterMessageCount));
                gaugeModels.Add(CreateGauge("servicebus_queue_size_bytes", "The current size of the queue, in bytes.", new[] { "name" }, new[] { queue.Name }, queue.CurrentSizeInBytes));
                gaugeModels.Add(CreateGauge("servicebus_queue_transfer_dead_letter_messages", "The number of messages transferred into dead letters.", new[] { "name" }, new[] { queue.Name }, queue.TransferDeadLetterMessageCount));
                gaugeModels.Add(CreateGauge("servicebus_queue_transfer_messages", "The number of messages transferred to another queue, topic, or subscription.", new[] { "name" }, new[] { queue.Name }, queue.TransferMessageCount));
            }

            return gaugeModels;
        }

        private static GaugeModel CreateGauge(string name, string help, string[] labels, string[] labelValues, long count)
        {
            return new GaugeModel
            {
                Name = name,
                Help = help,
                Labels = labels,
                LabelValues = labelValues,
                Value = count
            };
        }

        private async Task<IPagedCollection<IQueue>> GetQueuesAsync(IServiceBusNamespace serviceBusNamespace)
        {
            return await serviceBusNamespace.Queues.ListAsync();
        }
    }
}