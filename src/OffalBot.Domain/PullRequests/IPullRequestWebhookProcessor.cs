using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OffalBot.Domain.PullRequests
{
    public interface IPullRequestWebhookProcessor
    {
        Task Execute(JObject payload);
    }
}