using System.Threading.Tasks;

namespace OffalBot.Domain
{
    public interface ILabelMaker
    {
        Task CreateIfMissing(int repositoryId, string labelName, string labelColour);
    }
}