using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            return await new StreamReader(req.Body).ReadToEndAsync();
        }
    }
}
