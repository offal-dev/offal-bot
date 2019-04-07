using System.Threading.Tasks;
using Bindings.Azure.WebJobs.Extensions.UsefulBindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
using OffalBot.DataAccess.PullRequests;

namespace OffalBot.Functions.DataFunctions
{
    public static class PullRequests
    {
        [FunctionName("pull-requests")]
        public static async Task Run(
            [QueueTrigger("github-pullrequest")]
            JObject payload,
            CloudStorageAccount cloudStorage,
            ILogger log)
        {
            var action = payload["action"].Value<string>();
            var processor = new PullRequestProcessorFactory(cloudStorage)
                .CreateForAction(action);

            if (processor == null)
            {
                log.LogInformation($"Unsupported action type {action}");
                return;
            }

            log.LogInformation($"Execution action {action} ...");
            await processor.Execute(payload);
        }
    }
}