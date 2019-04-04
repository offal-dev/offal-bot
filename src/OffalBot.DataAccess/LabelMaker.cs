using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using OffalBot.Domain;

namespace OffalBot.DataAccess
{
    public class LabelMaker : ILabelMaker
    {
        private readonly IGitHubClient _githubClient;
        private readonly ILogger _log;

        public LabelMaker(IGitHubClient githubClient, ILogger log)
        {
            _githubClient = githubClient;
            _log = log;
        }

        public async Task CreateIfMissing(
            int repositoryId,
            string labelName,
            string labelColour)
        {
            var existingLabel = await _githubClient.Issue.Labels.GetAllForRepository(repositoryId);
            if (existingLabel.Any(x => x.Name.Equals(labelName)))
            {
                return;
            }

            _log.LogInformation($"Creating label {labelName}");
            await _githubClient.Issue.Labels.Create(repositoryId, new NewLabel(labelName, labelColour));
        }
    }
}