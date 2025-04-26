using CoolerMaster.ImageAi.Shared.Enums;

namespace CoolerMaster.ImageAi.Web.Models
{
    public class ImageParameterViewModel
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public ImageQuality ImageQuality { get; set; }
        public float CfgScale { get; set; }
        public int Seed { get; set; }
        public string NegativeText { get; set; }
        public int NumberOfImages { get; set; }
    }
}
