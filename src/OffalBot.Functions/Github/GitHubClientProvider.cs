using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Octokit;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace OffalBot.Functions.Github
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

        public async Task<IGitHubClient> CreateForInstallation(
            string githubAppId,
            string githubAppKey,
            int installationId)
        {
            if (installationId < 1)
            {
                throw new ArgumentNullException(nameof(installationId));
            }

            var githubAppClient = Create(githubAppId, githubAppKey);
            var response = await githubAppClient.GitHubApps.CreateInstallationToken(installationId);

            return new GitHubClient(new ProductHeaderValue("Offalbot"))
            {
                Credentials = new Credentials(response.Token)
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

            var now = DateTimeOffset.UtcNow;
            var claims = new Dictionary<string, object>
            {
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(10).ToUnixTimeSeconds() },
                {"iss", githubAppId }
            };

            using (var rsa = new RSACryptoServiceProvider())
            {
                var rsaParams = ToRsaParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
                rsa.ImportParameters(rsaParams);

                return Jose.JWT.Encode(claims, rsa, Jose.JwsAlgorithm.RS256);
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