using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Prometheus;
using Servicebus_exporter.Interfaces;

namespace Servicebus_exporter.Controllers
{
    public class MetricsController : Controller
    {
        private readonly IQueueService _queueService;

        private static string _clientId = "b2be284a-e99e-4b29-b4f0-8855558e5334";
        private static string _clientSecret = "Welkom01";
        private static string _tenantId = "208cebd9-57bb-4455-9d1c-478abebe72b6";
        private static string _subscriptionId = "331e081a-586a-497a-befd-ee049a31d234";
        private static string _resourceGroupName = "Swarm";
        private static string _resourceName = "dockerswarm";

        private IServiceBusNamespace _namespace;

        public MetricsController(IQueueService queueService)
        {
            _queueService = queueService;
            Metrics.SuppressDefaultMetrics();

            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
            var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _subscriptionId);
            
            _namespace = serviceBusManager.Namespaces.GetByResourceGroup(_resourceGroupName, _resourceName);
        }

        public async Task<string> Index()
        {
            var queueMetrics = _queueService.CreateMetricsAsync(_namespace);

            

            //topics
            var topics = await _namespace.Topics.ListAsync();
            foreach (var topic in topics)
            {
                var subscription = topic.Subscriptions;
                var subscriptions = await subscription.ListAsync();
                foreach (var subscription1 in subscriptions)
                {
                    var messageCount = subscription1.MessageCount;
                }
            }


            return "Demo";
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