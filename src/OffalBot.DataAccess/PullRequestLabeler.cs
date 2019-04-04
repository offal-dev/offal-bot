using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using OffalBot.Domain;

namespace OffalBot.DataAccess
{
    public class PullRequestLabeler
    {
        private readonly GitHubClient _githubClient;
        private readonly ILabelMaker _labelMaker;
        private readonly ILogger _log;

        public PullRequestLabeler(
            GitHubClient githubClient,
            ILabelMaker labelMaker,
            ILogger log)
        {
            _githubClient = githubClient;
            _labelMaker = labelMaker;
            _log = log;
        }

        public Task Process(ReviewRequest dao)
        {
            throw new System.NotImplementedException();
        }
    }
}