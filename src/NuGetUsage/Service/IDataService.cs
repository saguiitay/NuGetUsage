using System.Threading.Tasks;

namespace NuGetUsage.Service
{
    public interface IDataService
    {
        Task AddRepositoryInfo(string fullname);

        Task GetStarredRepositories();
    }
}