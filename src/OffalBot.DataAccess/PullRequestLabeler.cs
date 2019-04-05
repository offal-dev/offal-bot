using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

            await SetLabelOnIssue(reviewRequest.RepositoryId, reviewRequest.PullRequestNumber, expectedLabel);
            await SetLabelOnConnectedIssue(reviewRequest, expectedLabel);
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

        private async Task SetLabelOnIssue(
            int repositoryId,
            int issueNumber,
            string expectedLabel)
        {
            var issue = await _githubClient.Issue.Get(repositoryId, issueNumber);
            if (LabelIsAlreadySet(issue, expectedLabel))
            {
                _log.LogInformation("Nothing to do.");
                return;
            }

            var actualLabel = await FindActualLabelForRepository(repositoryId, expectedLabel);

            _log.LogInformation($"Setting label {actualLabel.Name} on issue...");
            await _githubClient.Issue.Labels.AddToIssue(
                repositoryId,
                issueNumber,
                new[] { actualLabel.Name });
        }

        private async Task<Label> FindActualLabelForRepository(int repositoryId, string expectedLabel)
        {
            var existingLabels =
                await _githubClient.Issue.Labels.GetAllForRepository(repositoryId);

            return existingLabels.First(x =>
                x.Name.Equals(expectedLabel, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task SetLabelOnConnectedIssue(
            ReviewRequest reviewRequest,
            string expectedLabel)
        {
            var regex = new Regex("Connects #([0-9]{0,8})");
            var match = regex.Match(reviewRequest.PullRequestComment);

            if (!match.Success)
            {
                return;
            }

            var issueNumber = Convert.ToInt32(match.Groups[1].Value);

            try
            {
                await _githubClient.Issue.Get(reviewRequest.RepositoryId, issueNumber);
            }
            catch
            {
                _log.LogWarning($"Unable to find issue for number {issueNumber}");
                return;
            }

            await SetLabelOnIssue(reviewRequest.RepositoryId, issueNumber, expectedLabel);
        }
    }
}