using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
using OffalBot.Domain;

namespace OffalBot.DataAccess.PullRequests
{
    public class OpenProcessor : IPullRequestWebhookProcessor
    {
        private readonly CloudStorageAccount _cloudStorage;

        public OpenProcessor(CloudStorageAccount cloudStorage)
        {
            _cloudStorage = cloudStorage;
        }

        public Task Execute(JObject payload)
        {
            throw new System.NotImplementedException();
        }
    }
}