namespace WebApplication1.Models
{
    public class GaugeModel
    {
        public string Name { get; set; }
        public string Help { get; set; }
        public string[] Labels { get; set; }
        public string[] LabelValues { get; set; }
        public long Value { get; set; }
    }
}