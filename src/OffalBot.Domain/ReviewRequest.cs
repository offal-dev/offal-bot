namespace OffalBot.Domain
{
    public class ReviewRequest
    {
        public int RepositoryId { get; set; }
        public string PullRequestComment { get; set; }
        public string ReviewState { get; set; }
        public int PullRequestNumber { get; set; }
    }
}