using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AzureServicebusExporter.Configuration
{
    public class ConfigDockerConfigurationProvider : ConfigurationProvider
    {
        private readonly DockerSecretsConfigurationSource _dockerSecretsConfigurationSource;
        private readonly string _secretPath;

        public ConfigDockerConfigurationProvider(DockerSecretsConfigurationSource dockerSecretsConfigurationSource, string secretPath)
        {
            _dockerSecretsConfigurationSource = dockerSecretsConfigurationSource;
            _secretPath = secretPath;
        }

        public override void Load()
        {
            if (Directory.Exists(_secretPath))
            {
                var files = Directory.GetFiles(_secretPath);
                
                Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);
                    var content = File.ReadAllText(filePath);
                    Data[fileName] = content;
                }
            }
        }
    }
}