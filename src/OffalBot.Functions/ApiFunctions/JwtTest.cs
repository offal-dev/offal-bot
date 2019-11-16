using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OffalBot.Functions.AccessToken;

namespace OffalBot.Functions.ApiFunctions
{
    public class JwtTest
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public JwtTest(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        [FunctionName("jwt-test")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jwt-test")] HttpRequest req,
            ILogger log)
        {
            var result = await _accessTokenProvider.ValidateToken(req);
            if (result.Status != AccessTokenStatus.Valid)
            {
                return new UnauthorizedResult();
            }

            return new JsonResult(new { email = result.EmailAddress() }, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
