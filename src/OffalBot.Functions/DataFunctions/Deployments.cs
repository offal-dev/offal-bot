using System.Threading.Tasks;
using Bindings.Azure.WebJobs.Extensions.UsefulBindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OffalBot.DataAccess;
using OffalBot.Domain;
using OffalBot.Functions.Github;

namespace OffalBot.Functions.DataFunctions
{
    public static class Deployments
    {
        [FunctionName("deployments")]
        public static async Task Run(
            [QueueTrigger("github-deployment")]JObject review,
            [FromConfig(Name = "github-app-id")]string githubAppId,
            [FromConfig(Name = "github-app-key")]string githubAppKey,
            CloudStorageAccount cloudStorage,
            ILogger log)
        {
            var azureStorage = new AzureStorage(cloudStorage);
            var reviewRequest = new DeploymentRequest
            {
                InstallationId = review["installation"]["id"].Value<int>(),
                RepositoryId = review["repository"]["id"].Value<long>(),
                Environment = review["deployment"]["environment"].Value<string>(),
                CommitSha = review["deployment"]["sha"].Value<string>()
            };

            log.LogInformation($"Processing: {JsonConvert.SerializeObject(reviewRequest, Formatting.Indented)}");

            var githubClient = await new GitHubClientProvider().CreateForInstallation(
                githubAppId,
                githubAppKey,
                reviewRequest.InstallationId);

            var pullRequestLabeler = new DeploymentLabeler(
                githubClient,
                new LabelMaker(githubClient, log),
                new IssueLabelManager(githubClient, log), 
                log);

            await pullRequestLabeler.Process(reviewRequest);

            log.LogInformation($"Taking a copy of processed queue item...");
            var queue = await azureStorage.GetQueue("deployments-backup");
            await queue.AddMessageAsync(new CloudQueueMessage(review.ToString()));
        }
    }
}
