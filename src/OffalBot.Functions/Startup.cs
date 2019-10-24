using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OffalBot.Functions;
using OffalBot.Functions.AccessToken;

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
        }
    }
}