using System.Threading.Tasks;
using Bindings.Azure.WebJobs.Extensions.UsefulBindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OffalBot.DataAccess;
using OffalBot.Domain;
using OffalBot.Functions.Github;

namespace OffalBot.Functions
{
    public static class Deployments
    {
        [FunctionName("deployments")]
        public static async Task Run(
            [QueueTrigger("github-deployments")]JObject review,
            [FromConfig(Name = "github-app-id")]string githubAppId,
            [FromConfig(Name = "github-app-key")]string githubAppKey,
            ILogger log)
        {
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
        }
    }
}
