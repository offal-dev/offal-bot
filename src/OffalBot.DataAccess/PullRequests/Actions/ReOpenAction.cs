using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OffalBot.Domain.PullRequests;
using OffalBot.Domain.PullRequests.Actions;

namespace OffalBot.DataAccess.PullRequests.Actions
{
    public class ReOpenAction : IPullRequestAction
    {
        private readonly IPullRequestRepository _pullRequestRepository;

        public ReOpenAction(IPullRequestRepository pullRequestRepository)
        {
            _pullRequestRepository = pullRequestRepository;
        }

        public async Task Execute(JObject payload)
        {
            var organisation = payload["organization"]["login"].Value<string>();
            var pullRequestId = payload["pull_request"]["id"].Value<int>();

            await _pullRequestRepository.UpdateStatus(
                organisation,
                pullRequestId,
                PullRequestStatus.Open);
        }
    }
}