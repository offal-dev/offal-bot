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
using OffalBot.Functions.Auth;

namespace OffalBot.Functions.ApiFunctions
{
    public class PullRequests
    {
        private readonly CloudStorageAccount _storageAccount;

        public PullRequests(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
        }

        [FunctionName("pull-requests-api")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pull-requests")] HttpRequest req,
            ILogger log)
        {
            var session = await req.GetAuthSession(_storageAccount);
            if (session == null)
            {
                log.LogInformation("Unable to find session");
                return new UnauthorizedResult();
            }

            var organisations = session.Organisations.Select(x => x.Name).ToList();
            var pullRequestRepository = new PullRequestRepository(new AzureStorage(_storageAccount));
            var pullRequests = await pullRequestRepository.GetForOrganisations(organisations);

            return new JsonResult(pullRequests, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
