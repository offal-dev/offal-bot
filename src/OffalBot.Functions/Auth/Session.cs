using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OffalBot.Functions.Auth
{
    public class Session
    {
        public string Id { get; }
        public string Username { get; }
        public DateTimeOffset Expiry { get; }
        public IEnumerable<SessionDao.Organisation> Organisations { get; }

        public Session(SessionDao dao)
        {
            Id = dao.PartitionKey;
            Expiry = dao.Expiry;
            Username = dao.Username;
            Organisations = JsonConvert.DeserializeObject<List<SessionDao.Organisation>>(dao.Organisations);
        }

    }
}