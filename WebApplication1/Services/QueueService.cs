using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using WebApplication1.Helpers;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class QueueService : IQueueService
    {
        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            List<GaugeModel> gaugeModels = new List<GaugeModel>();
            var queues = await GetQueuesAsync(serviceBusNamespace);
            foreach (var queue in queues)
            {
                gaugeModels.Add(GaugeHelper.Create("servicebus_queue_active_messages", "The number of messages in the queue.", new[] { "name" }, new[] { queue.Name }, queue.MessageCount));
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


//servicebus_queue_active_messages{name="somequeue"} 0
//servicebus_queue_dead_letter_messages{name="somequeue"} 0
//servicebus_queue_max_size_bytes{name="somequeue"} 1.073741824e+09
//servicebus_queue_scheduled_messages{name="somequeue"} 0
//servicebus_queue_size_bytes{name="somequeue"} 0
//servicebus_queue_transfer_dead_letter_messages{name="somequeue"} 0
//servicebus_queue_transfer_messages{name="somequeue"} 0

//servicebus_subscription_active_messages{name="somesubscription",topic_name="sometopic"} 0
//servicebus_subscription_dead_letter_messages{name="somesubscription",topic_name="sometopic"} 0
//servicebus_subscription_scheduled_messages{name="somesubscription",topic_name="sometopic"} 0
//servicebus_subscription_transfer_dead_letter_messages{name="somesubscription",topic_name="sometopic"} 0
//servicebus_subscription_transfer_messages{name="somesubscription",topic_name="sometopic"} 0

//servicebus_topic_active_messages{name="sometopic"} 0
//servicebus_topic_dead_letter_messages{name="sometopic"} 0
//servicebus_topic_max_size_bytes{name="sometopic"} 1.073741824e+09
//servicebus_topic_scheduled_messages{name="sometopic"} 0
//servicebus_topic_size_bytes{name="sometopic"} 0
//servicebus_topic_transfer_dead_letter_messages{name="sometopic"} 0
//servicebus_topic_transfer_messages{name="sometopic"} 0

//servicebus_up 1