using Octokit;
using OffalBot.Domain;

namespace OffalBot.DataAccess
{
    public class LabelMaker : ILabelMaker
    {
        private readonly IGitHubClient _githubClient;

        public LabelMaker(IGitHubClient githubClient)
        {
            _githubClient = githubClient;
        }
    }
}