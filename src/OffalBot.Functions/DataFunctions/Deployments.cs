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
    public class Deployments
    {
        private readonly GithubConfig _githubConfig;

        public Deployments(
            GithubConfig githubOauthConfig)
        {
            _githubConfig = githubOauthConfig;
        }

        [FunctionName("deployments")]
        [return: Queue("deployments-backup")]
        public async Task<string> Run(
            [QueueTrigger("github-deployment")]JObject review,
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
                _githubConfig.AppId,
                _githubConfig.AppKey,
                reviewRequest.InstallationId);

            var pullRequestLabeler = new DeploymentLabeler(
                githubClient,
                new LabelMaker(githubClient, log),
                new IssueLabelManager(githubClient, log), 
                log);

            await pullRequestLabeler.Process(reviewRequest);

            log.LogInformation("Taking a copy of processed queue item...");
            return review.ToString();
        }
    }
}
