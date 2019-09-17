using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OffalBot.Domain.PullRequests.Actions
{
    public interface IPullRequestAction
    {
        Task Execute(JObject payload);
    }
}