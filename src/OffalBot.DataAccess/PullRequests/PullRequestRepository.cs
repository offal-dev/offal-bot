using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using OffalBot.Domain.PullRequests;

namespace OffalBot.DataAccess.PullRequests
{
    public class PullRequestRepository : IPullRequestRepository
    {
        private readonly IAzureStorage _azureStorage;

        public PullRequestRepository(IAzureStorage azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public async Task Upsert(string organisation, PullRequest pullRequest)
        {
            var entity = new PullRequestDao
            {
                PartitionKey = organisation.ToLowerInvariant(),
                RowKey = pullRequest.Id.ToString(),
                Number = pullRequest.Number,
                Title = pullRequest.Title,
                Timestamp = DateTimeOffset.Now,
                Url = pullRequest.Url.ToString(),
                CreatedAt = pullRequest.CreatedAt,
                UpdatedAt = pullRequest.UpdatedAt,
                ClosedAt = pullRequest.ClosedAt,
                Status = pullRequest.Status.ToString(),
                RepositoryName = pullRequest.RepositoryName,
                RepositoryUrl = pullRequest.RepositoryUrl.ToString()
            };

            var table = await _azureStorage.GetTable("pullrequests");
            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        private class PullRequestDao : TableEntity
        {
            public string Title { get; set; }
            public int Number { get; set; }
            public string Url { get; set; }
            public string Status { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public DateTimeOffset? ClosedAt { get; set; }
            public string RepositoryName { get; set; }
            public string RepositoryUrl { get; set; }
        }
    }
}