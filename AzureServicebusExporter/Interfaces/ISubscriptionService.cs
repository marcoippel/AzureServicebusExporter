using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServicebusExporter.Models;
using Microsoft.Azure.Management.ServiceBus.Fluent;

namespace AzureServicebusExporter.Interfaces
{
    public interface ISubscriptionService
    {
        Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace);
    }
}