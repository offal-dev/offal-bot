using System;
using System.Linq;
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

            _log.LogInformation($"For SHA '{reviewRequest.CommitSha}' Found Issue {pullRequest.Number}");

            await _issueLabelManager.SetLabelOnIssue(
                reviewRequest.RepositoryId,
                pullRequest.Number,
                reviewRequest.LabelFriendlyEnvironment());

            await RemoveCodeReviewLabels(reviewRequest, pullRequest);
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

        private async Task RemoveCodeReviewLabels(
            DeploymentRequest reviewRequest,
            Issue pullRequest)
        {
            await _issueLabelManager.RemoveLabel(
                reviewRequest.RepositoryId,
                pullRequest.Number,
                Labels.Approved.Name);

            await _issueLabelManager.RemoveLabel(
                reviewRequest.RepositoryId,
                pullRequest.Number,
                Labels.ChangesRequested.Name);
        }
    }
}