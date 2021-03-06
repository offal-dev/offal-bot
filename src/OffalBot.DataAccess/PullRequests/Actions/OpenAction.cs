﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OffalBot.Domain.PullRequests;
using OffalBot.Domain.PullRequests.Actions;

namespace OffalBot.DataAccess.PullRequests.Actions
{
    public class OpenAction : IPullRequestAction
    {
        private readonly IPullRequestRepository _pullRequestRepository;

        public OpenAction(IPullRequestRepository pullRequestRepository)
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
                Url = new Uri(payload["pull_request"]["html_url"].Value<string>()),
                CreatedAt = payload["pull_request"]["created_at"].Value<DateTimeOffset>(),
                UpdatedAt = payload["pull_request"]["updated_at"].Value<DateTimeOffset>(),
                Status = PullRequestStatus.Open,
                RepositoryName = payload["repository"]["full_name"].Value<string>(),
                RepositoryUrl = new Uri(payload["repository"]["html_url"].Value<string>()),
            };

            await _pullRequestRepository.Upsert(organisation, pullRequest);
        }
    }
}