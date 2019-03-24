using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OffalBot.Functions
{
    public static class InboundGithubWebhook
    {
        [FunctionName("inbound-github-webhook")]
        [return: Queue("github-webhooks")]
        public static async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github/webhooks")]HttpRequest req,
            ILogger log)
        {
            var payLoad = await new StreamReader(req.Body).ReadToEndAsync();
            var jsonObject = JObject.Parse(payLoad);

            var headers = req.Headers
                .Select(x => new { key = x.Key, value = x.Value.FirstOrDefault() })
                .ToDictionary(x => x.key, y => y.value);

            jsonObject["httpHeaders"] = JObject.FromObject(headers);

            return JsonConvert.SerializeObject(jsonObject);
        }
    }
}
