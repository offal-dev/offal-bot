using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OffalBot.Functions.Auth;

namespace OffalBot.Functions.ApiFunctions
{
    public static class MeApi
    {
        [FunctionName("me-api")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequest req,
            CloudStorageAccount storageAccount,
            ILogger log)
        {
            var session = await req.GetAuthSession(storageAccount);
            if (session == null)
            {
                log.LogInformation("Unable to find session");
                return new UnauthorizedResult();
            }

            return new JsonResult(session, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
