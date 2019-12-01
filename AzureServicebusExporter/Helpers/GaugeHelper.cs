using AzureServicebusExporter.Models;

namespace AzureServicebusExporter.Helpers
{
    public static class GaugeHelper
    {
        public static GaugeModel Create(string name, string help, string[] labels, string[] labelValues, long count)
        {
            return new GaugeModel
            {
                Name = name,
                Help = help,
                Labels = labels,
                LabelValues = labelValues,
                Value = count
            };
        }
    }
}
