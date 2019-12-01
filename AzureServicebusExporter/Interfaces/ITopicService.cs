using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServicebusExporter.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;

namespace AzureServicebusExporter.Interfaces
{
    public interface ITopicService
    {
        Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace);
        Task<IPagedCollection<ITopic>> GetTopicsAsync(IServiceBusNamespace serviceBusNamespace);
    }
}