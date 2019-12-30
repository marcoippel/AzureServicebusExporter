using AzureServicebusExporter.Interfaces;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;

namespace AzureServicebusExporter.Services
{
    public class AzureAuthenticationService : IAzureAuthenticationService
    {
        public IServiceBusManager Authenticate(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {
            var azureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            return ServiceBusManager.Authenticate(azureCredentials, subscriptionId);
        }
    }
}
