using System;
using System.Linq;
using System.Security.Authentication;

namespace OffalBot.Functions.AccessToken
{
    public static class AccessTokenResultExtensions
    {
        public static string EmailAddress(this AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult.Status != AccessTokenStatus.Valid)
            {
                throw new AuthenticationException("Token invalid", accessTokenResult.Exception);
            }

            return accessTokenResult.Principal.Claims
                .First(x => x.Type.Equals("email", StringComparison.InvariantCultureIgnoreCase))
                .Value;
        }
    }
}