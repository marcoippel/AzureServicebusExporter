using AzureServicebusExporter.Helpers;
using Xunit;

namespace AzureServicebusExporter.Tests.Helpers
{
    public class GaugeHelperTests
    {
        [Fact]
        public void Create_Should_Return_A_GaugeModel()
        {
            var name = "Name";
            var helpText = "HelpText";
            var labels = new[] {"name, env"};
            var labelValues = new[] {"orders", "prod"};
            var count = 12;

            var gaugeModel = GaugeHelper.Create(name, helpText, labels, labelValues, count);

            Assert.Equal(gaugeModel.Name, name);
            Assert.Equal(gaugeModel.Help, helpText);
            Assert.Equal(gaugeModel.Labels, labels);
            Assert.Equal(gaugeModel.LabelValues, labelValues);
            Assert.Equal(gaugeModel.Value, count);
        }
    }
}
