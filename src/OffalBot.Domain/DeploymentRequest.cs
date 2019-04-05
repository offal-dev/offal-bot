using System;

namespace OffalBot.Domain
{
    public class DeploymentRequest
    {
        public int InstallationId { get; set; }
        public long RepositoryId { get; set; }
        public string Environment { get; set; }
        public string CommitSha { get; set; }

        public string LabelFriendlyEnvironment()
        {
            if (string.IsNullOrEmpty(Environment?.Trim()))
            {
                throw new ArgumentNullException(nameof(Environment));
            }

            return Environment
                .Replace(" ", "-")
                .ToLowerInvariant()
                .Trim();
        }
    }
}