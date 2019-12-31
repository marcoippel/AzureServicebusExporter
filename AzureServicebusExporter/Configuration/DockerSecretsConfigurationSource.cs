using Microsoft.Extensions.Configuration;

namespace AzureServicebusExporter.Configuration
{
    public class DockerSecretsConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigDockerConfigurationProvider(this, SecretPath);
        }

        public string SecretPath { get; set; }
    }
}
