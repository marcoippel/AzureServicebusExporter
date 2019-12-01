using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServicebusExporter.Models;
using Microsoft.Azure.Management.ServiceBus.Fluent;

namespace AzureServicebusExporter.Interfaces
{
    public interface IQueueService
    {
        Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace);
    }
}
