namespace OffalBot.Domain
{
    public class ReviewRequest
    {
        public long RepositoryId { get; set; }
        public string PullRequestComment { get; set; }
        public string ReviewState { get; set; }
        public int PullRequestNumber { get; set; }
        public int InstallationId { get; set; }
    }
}