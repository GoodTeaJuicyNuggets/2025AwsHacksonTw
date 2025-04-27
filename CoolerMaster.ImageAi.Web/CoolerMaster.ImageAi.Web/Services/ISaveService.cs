using System.Threading.Tasks;
using CoolerMaster.ImageAi.Shared;

namespace CoolerMaster.ImageAi.Web.Services
{
    public interface ISaveService
    {
        Task SaveImage(ImageSource imageSource, string imageUrl, string imagePrompt = "");
    }
}
