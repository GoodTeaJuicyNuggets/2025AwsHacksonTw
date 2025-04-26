using CoolerMaster.ImageAi.Shared.Configurations;
using CoolerMaster.ImageAi.Shared.Interfaces;
using Amazon.Runtime;
using Amazon.BedrockRuntime;
using Amazon;
using System.Text.Json;
using CoolerMaster.ImageAi.Shared.Models;
using Amazon.BedrockRuntime.Model;
using System.Text.Json.Nodes;

namespace CoolerMaster.ImageAi.Shared
{
    public class AwsBedrockClient : IAwsBedrockClient
    {
        private readonly AwsBedrockConfig _awsBedrockConfig;
        private readonly AmazonBedrockRuntimeClient _bedrockRuntimeClient;
        public AwsBedrockClient(AwsBedrockConfig awsBedrockConfig)
        {
            _awsBedrockConfig = awsBedrockConfig;

            var credentials = new BasicAWSCredentials(_awsBedrockConfig.AccessKeyId, _awsBedrockConfig.SecretAccessKey);
            var bucketRegion = RegionEndpoint.GetBySystemName(_awsBedrockConfig.Region);
            _bedrockRuntimeClient = new AmazonBedrockRuntimeClient(credentials, bucketRegion);
        }
        public async Task<string> TextToImage(string prompt, string base64Image, ImageParameter imgParam)
        {
            var nativeRequest = string.Empty;

            if (!string.IsNullOrEmpty(base64Image))
            {
                nativeRequest = JsonSerializer.Serialize(new
                {
                    taskType = "TEXT_IMAGE",
                    textToImageParams = new
                    {
                        conditionImage = base64Image,
                        text = prompt
                    },
                    imageGenerationConfig = new
                    {
                        seed = imgParam.Seed,
                        quality = imgParam.ImageQuality.ToString().ToLower(),
                        width = imgParam.ImageWidth,
                        height = imgParam.ImageHeight,
                        numberOfImages = imgParam.NumberOfImages
                    }
                });
            }
            else
            {
                nativeRequest = JsonSerializer.Serialize(new
                {
                    taskType = "TEXT_IMAGE",
                    textToImageParams = new
                    {
                        text = prompt
                    },
                    imageGenerationConfig = new
                    {
                        seed = imgParam.Seed,
                        quality = imgParam.ImageQuality.ToString().ToLower(),
                        width = imgParam.ImageWidth,
                        height = imgParam.ImageHeight,
                        numberOfImages = imgParam.NumberOfImages
                    }
                });
            }

            var request = new InvokeModelRequest()
            {
                ModelId = _awsBedrockConfig.ModelId,
                Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nativeRequest)),
                ContentType = "application/json"
            };

            try
            {
                var response = await _bedrockRuntimeClient.InvokeModelAsync(request);

                var modelResponse = await JsonNode.ParseAsync(response.Body);

                return modelResponse["images"]?[0].ToString() ?? "";
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        public async Task<string> ImageVariation(string prompt, List<string> base64Images, ImageParameter imgParam)
        {
            var nativeRequest = JsonSerializer.Serialize(new
            {
                taskType = "IMAGE_VARIATION",
                imageVariationParams = new
                {
                    images = base64Images,
                    similarityStrength = 0.5f,
                    text = prompt,
                    negativeText = imgParam?.NegativeText?? ""
                },
                imageGenerationConfig = new
                {
                    height = imgParam.ImageHeight,
                    width = imgParam.ImageWidth,
                    cfgScale = imgParam.CfgScale,
                    seed = imgParam.Seed,
                    numberOfImages = imgParam.NumberOfImages
                }
            });

            var request = new InvokeModelRequest()
            {
                ModelId = _awsBedrockConfig.ModelId,
                Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nativeRequest)),
                ContentType = "application/json"
            };

            try
            {
                var response = await _bedrockRuntimeClient.InvokeModelAsync(request);

                var modelResponse = await JsonNode.ParseAsync(response.Body);

                return modelResponse["images"]?[0].ToString() ?? "";
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
