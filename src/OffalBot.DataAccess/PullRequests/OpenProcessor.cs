using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OffalBot.Domain.PullRequests;

namespace OffalBot.DataAccess.PullRequests
{
    public class OpenProcessor : IPullRequestWebhookProcessor
    {
        private readonly IPullRequestRepository _pullRequestRepository;

        public OpenProcessor(IPullRequestRepository pullRequestRepository)
        {
            _pullRequestRepository = pullRequestRepository;
        }

        public async Task Execute(JObject payload)
        {
            var organisation = payload["organization"]["login"].Value<string>();
            var pullRequest = new PullRequest
            {
                Id = payload["pull_request"]["id"].Value<int>(),
                Title = payload["pull_request"]["title"].Value<string>(),
                Number = payload["pull_request"]["number"].Value<int>(),
                Url = new Uri(payload["pull_request"]["url"].Value<string>()),
                CreatedAt = payload["pull_request"]["created_at"].Value<DateTimeOffset>(),
                UpdatedAt = payload["pull_request"]["updated_at"].Value<DateTimeOffset>()
            };

            await _pullRequestRepository.Upsert(organisation, pullRequest);
        }
    }
}