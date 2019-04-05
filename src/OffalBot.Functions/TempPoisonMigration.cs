using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OffalBot.Functions
{
    public static class TempPoisonMigration
    {
        [FunctionName("temp-deployment-poison-migration")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "deployment-poison-migration")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            var poison = await new AzureStorage(context).GetQueue("github-deployment-poison");
            var messages = await poison.GetMessagesAsync(10);

            var deployment = await new AzureStorage(context).GetQueue("github-deployment");

            foreach (var message in messages)
            {
                log.LogInformation("Migrating message...");
                await deployment.AddMessageAsync(message);
                await poison.DeleteMessageAsync(message);
            }

            return new OkResult();
        }
    }
}
