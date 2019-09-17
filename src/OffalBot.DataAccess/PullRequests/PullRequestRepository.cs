using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using OffalBot.DataAccess.Extensions;
using OffalBot.Domain.PullRequests;

namespace OffalBot.DataAccess.PullRequests
{
    public class PullRequestRepository : IPullRequestRepository
    {
        private const string PullRequestsTable = "pullrequests";
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

            var table = await _azureStorage.GetTable(PullRequestsTable);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        public async Task UpdateStatus(
            string organisation,
            int pullRequestId,
            PullRequestStatus status)
        {
            var table = await _azureStorage.GetTable(PullRequestsTable);

            var entity = new DynamicTableEntity(organisation.ToLowerInvariant(), pullRequestId.ToString())
            {
                ETag = "*",
                Properties = { { "Status", new EntityProperty(status.ToString()) } }
            };

            var mergeOperation = TableOperation.Merge(entity);
            await table.ExecuteAsync(mergeOperation);
        }

        public async Task<IEnumerable<PullRequest>> GetForOrganisations(List<string> organisations)
        {
            if (organisations == null)
            {
                throw new ArgumentNullException(nameof(organisations));
            }

            if (!organisations.Any())
            {
                throw new Exception("No organisations found");
            }

            var query = MatchOnOrganisation(organisations.First());

            for (var index = 1; index < organisations.Count; index++)
            {
                var organisation = organisations[index];
                query = TableQuery.CombineFilters(
                    query,
                    TableOperators.Or,
                    MatchOnOrganisation(organisation));
            }

            var table = await _azureStorage.GetTable(PullRequestsTable);
            var result = await _azureStorage.QueryTable(
                table,
                new TableQuery { FilterString = query },
                QueryResolver);

            return result.Select(MapToDomain);
        }

        private static PullRequest MapToDomain(PullRequestDao dao)
        {
            return new PullRequest
            {
                Id = Convert.ToInt32(dao.RowKey),
                Number = dao.Number ?? 0,
                Title = dao.Title,
                Url = string.IsNullOrEmpty(dao.Url)
                    ? null
                    : new Uri(dao.Url),
                Status = (PullRequestStatus)Enum.Parse(typeof(PullRequestStatus), dao.Status, true),
                RepositoryName = dao.RepositoryName,
                RepositoryUrl = string.IsNullOrEmpty(dao.RepositoryUrl)
                    ? null
                    : new Uri(dao.RepositoryUrl),
                CreatedAt = dao.CreatedAt ?? DateTimeOffset.MinValue,
                UpdatedAt = dao.UpdatedAt ?? DateTimeOffset.MinValue,
                ClosedAt = dao.ClosedAt
            };
        }

        private static PullRequestDao QueryResolver(
            string partitionKey,
            string rowKey,
            DateTimeOffset timestamp,
            IDictionary<string, EntityProperty> properties,
            string etag)
        {
            return new PullRequestDao
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Timestamp = timestamp,
                Title = properties.Get("Title")?.StringValue,
                Number = properties.Get("Number")?.Int32Value,
                Url = properties.Get("Url")?.StringValue,
                Status = properties.Get("Status")?.StringValue,
                CreatedAt = properties.Get("CreatedAt")?.DateTimeOffsetValue,
                UpdatedAt = properties.Get("UpdatedAt")?.DateTimeOffsetValue,
                ClosedAt = properties.Get("ClosedAt")?.DateTimeOffsetValue,
                RepositoryName = properties.Get("RepositoryName")?.StringValue,
                RepositoryUrl = properties.Get("RepositoryUrl")?.StringValue,
            };
        }

        private static string MatchOnOrganisation(string organisation)
        {
            return TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                organisation.ToLowerInvariant());
        }

        private class PullRequestDao : TableEntity
        {
            public string Title { get; set; }
            public int? Number { get; set; }
            public string Url { get; set; }
            public string Status { get; set; }
            public DateTimeOffset? CreatedAt { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
            public DateTimeOffset? ClosedAt { get; set; }
            public string RepositoryName { get; set; }
            public string RepositoryUrl { get; set; }
        }
    }
}