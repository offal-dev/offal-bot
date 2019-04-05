using System;
using System.Collections.Generic;
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
        private readonly IIssueLabelManager _issueLabelManager;
        private readonly ILogger _log;
        private static readonly Dictionary<string, string> StateLabelMapping = new Dictionary<string, string>
        {
            {"approved", Labels.Approved.Name },
            {"changes_requested", Labels.ChangesRequested.Name }
        };

        public PullRequestLabeler(
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

        public async Task Process(ReviewRequest reviewRequest)
        {
            if (!StateLabelMapping.ContainsKey(reviewRequest.ReviewState))
            {
                _log.LogInformation($"Unsupported state {reviewRequest.ReviewState}");
                return;
            }

            await EnsureLabelExistInRepository(reviewRequest);

            var expectedLabel = StateLabelMapping[reviewRequest.ReviewState];

            await _issueLabelManager.SetLabelOnIssue(
                reviewRequest.RepositoryId,
                reviewRequest.PullRequestNumber,
                expectedLabel);

            await SetLabelOnConnectedIssue(reviewRequest, expectedLabel);
        }

        private async Task EnsureLabelExistInRepository(ReviewRequest reviewRequest)
        {
            await _labelMaker.CreateIfMissing(reviewRequest.RepositoryId, Labels.Approved.Name, Labels.Approved.Colour);
            await _labelMaker.CreateIfMissing(reviewRequest.RepositoryId, Labels.ChangesRequested.Name,
                Labels.ChangesRequested.Colour);
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

            await _issueLabelManager.SetLabelOnIssue(
                reviewRequest.RepositoryId,
                issueNumber,
                expectedLabel);
        }
    }
}