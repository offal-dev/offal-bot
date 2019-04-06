using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace OffalBot.Functions.Auth
{
    public class SessionDao : TableEntity
    {
        public string Organisations { get; set; }
        public string Username { get; set; }
        public DateTimeOffset Expiry { get; set; }

        public class Organisation
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}