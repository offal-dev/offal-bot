using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace OffalBot.DataAccess
{
    public interface IAzureStorage
    {
        Task<CloudQueue> GetQueue(string queueName);
        Task<CloudTable> GetTable(string tableName);
        Task<IEnumerable<T>> QueryTable<T>(
            CloudTable table,
            TableQuery query,
            EntityResolver<T> resolver);
    }
}