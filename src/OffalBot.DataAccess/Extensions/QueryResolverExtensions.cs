using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace OffalBot.DataAccess.Extensions
{
    public static class QueryResolverExtensions
    {
        public static EntityProperty Get(
            this IDictionary<string, EntityProperty> properties,
            string key)
        {
            if (!properties.ContainsKey(key))
            {
                return null;
            }

            return properties[key];
        }
    }
}