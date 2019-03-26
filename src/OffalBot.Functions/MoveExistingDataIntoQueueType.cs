using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;

namespace OffalBot.Functions
{
    public static class MoveExistingDataIntoQueueType
    {
        [FunctionName("QueueTrigger")]
        public static async Task QueueTrigger(
            [QueueTrigger("github-webhooks")] JObject webhookEntry,
            ExecutionContext context,
            ILogger log)
        {
            var eventType = webhookEntry["httpHeaders"]["X-GitHub-Event"];
            
            var queue = await new AzureStorage(context).GetQueue($"github-{eventType}");
            await queue.AddMessageAsync(new CloudQueueMessage(webhookEntry.ToString()));
        }
    }
}