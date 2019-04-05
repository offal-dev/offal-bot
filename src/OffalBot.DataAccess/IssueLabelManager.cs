using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using OffalBot.Domain;

namespace OffalBot.DataAccess
{
    public class IssueLabelManager : IIssueLabelManager
    {
        private readonly IGitHubClient _gitHubClient;
        private readonly ILogger _log;

        public IssueLabelManager(
            IGitHubClient gitHubClient,
            ILogger log)
        {
            _gitHubClient = gitHubClient;
            _log = log;
        }

        public async Task SetLabelOnIssue(
            long repositoryId,
            int issueNumber,
            string label)
        {
            var issue = await _gitHubClient.Issue.Get(repositoryId, issueNumber);
            if (LabelIsSet(issue, label))
            {
                _log.LogInformation("Nothing to do.");
                return;
            }

            var actualLabel = await FindActualLabelForRepository(repositoryId, label);

            _log.LogInformation($"Setting label {actualLabel.Name} on issue...");
            await _gitHubClient.Issue.Labels.AddToIssue(
                repositoryId,
                issueNumber,
                new[] { actualLabel.Name });
        }

        public async Task RemoveLabel(
            long repositoryId,
            int issueNumber,
            string label)
        {
            var issue = await _gitHubClient.Issue.Get(repositoryId, issueNumber);
            if (!LabelIsSet(issue, label))
            {
                _log.LogInformation("Nothing to do.");
                return;
            }

            var actualLabel = await FindActualLabelForRepository(repositoryId, label);

            _log.LogInformation($"Removing label {actualLabel.Name} on issue...");
            await _gitHubClient.Issue.Labels.RemoveFromIssue(
                repositoryId,
                issueNumber,
                actualLabel.Name);
        }

        private static bool LabelIsSet(Issue issue, string expectedLabel)
        {
            return issue.Labels.Any(x => x.Name.Equals(expectedLabel, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task<Label> FindActualLabelForRepository(long repositoryId, string expectedLabel)
        {
            var existingLabels =
                await _gitHubClient.Issue.Labels.GetAllForRepository(repositoryId);

            return existingLabels.First(x =>
                x.Name.Equals(expectedLabel, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}