using CoolerMaster.ImageAi.Shared.Models;

namespace CoolerMaster.ImageAi.Shared.Interfaces
{
    public interface IAwsBedrockClient
    {
        Task<string> TextToImage(string prompt, string base64Image, ImageParameter imgParam);
        Task<string> ImageVariation(string prompt, List<string> base64Images, ImageParameter imgParam);
    }
}
