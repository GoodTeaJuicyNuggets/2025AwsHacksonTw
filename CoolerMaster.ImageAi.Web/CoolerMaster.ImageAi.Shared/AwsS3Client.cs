using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CoolerMaster.ImageAi.Shared.Configurations;
using CoolerMaster.ImageAi.Shared.Interfaces;
using System.Net;

namespace CoolerMaster.ImageAi.Shared
{
    public class AwsS3Client : IAwsS3Client
    {
        private readonly AwsS3Config _awsS3Config;
        private readonly IAmazonS3 _s3Client;
        public AwsS3Client(AwsS3Config awsS3Config)
        {
            _awsS3Config = awsS3Config;

            var bucketRegion = RegionEndpoint.GetBySystemName(_awsS3Config.Region);
            _s3Client = new AmazonS3Client(_awsS3Config.AccessKeyId, _awsS3Config.SecretAccessKey, bucketRegion);
        }
        public async Task<bool> UploadImageAsync(Stream imageStream, string folderName, string fileName, string contentType)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _awsS3Config.BucketName,
                Key = $"{folderName}/{fileName}",
                InputStream = imageStream,
                ContentType = contentType
            };

            try
            {
                var response = await _s3Client.PutObjectAsync(putRequest);

                return (response.HttpStatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
