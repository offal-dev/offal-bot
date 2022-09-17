using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OffalBot.Functions;
using OffalBot.Functions.AccessToken;
using OffalBot.Functions.Configuration;

[assembly: WebJobsStartup(typeof(Startup))]
namespace OffalBot.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var audience = Environment.GetEnvironmentVariable("oauth-audience");
            var issuer = Environment.GetEnvironmentVariable("oauth-issuer");

            builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>(s =>
                new AccessTokenProvider(audience, issuer));

            builder.Services.AddSingleton(_ =>
                new GithubConfig 
                { 
                    OauthClientId = Environment.GetEnvironmentVariable("github-oauth-client-id"),
                    OauthSecret = Environment.GetEnvironmentVariable("github-oauth-client-secret"),
                    AppId = Environment.GetEnvironmentVariable("github-app-id"),
                    AppKey = Environment.GetEnvironmentVariable("github-app-key")
                });

            builder.Services.AddSingleton(_ =>
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage")));
        }
    }
}