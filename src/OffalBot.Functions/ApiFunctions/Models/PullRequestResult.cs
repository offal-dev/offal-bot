using System;
using System.Collections.Generic;

namespace OffalBot.Functions.ApiFunctions.Models
{
    public class PullRequestResult
    {
        public string Title { get; set; }
        public int Number { get; set; }
        public Uri Url { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset ClosedAt { get; set; }
        public GithubUser Author { get; set; }
        public PullRequestStatus Status { get; set; }
        public string RepositoryName { get; set; }
        public Uri RepositoryUrl { get; set; }
        public IEnumerable<GithubUser> Assignees { get; set; }
        public IEnumerable<DeploymentEnvironment> DeploymentEnvironments { get; set; }
        public IEnumerable<ReviewStatus> Reviews { get; set; }
        public IEnumerable<StatusCheckResult> StatusCheckResults { get; set; }
        public IEnumerable<Label> Labels { get; set; }
        public int NumberOfComments { get; set; }
    }
}