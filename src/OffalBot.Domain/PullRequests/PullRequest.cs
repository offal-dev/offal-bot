﻿using System;

namespace OffalBot.Domain.PullRequests
{
    public class PullRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public Uri Url { get; set; }
        public PullRequestStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public string RepositoryName { get; set; }
        public Uri RepositoryUrl { get; set; }
    }
}