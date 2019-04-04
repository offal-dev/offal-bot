using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using OffalBot.Domain;

namespace OffalBot.DataAccess
{
    public class PullRequestLabeler
    {
        private readonly IGitHubClient _githubClient;
        private readonly ILabelMaker _labelMaker;
        private readonly ILogger _log;

        public PullRequestLabeler(
            IGitHubClient githubClient,
            ILabelMaker labelMaker,
            ILogger log)
        {
            _githubClient = githubClient;
            _labelMaker = labelMaker;
            _log = log;
        }

        public async Task Process(ReviewRequest reviewRequest)
        {
            var contributors = await _githubClient.Repository.GetAllContributors(reviewRequest.RepositoryId);
        }
    }
}