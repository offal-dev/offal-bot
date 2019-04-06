using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace OffalBot.Functions.Functions
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

            var sessionId = req.Cookies["session"];

            if (string.IsNullOrEmpty(sessionId))
            {
                return new UnauthorizedResult();
            }

            var table = await new AzureStorage(storageAccount).GetTable("sessions");
            var tableOperation = TableOperation
                .Retrieve<SessionDao>(sessionId, sessionId);
            var tableResult = await table.ExecuteAsync(tableOperation);

            if (tableResult == null || tableResult.HttpStatusCode != (int)HttpStatusCode.OK)
            {
                return new UnauthorizedResult();
            }

            if (tableResult.Result.GetType() != typeof(SessionDao))
            {
                return new BadRequestResult();
            }

            var session = (SessionDao)tableResult.Result;
            if (session.Expiry < DateTimeOffset.Now)
            {
                return new UnauthorizedResult();
            }

            return new JsonResult(session, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }
}
