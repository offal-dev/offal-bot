using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Bindings.Azure.WebJobs.Extensions.UsefulBindings;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using OffalBot.Functions.Github;

namespace OffalBot.Functions.Functions
{
    public static class GithubLogin
    {
        [FunctionName("github-login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "github/login")] HttpRequest req,
            [FromConfig(Name = "github-oauth-client-id")]string clientId,
            [FromConfig(Name = "github-oauth-client-secret")]string clientSecret,
            CloudStorageAccount storageAccount,
            ILogger log)
        {
            var code = req.Query["code"];
            log.LogInformation($"Found code {code}");
            log.LogInformation("Exchanging code for access code...");

            var accessToken = await GetAccessToken(
                clientId,
                clientSecret,
                code);

            if (string.IsNullOrEmpty(accessToken))
            {
                return new BadRequestResult();
            }

            var githubClient = new GitHubClientProvider()
                .CreateForAccessToken(accessToken);

            log.LogInformation("Getting account details...");
            var user = await githubClient.User.Current();
            var organisations = await githubClient.Organization.GetAllForCurrent();
            if (!organisations.Any())
            {
                return new BadRequestResult();
            }

            var sessionId = await StoreSessionInfo(
                storageAccount,
                organisations,
                user,
                log);

            req.HttpContext.Response.Cookies.Append(
                "session",
                sessionId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.Now.AddHours(8)
                });

            return new RedirectResult("https://www.offal.dev");
        }

        private static async Task<string> GetAccessToken(
            string clientId,
            string clientSecret,
            StringValues code)
        {
            var accessTokenResponse = await (await "https://github.com/login/oauth/access_token"
                    .SetQueryParam("client_id", clientId)
                    .SetQueryParam("client_secret", clientSecret)
                    .SetQueryParam("code", code)
                    .WithHeader("accept", "application/json")
                    .PostAsync(new StringContent("")))
                .Content.ReadAsAsync<JObject>();

            return accessTokenResponse["access_token"].Value<string>();
        }
        private static async Task<string> StoreSessionInfo(
            CloudStorageAccount storageAccount,
            IEnumerable<Organization> organisations,
            Account user,
            ILogger log)
        {
            var session = CreateSessionObject(organisations, user);

            log.LogInformation("Creating session...");
            var table = await new AzureStorage(storageAccount).GetTable("sessions");
            await table.ExecuteAsync(TableOperation.Insert(session));

            return session.PartitionKey;
        }

        private static SessionDao CreateSessionObject(
            IEnumerable<Organization> orgs,
            Account user)
        {
            var sessionId = Guid.NewGuid().ToString();
            var organisations = orgs.Select(x => new SessionDao.Organisation { Id = x.Id, Name = x.Login });
            var organisationsSerialised = JsonConvert.SerializeObject(organisations);
            var session = new SessionDao
            {
                PartitionKey = sessionId,
                RowKey = sessionId,
                Timestamp = DateTimeOffset.Now,
                Organisations = organisationsSerialised,
                Username = user.Login,
                Expiry = DateTimeOffset.Now.AddHours(8)
            };
            return session;
        }

        private static string CreateCookie(
            string name,
            string value)
        {
            return new StringBuilder()
                .Append(HttpUtility.UrlEncode(name) + "=" + HttpUtility.UrlEncode(value))
                .Append("; HttpOnly")
                .Append("; Secure")
                .ToString();
        }

        public class SessionDao : TableEntity
        {
            public string Organisations { get; set; }
            public string Username { get; set; }
            public DateTimeOffset Expiry { get; set; }

            public class Organisation
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }
    }
}
