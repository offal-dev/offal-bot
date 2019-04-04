using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Octokit;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace OffalBot.Functions.GithubAuth
{
    public class GitHubClientProvider
    {
        public GitHubClient Create(
            string githubAppId,
            string githubAppKey)
        {
            if (string.IsNullOrEmpty(githubAppId))
            {
                throw new ArgumentNullException(nameof(githubAppId));
            }

            if (string.IsNullOrEmpty(githubAppKey))
            {
                throw new ArgumentNullException(nameof(githubAppKey));
            }

            return new GitHubClient(new ProductHeaderValue("OffalBot"))
            {
                Credentials = new Credentials(
                    CreateJwtToken(githubAppId, githubAppKey),
                    AuthenticationType.Bearer)
            };
        }

        private static string CreateJwtToken(
            string githubAppId,
            string githubAppKey)
        {
            var privateKeyBytes = Convert.FromBase64String(githubAppKey);

            AsymmetricCipherKeyPair keyPair;
            using (var memoryStream = new MemoryStream(privateKeyBytes))
            using (var streamReader = new StreamReader(memoryStream))
            {
                keyPair = (AsymmetricCipherKeyPair)new PemReader(streamReader).ReadObject();
            }

            var claims = new List<Claim>
            {
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString()),
                new Claim("iss", githubAppId)
            };

            using (var rsa = new RSACryptoServiceProvider())
            {
                var rsaParams = ToRsaParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
                rsa.ImportParameters(rsaParams);

                var payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);
                return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256);
            }
        }

        private static RSAParameters ToRsaParameters(RsaPrivateCrtKeyParameters privateKey)
        {
            return new RSAParameters
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned()
            };
        }
    }
}