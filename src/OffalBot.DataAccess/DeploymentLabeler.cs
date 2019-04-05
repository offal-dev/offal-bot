using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using OffalBot.Domain;

namespace OffalBot.DataAccess
{
    public class DeploymentLabeler
    {
        private readonly IGitHubClient _githubClient;
        private readonly ILabelMaker _labelMaker;
        private readonly IIssueLabelManager _issueLabelManager;
        private readonly ILogger _log;

        public DeploymentLabeler(
            IGitHubClient githubClient,
            ILabelMaker labelMaker,
            IIssueLabelManager issueLabelManager,
            ILogger log)
        {
            _githubClient = githubClient;
            _labelMaker = labelMaker;
            _issueLabelManager = issueLabelManager;
            _log = log;
        }

        public async Task Process(DeploymentRequest reviewRequest)
        {
            await _labelMaker.CreateIfMissing(
                reviewRequest.RepositoryId,
                reviewRequest.LabelFriendlyEnvironment(),
                "EDEDED");

            var pullRequest = await FindPullRequestForCommit(
                reviewRequest.CommitSha,
                reviewRequest.RepositoryId);

            var prRepository = await FindIssueRepository(pullRequest);
            
            _log.LogInformation($"For SHA '{reviewRequest.CommitSha}' Found Issue {pullRequest.Number} in {prRepository.Name}");

            await _issueLabelManager.SetLabelOnIssue(
                prRepository.Id,
                pullRequest.Number,
                reviewRequest.LabelFriendlyEnvironment());

            await _issueLabelManager.RemoveLabel(
                prRepository.Id,
                pullRequest.Number,
                Labels.Approved.Name);

            await _issueLabelManager.RemoveLabel(
                prRepository.Id,
                pullRequest.Number,
                Labels.ChangesRequested.Name);
        }

        private async Task<Repository> FindIssueRepository(Issue pullRequest)
        {
            var regex = new Regex("api\\.github\\.com\\/repos\\/(.*?)\\/(.*?)\\/issues");
            var match = regex.Match(pullRequest.Url);
            if (!match.Success)
            {
                throw new Exception($"Unable to detect repo from URL {pullRequest.Url}");
            }

            return await _githubClient.Repository.Get(
                match.Groups[1].Value,
                match.Groups[2].Value);
        }

        private async Task<Issue> FindPullRequestForCommit(
            string reviewRequestCommitSha,
            long repositoryId)
        {
            var repoInfo = await _githubClient.Repository.Get(repositoryId);

            var searchResult = await _githubClient.Search.SearchIssues(new SearchIssuesRequest(reviewRequestCommitSha)
            {
                Repos = new RepositoryCollection {
                    { repoInfo.Owner.Login, repoInfo.Name }
                }
            });

            if (searchResult.TotalCount < 1)
            {
                throw new Exception($"Unable to find pull request for SHA {reviewRequestCommitSha}");
            }

            return searchResult.Items.First();
        }
    }
}