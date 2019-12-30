using System;

namespace AzureServicebusExporter
{
    public static class GuardClauses
    {
        public static void IsNotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
