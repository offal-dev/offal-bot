using OffalBot.DataAccess.PullRequests.Actions;
using OffalBot.Domain.PullRequests.Actions;

namespace OffalBot.DataAccess.PullRequests
{
    public class PullRequestActionFactory
    {
        private readonly IAzureStorage _azureStorage;

        public PullRequestActionFactory(IAzureStorage azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public IPullRequestAction CreateFor(string action)
        {
            switch ((action ?? "").ToLowerInvariant())
            {
                case "opened": return new OpenAction(
                    new PullRequestRepository(_azureStorage));
                case "closed": return new ClosedAction(
                    new PullRequestRepository(_azureStorage));
                case "reopened": return new ReOpenAction(
                    new PullRequestRepository(_azureStorage));
            }

            return null;
        }
    }
}