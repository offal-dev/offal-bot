using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using OffalBot.DataAccess.PullRequests;

namespace OffalBot.Functions.DataFunctions
{
    public class PullRequests
    {
        private readonly CloudStorageAccount _cloudStorage;

        public PullRequests(
            CloudStorageAccount cloudStorage)
        {
            _cloudStorage = cloudStorage;
        }

        [FunctionName("pull-requests")]
        public async Task Run(
            [QueueTrigger("github-pullrequest")]JObject payload,
            ILogger log)
        {
            var actionType = payload["action"].Value<string>();
            var azureStorage = new AzureStorage(_cloudStorage);
            var action = new PullRequestActionFactory(azureStorage)
                .CreateFor(actionType);

            if (action == null)
            {
                log.LogInformation($"Unsupported action type {actionType} ");
                return;
            }

            log.LogInformation($"Executing action {actionType} ...");
            await action.Execute(payload);

            log.LogInformation($"Taking a copy of processed queue item...");
            var queue = await azureStorage.GetQueue("github-pullrequest-backup");
            await queue.AddMessageAsync(new CloudQueueMessage(payload.ToString()));
        }
    }
}