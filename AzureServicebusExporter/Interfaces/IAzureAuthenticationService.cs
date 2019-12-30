using Microsoft.Azure.Management.ServiceBus.Fluent;

namespace AzureServicebusExporter.Interfaces
{
    public interface IAzureAuthenticationService
    {
        IServiceBusManager Authenticate(string clientId, string clientSecret, string tenantId, string subscriptionId);
    }
}