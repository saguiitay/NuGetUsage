using System.Collections.Generic;
using System.Threading.Tasks;
using NuGetUsage.Models;

namespace NuGetUsage.Service
{
    public interface IGitHubService
    {
        Task<bool> IsValidRepositoryName(string fullname);
        Task<Repo> GetRepository(string fullname);

        IEnumerable<Task<IEnumerable<Repo>>> GetStarredRepositories();
    }
}