using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IQueueService
    {
        Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace);
    }
}
