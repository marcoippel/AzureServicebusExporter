using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Prometheus;

namespace ServicebusExporter
{
    class Program
    {
        private static string _clientId = "b2be284a-e99e-4b29-b4f0-8855558e5334";
        private static string _clientSecret = "Welkom01";
        private static string _tenantId = "208cebd9-57bb-4455-9d1c-478abebe72b6";
        private static string _subscriptionId = "331e081a-586a-497a-befd-ee049a31d234";
        private static string _resourceGroupName = "Swarm";
        private static string _resourceName = "dockerswarm";

        static async Task Main(string[] args)
        {
            Metrics.SuppressDefaultMetrics();

            
            Gauge jobsInQueue = Metrics.CreateGauge("myapp_jobs_queued", "Number of jobs waiting for processing in the queue.", new []{"testa"});
            

            //var server = new MetricServer(hostname: "localhost", port: 1234);
            //server.Start();

            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud); 

            var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _subscriptionId);
            IServiceBusNamespace @namespace = serviceBusManager.Namespaces.GetByResourceGroup(_resourceGroupName, _resourceName);

            jobsInQueue.WithLabels(new[] {"testb"});

            var b = await @namespace.Queues.ListAsync();

            foreach (IQueue queue in b)
            {
                var messageCount = queue.MessageCount;
            }

            while (true)
            {
                jobsInQueue.Inc();
                Thread.Sleep(TimeSpan.FromSeconds(1));
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
        }
    }
}
