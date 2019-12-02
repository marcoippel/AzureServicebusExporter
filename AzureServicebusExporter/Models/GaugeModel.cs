namespace AzureServicebusExporter.Models
{
    public class GaugeModel
    {
        public GaugeModel()
        {
            Labels = new string[]{};
            LabelValues = new string[]{};
        }

        public string Name { get; set; }
        public string Help { get; set; }
        public string[] Labels { get; set; }
        public string[] LabelValues { get; set; }
        public long Value { get; set; }
    }
}