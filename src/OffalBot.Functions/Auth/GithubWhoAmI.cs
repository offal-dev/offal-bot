using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace OffalBot.Functions.Auth
{
    public static class GithubWhoAmI
    {
        [FunctionName("github-who-am-i")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "github/me")] HttpRequest req,
            CloudStorageAccount storageAccount,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var session = await req.GetAuthSession(storageAccount);
            if (session == null)
            {
                return new UnauthorizedResult();
            }

            return new JsonResult(session, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }
}
