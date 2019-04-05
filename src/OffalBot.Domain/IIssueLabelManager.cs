using System.Threading.Tasks;

namespace OffalBot.Domain
{
    public interface IIssueLabelManager
    {
        Task SetLabelOnIssue(long repositoryId, int issueNumber, string label);
        Task RemoveLabel(long repositoryId, int issueNumber, string label);
    }
}