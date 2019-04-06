using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace OffalBot.Functions
{
    public class AzureStorage
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

        private static string SanitiseName(string name)
        {
            var trimmer = new Regex("([^A-Za-z0-9\\-]+)");
            var sanitised = trimmer.Replace(name, string.Empty);

            return sanitised.ToLowerInvariant();
        }
    }
}