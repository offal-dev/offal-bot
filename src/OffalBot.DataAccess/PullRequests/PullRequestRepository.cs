using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OffalBot.Domain.PullRequests;

namespace OffalBot.DataAccess.PullRequests
{
    public class PullRequestRepository : IPullRequestRepository
    {
        private readonly CloudTableClient _tableClient;

        public PullRequestRepository(CloudTableClient tableClient)
        {
            _tableClient = tableClient;
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
                ClosedAt = pullRequest.ClosedAt
            };

            var table =_tableClient.GetTableReference("pullrequests");
            await table.CreateIfNotExistsAsync();
            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        private class PullRequestDao : TableEntity
        {
            public string Title { get; set; }
            public int Number { get; set; }
            public string Url { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public DateTimeOffset? ClosedAt { get; set; }
        }
    }
}