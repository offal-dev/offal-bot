using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OffalBot.Domain
{
    public interface IPullRequestWebhookProcessor
    {
        Task Execute(JObject payload);
    }
}