namespace CoolerMaster.ImageAi.Shared.Interfaces
{
    public interface IAwsS3Client
    {
        Task<string> UploadImageAsync(Stream imageStream, string folderName, string fileName, string contentType);
    }
}
