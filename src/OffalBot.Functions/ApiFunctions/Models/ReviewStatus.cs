using System;

namespace OffalBot.Functions.ApiFunctions.Models
{
    public class ReviewStatus
    {
        public GithubUser Reviewer { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        public ReviewState State { get; set; }
    }

    public enum ReviewState
    {
        ChangesRequested,
        Approved,
        Rejected
    }
}