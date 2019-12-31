using Microsoft.Extensions.Configuration;

namespace AzureServicebusExporter.Configuration
{
    public static class DockerSecretExtensions
    {
        public static IConfigurationBuilder AddDockerSecrets(
            this IConfigurationBuilder configurationBuilder, string secretPath)
        {
            configurationBuilder.Add((IConfigurationSource) new DockerSecretsConfigurationSource()
            {
                SecretPath = secretPath
            });

            return configurationBuilder;
        }
    }
}
