using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using WebApplication1.Helpers;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class TopicService : ITopicService
    {
        public async Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            List<GaugeModel> gaugeModels = new List<GaugeModel>();
            var topics = await GetTopicsAsync(serviceBusNamespace);

            foreach (var topic in topics)
            {
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_active_messages", "The number of active messages in the topic.", new[] { "name" }, new[] { topic.Name }, topic.ActiveMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_dead_letter_messages", "The number of messages in the dead-letter topic.", new[] { "name" }, new[] { topic.Name }, topic.DeadLetterMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_dead_letter_messages", "The number of messages transferred into dead letters.", new[] { "name" }, new[] { topic.Name }, topic.TransferDeadLetterMessageCount));
                gaugeModels.Add(GaugeHelper.Create("servicebus_topic_transfer_messages", "The number of messages transferred to another topic, topic, or subscription.", new[] { "name" }, new[] { topic.Name }, topic.TransferMessageCount));
            }

            return gaugeModels;
        }

        private async Task<IPagedCollection<ITopic>> GetTopicsAsync(IServiceBusNamespace serviceBusNamespace)
        {
            return await serviceBusNamespace.Topics.ListAsync();
        }
    }
}
