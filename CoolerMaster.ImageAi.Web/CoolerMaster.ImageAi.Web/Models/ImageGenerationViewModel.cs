namespace CoolerMaster.ImageAi.Web.Models
{
    public class ImageGenerationViewModel
    {
        public string ImageWidth { get; set; }
        public string ImageHeight { get; set; }
        public string ImageQuality { get; set; }
        public string CfgScale { get; set; }
        public string Seed { get; set; }
        public string NegativeText { get; set; }
        public string NumberOfImages { get; set; }
    }
}
