using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OffalBot.Functions.DataFunctions
{
    public class InboundGithubWebhook
    {
        private readonly CloudStorageAccount _cloudStorageAccount;

        public InboundGithubWebhook(CloudStorageAccount cloudStorageAccount)
        {
            _cloudStorageAccount = cloudStorageAccount;
        }

        [FunctionName("inbound-github-webhook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github/webhooks")]HttpRequest req,
            ILogger log)
        {
            var payLoad = await new StreamReader(req.Body).ReadToEndAsync();
            var jsonObject = JObject.Parse(payLoad);

            var headers = req.Headers
                .Select(x => new { key = x.Key, value = x.Value.FirstOrDefault() })
                .ToDictionary(x => x.key, y => y.value);

            jsonObject["httpHeaders"] = JObject.FromObject(headers);

            var eventType = (headers["X-GitHub-Event"] ?? "").Trim();
            if (string.IsNullOrEmpty(eventType))
            {
                log.LogError("Unable to find event type header value (X-GitHub-Event)");
                return new BadRequestResult();
            }

            log.LogInformation($"Processing event type {eventType}");
            var queue = await new AzureStorage(_cloudStorageAccount).GetQueue($"github-{eventType}");
            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(jsonObject)));

            return new OkResult();
        }
    }
}
