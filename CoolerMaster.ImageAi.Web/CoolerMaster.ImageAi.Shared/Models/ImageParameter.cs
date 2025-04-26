namespace CoolerMaster.ImageAi.Shared.Models
{
    public class ImageParameter
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public Enum ImageQuality { get; set; }
        public float CfgScale { get; set; }
        public int Seed { get; set; }
        public string NegativeText { get; set; }
        public int NumberOfImages { get; set; }
    }
}
