using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OffalBot.DataAccess;
using OffalBot.Domain;
using OffalBot.Functions.Configuration;
using OffalBot.Functions.Github;

namespace OffalBot.Functions.DataFunctions
{
    public class PullRequestReviews
    {
        private readonly GithubConfig _githubConfig;

        public PullRequestReviews(GithubConfig githubConfig)
        {
            _githubConfig = githubConfig;
        }

        [FunctionName("pull-request-reviews")]
        public async Task Run(
            [QueueTrigger("github-pullrequestreview")]JObject review,
            ILogger log)
        {
            var reviewRequest = new ReviewRequest
            {
                RepositoryId = review["repository"]["id"].Value<long>(),
                PullRequestComment = review["pull_request"]["body"].Value<string>(),
                ReviewState = review["review"]["state"].Value<string>().ToLowerInvariant(),
                PullRequestNumber = review["pull_request"]["number"].Value<int>(),
                InstallationId = review["installation"]["id"].Value<int>()
            };

            log.LogInformation($"Processing: {JsonConvert.SerializeObject(reviewRequest, Formatting.Indented)}");

            var githubClient = await new GitHubClientProvider().CreateForInstallation(
                _githubConfig.AppId,
                _githubConfig.AppKey,
                reviewRequest.InstallationId);

            var pullRequestLabeler = new PullRequestLabeler(
                githubClient,
                new LabelMaker(githubClient, log),
                new IssueLabelManager(githubClient, log),
                log);

            await pullRequestLabeler.Process(reviewRequest);
        }
    }
}
