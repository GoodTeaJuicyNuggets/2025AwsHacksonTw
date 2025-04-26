namespace CoolerMaster.ImageAi.Web.Interfaces
{
    public interface IAwsS3Client
    {
        Task<bool> UploadImageAsync(Stream imageStream, string folderName, string fileName, string contentType);
    }
}
