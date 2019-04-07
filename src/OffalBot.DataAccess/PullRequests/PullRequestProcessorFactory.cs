using Microsoft.WindowsAzure.Storage;
using OffalBot.Domain;
using OffalBot.Domain.PullRequests;

namespace OffalBot.DataAccess.PullRequests
{
    public class PullRequestProcessorFactory
    {
        private readonly CloudStorageAccount _cloudStorage;

        public PullRequestProcessorFactory(CloudStorageAccount cloudStorage)
        {
            _cloudStorage = cloudStorage;
        }

        public IPullRequestWebhookProcessor CreateForAction(string action)
        {
            switch ((action ?? "").ToLowerInvariant())
            {
                case "opened": return new OpenProcessor(
                    new PullRequestRepository(_cloudStorage.CreateCloudTableClient()));
            }

            return null;
        }
    }
}