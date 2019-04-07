using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using OffalBot.DataAccess;

namespace OffalBot.Functions
{
    public class AzureStorage : IAzureStorage
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorage(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
        }

        public async Task<CloudQueue> GetQueue(string queueName)
        {
            var queueClient = _storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(SanitiseName(queueName));

            await queue.CreateIfNotExistsAsync();

            return queue;
        }

        public async Task<CloudTable> GetTable(string tableName)
        {
            var client = _storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference(SanitiseName(tableName));
            await table.CreateIfNotExistsAsync();

            return table;
        }

        public async Task<IEnumerable<T>> QueryTable<T>(
            CloudTable table,
            TableQuery query,
            EntityResolver<T> resolver)
        {
            TableContinuationToken token = null;

            var results = new List<T>();
            do
            {
                var seg = await table
                    .ExecuteQuerySegmentedAsync(query, resolver,token)
                    .ConfigureAwait(false);

                token = seg.ContinuationToken;

                results.AddRange(seg.Results);
            } while (token != null);

            return results;
        }

        private static string SanitiseName(string name)
        {
            var trimmer = new Regex("([^A-Za-z0-9\\-]+)");
            var sanitised = trimmer.Replace(name, string.Empty);

            return sanitised.ToLowerInvariant();
        }
    }
}