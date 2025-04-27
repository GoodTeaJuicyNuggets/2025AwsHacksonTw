using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using CoolerMaster.ImageAi.Shared.Configurations;
using CoolerMaster.ImageAi.Shared.Interfaces;

namespace CoolerMaster.ImageAi.Shared
{
    public class AwsBedrcokAgentClient : IAwsBedrcokAgentClient
    {
        private readonly AwsBedrockConfig _awsBedrockConfig;
        private readonly AmazonBedrockAgentRuntimeClient _client;

        public AwsBedrcokAgentClient(AwsBedrockConfig awsBedrockConfig)
        {
            _awsBedrockConfig = awsBedrockConfig;

            var credentials = new BasicAWSCredentials(_awsBedrockConfig.AccessKeyId, _awsBedrockConfig.SecretAccessKey);
            var bucketRegion = RegionEndpoint.GetBySystemName(_awsBedrockConfig.Region);
            _client = new AmazonBedrockAgentRuntimeClient(credentials, bucketRegion);
        }

        public async Task<string> InvokeAgentAsync(string inputText)
        {
            var agentClient = new AmazonBedrockAgentRuntimeClient(RegionEndpoint.USEast1); // 選你的 region

            var request = new InvokeAgentRequest
            {
                AgentId = "PAPZYVKH3J",
                AgentAliasId = "Production",
                SessionId = Guid.NewGuid().ToString(),
                InputText = inputText
            };

            var response = await agentClient.InvokeAgentAsync(request);

            // 拿到 output
            return $"Output: {response.Completion}";
        }
    }
}
