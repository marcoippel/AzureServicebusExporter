using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Servicebus_exporter.Models;
using Servicebus_exporter.Services;

namespace Servicebus_exporter.Interfaces
{
    public interface IQueueService
    {
        Task<List<GaugeModel>> CreateMetricsAsync(IServiceBusNamespace serviceBusNamespace);
    }
}
