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
    public static class PullRequestReviews
    {
        [FunctionName("PullRequestReviews")]
        public static async Task Run(
            [QueueTrigger("github-pullrequestreview")]JObject review,
            [FromConfig(Name = "github-app-id")]string githubAppId,
            [FromConfig(Name = "github-app-key")]string githubAppKey,
            ILogger log)
        {
            var reviewRequest = new ReviewRequest
            {
                RepositoryId = review["repository"]["id"].Value<int>(),
                PullRequestComment = review["pull_request"]["body"].Value<string>(),
                ReviewState = review["review"]["state"].Value<string>().ToLowerInvariant(),
                PullRequestNumber = review["pull_request"]["number"].Value<int>(),
                InstallationId = review["installation"]["id"].Value<int>()
            };

            log.LogInformation($"Processing: {JsonConvert.SerializeObject(reviewRequest, Formatting.Indented)}");

            var githubClient = await new GitHubClientProvider().CreateForInstallation(
                githubAppId,
                githubAppKey,
                reviewRequest.InstallationId);

            var pullRequestLabeler = new PullRequestLabeler(
                githubClient,
                new LabelMaker(githubClient, log),
                log);

            await pullRequestLabeler.Process(reviewRequest);
        }
    }
}
