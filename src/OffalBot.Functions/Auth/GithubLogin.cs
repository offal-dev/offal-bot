using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
using OffalBot.Functions.Configuration;
using OffalBot.Functions.Github;

namespace OffalBot.Functions.Auth
{
    public class GithubLogin
    {
        private readonly GithubConfig _githubConfig;
        private readonly CloudStorageAccount _storageAccount;

        public GithubLogin(
            GithubConfig githubConfig,
            CloudStorageAccount storageAccount)
        {
            _githubConfig = githubConfig;
            _storageAccount = storageAccount;
        }

        [FunctionName("github-login")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "github/login")] HttpRequest req,
            ILogger log)
        {
            var code = req.Query["code"];
            log.LogInformation($"Found code {code}");
            log.LogInformation("Exchanging code for access code...");

            var accessToken = await GetAccessToken(
                _githubConfig.OauthClientId,
                _githubConfig.OauthSecret,
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
                _storageAccount,
                organisations,
                user,
                log);

            req.HttpContext.Response.Cookies.Append(
                "session",
                sessionId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !req.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase),
                    Expires = DateTimeOffset.Now.AddHours(8),
                    SameSite = SameSiteMode.None
                });

            return new RedirectResult("https://www.offal.dev/logged-in");
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
    }
}
