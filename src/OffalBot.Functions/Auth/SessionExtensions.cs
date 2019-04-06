using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace OffalBot.Functions.Auth
{
    public static class SessionExtensions
    {
        public static async Task<Session> GetAuthSession(
            this HttpRequest req,
            CloudStorageAccount storageAccount)
        {
            var sessionId = req.Cookies["session"];

            if (string.IsNullOrEmpty(sessionId))
            {
                return null;
            }

            var table = await new AzureStorage(storageAccount).GetTable("sessions");
            var tableOperation = TableOperation
                .Retrieve<SessionDao>(sessionId, sessionId);
            var tableResult = await table.ExecuteAsync(tableOperation);

            if (tableResult == null || tableResult.HttpStatusCode != (int)HttpStatusCode.OK)
            {
                return null;
            }

            if (tableResult.Result.GetType() != typeof(SessionDao))
            {
                return null;
            }

            var sessionDao = (SessionDao)tableResult.Result;
            if (sessionDao.Expiry < DateTimeOffset.Now)
            {
                return null;
            }

            return new Session(sessionDao);
        }
    }
}