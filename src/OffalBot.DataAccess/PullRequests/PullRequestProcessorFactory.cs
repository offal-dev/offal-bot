using Microsoft.WindowsAzure.Storage;
using OffalBot.Domain;
using OffalBot.Domain.PullRequests;

namespace OffalBot.DataAccess.PullRequests
{
    public class PullRequestProcessorFactory
    {
        private readonly IAzureStorage _azureStorage;

        public PullRequestProcessorFactory(IAzureStorage azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public IPullRequestWebhookProcessor CreateForAction(string action)
        {
            switch ((action ?? "").ToLowerInvariant())
            {
                case "opened": return new OpenProcessor(
                    new PullRequestRepository(_azureStorage));
            }

            return null;
        }
    }
}