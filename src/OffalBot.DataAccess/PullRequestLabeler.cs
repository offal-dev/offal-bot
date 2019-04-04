using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Dictionary<string, string> StateLabelMapping = new Dictionary<string, string>
        {
            {"approved", Labels.Approved.Name },
            {"rejected", Labels.RequestedChanges.Name }
        };

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
            if (!StateLabelMapping.ContainsKey(reviewRequest.ReviewState))
            {
                _log.LogInformation($"Unsupported state {reviewRequest.ReviewState}");
                return;
            }

            await EnsureLabelExistInRepository(reviewRequest);

            var expectedLabel = StateLabelMapping[reviewRequest.ReviewState];
            var issue = await _githubClient.Issue.Get(reviewRequest.RepositoryId, reviewRequest.PullRequestNumber);
            if (LabelIsAlreadySet(issue, expectedLabel))
            {
                _log.LogInformation("Nothing to do.");
                return;
            }

            await SetLabelOnPullRequest(reviewRequest, issue, expectedLabel);
            //await SetLabelOnConnectedIssue(reviewRequest, expectedLabel);
        }

        private async Task EnsureLabelExistInRepository(ReviewRequest reviewRequest)
        {
            await _labelMaker.CreateIfMissing(reviewRequest.RepositoryId, Labels.Approved.Name, Labels.Approved.Colour);
            await _labelMaker.CreateIfMissing(reviewRequest.RepositoryId, Labels.RequestedChanges.Name,
                Labels.RequestedChanges.Colour);
        }

        private static bool LabelIsAlreadySet(Issue issue, string expectedLabel)
        {
            return issue.Labels.Any(x => x.Name.Equals(expectedLabel, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task SetLabelOnPullRequest(
            ReviewRequest reviewRequest,
            Issue issue,
            string expectedLabel)
        {
            var actualLabel = await FindActualLabelForRepository(reviewRequest, expectedLabel);

            _log.LogInformation($"Setting label {actualLabel.Name} on PR...");
            await _githubClient.Issue.Labels.AddToIssue(
                reviewRequest.RepositoryId,
                reviewRequest.PullRequestNumber,
                new[] { actualLabel.Name });
        }

        private async Task<Label> FindActualLabelForRepository(ReviewRequest reviewRequest, string expectedLabel)
        {
            var existingLabels =
                await _githubClient.Issue.Labels.GetAllForRepository(reviewRequest.RepositoryId);

            return existingLabels.First(x =>
                x.Name.Equals(expectedLabel, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task SetLabelOnConnectedIssue(
            ReviewRequest reviewRequest,
            string expectedLabel)
        {
            throw new NotImplementedException();
        }
    }
}