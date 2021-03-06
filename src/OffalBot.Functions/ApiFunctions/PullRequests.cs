using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OffalBot.DataAccess.PullRequests;
using OffalBot.Functions.ApiFunctions.Models;
using OffalBot.Functions.Auth;

namespace OffalBot.Functions.ApiFunctions
{
    public static class PullRequests
    {
        [FunctionName("pull-requests-api")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pull-requests")] HttpRequest req,
            CloudStorageAccount storageAccount,
            ILogger log)
        {
            var session = await req.GetAuthSession(storageAccount);
            if (session == null)
            {
                log.LogInformation("Unable to find session");
                return new UnauthorizedResult();
            }

            var organisations = session.Organisations.Select(x => x.Name).ToList();
            var pullRequestRepository = new PullRequestRepository(new AzureStorage(storageAccount));
            var pullRequests = await pullRequestRepository.GetForOrganisations(organisations);

            return new JsonResult(pullRequests, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
