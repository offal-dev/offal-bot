﻿using System.Threading.Tasks;

namespace OffalBot.Domain.PullRequests
{
    public interface IPullRequestRepository
    {
        Task Upsert(string organisation, PullRequest pullRequest);
        Task UpdateStatus(string organisation, int pullRequestId, PullRequestStatus status);
    }
}