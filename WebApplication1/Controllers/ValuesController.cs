using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Prometheus;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private static string _clientId = "b2be284a-e99e-4b29-b4f0-8855558e5334";
        private static string _clientSecret = "Welkom01";
        private static string _tenantId = "208cebd9-57bb-4455-9d1c-478abebe72b6";
        private static string _subscriptionId = "331e081a-586a-497a-befd-ee049a31d234";
        private static string _resourceGroupName = "Swarm";
        private static string _resourceName = "dockerswarm";

        private IServiceBusNamespace _namespace;

        public ValuesController(IQueueService queueService)
        {
            _queueService = queueService;
            Metrics.SuppressDefaultMetrics();

            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
            var serviceBusManager = ServiceBusManager.Authenticate(azureCredentials, _subscriptionId);

            _namespace = serviceBusManager.Namespaces.GetByResourceGroup(_resourceGroupName, _resourceName);
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            Gauge jobsInQueue = Metrics.CreateGauge("myapp_jobs_queued", "Number of jobs waiting for processing in the queue.", new[] { "testa" });

            //var queueMetrics = await _queueService.CreateMetricsAsync(_namespace);

            //foreach (var queueMetric in queueMetrics)
            //{
            //    CreateGaugeMetric(queueMetric);
            //}


            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var queueMetrics = await _queueService.CreateMetricsAsync(_namespace);

            foreach (var queueMetric in queueMetrics)
            {
                CreateGaugeMetric(queueMetric);
            }

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private void CreateGaugeMetric(GaugeModel queueMetric)
        {
            var gauge = Metrics.CreateGauge(queueMetric.Name, queueMetric.Help, queueMetric.Labels);
            gauge.Labels(queueMetric.LabelValues);
            gauge.Inc();
        }
    }
}
