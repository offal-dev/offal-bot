using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace OffalBot.Functions.AccessToken
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private static IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private const string AUTH_HEADER_NAME = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        private readonly string _audience;
        private readonly string _issuer;

        public AccessTokenProvider(
            string audience,
            string issuer)
        {
            _audience = audience;
            _issuer = issuer;

            if (_configurationManager == null)
            {
                var documentRetriever = new HttpDocumentRetriever
                {
                    RequireHttps = issuer.StartsWith("https://")
                };

                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{issuer}.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    documentRetriever
                );
            }
        }

        public async Task<AccessTokenResult> ValidateToken(HttpRequest request)
        {
            try
            {
                if (request == null || !request.Headers.ContainsKey(AUTH_HEADER_NAME) ||
                    !request.Headers[AUTH_HEADER_NAME].ToString().StartsWith(BEARER_PREFIX))
                {
                    return AccessTokenResult.NoToken();
                }

                var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
                var token = request.Headers[AUTH_HEADER_NAME].ToString().Substring(BEARER_PREFIX.Length);

                var tokenParams = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidAudience = _audience,
                    ValidateAudience = true,
                    ValidIssuer = _issuer,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKeys = config.SigningKeys
                };

                var result = new JwtSecurityTokenHandler()
                    .ValidateToken(token, tokenParams, out _);

                using (var client = new HttpClient())
                {
                    var userInfo = await client.GetUserInfoAsync(new UserInfoRequest
                    {
                        Address = config.UserInfoEndpoint,
                        Token = token
                    });

                    if (userInfo.IsError)
                    {
                        throw userInfo.Exception;
                    }

                    var identity = new ClaimsIdentity(result.Identities.First());
                    identity.AddClaims(userInfo.Claims);

                    return AccessTokenResult.Success(new ClaimsPrincipal(identity));
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }
    }
}