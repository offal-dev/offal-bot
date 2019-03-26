using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace OffalBot.Functions
{
    public class AzureStorage
    {
        private readonly string _storageConnectionString;

        public AzureStorage(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _storageConnectionString = config["AzureWebJobsStorage"];
        }

        public async Task<CloudQueue> GetQueue(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(SanitiseQueueName(queueName));

            await queue.CreateIfNotExistsAsync();

            return queue;
        }

        private static string SanitiseQueueName(string queueName)
        {
            var trimmer = new Regex("([^A-Za-z0-9\\-]+)");
            var sanitised = trimmer.Replace(queueName, string.Empty);

            return sanitised.ToLowerInvariant();
        }
    }
}