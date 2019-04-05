using System.Threading.Tasks;

namespace OffalBot.Domain
{
    public interface ILabelMaker
    {
        Task CreateIfMissing(long repositoryId, string labelName, string labelColour);
    }
}