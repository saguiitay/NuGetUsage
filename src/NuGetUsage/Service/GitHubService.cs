using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using NuGetUsage.Helpers;
using NuGetUsage.Models;
using Octokit;

namespace NuGetUsage.Service
{
    public class GitHubService : IGitHubService
    {
        private const int NumRetries = 3;

        private readonly GitHubClient _github;
        private readonly IEqualityComparer<PackageRef> _packageEqualityComparer;

        public GitHubService()
        {
            var githubKey = CloudConfigurationManager.GetSetting("GitHubKey");
            _github = new GitHubClient(new ProductHeaderValue("NuGetUsage"))
                {
                    Credentials = new Credentials(githubKey)
                };

            _packageEqualityComparer = new PackageRefNameEqualityComparer();
        }

        public async Task<bool> IsValidRepositoryName(string fullname)
        {
            if (string.IsNullOrWhiteSpace(fullname))
                return false;

            var nameParts = fullname.Split('/');
            return nameParts.Length == 2;
        }

        public async Task<Repo> GetRepository(string fullname)
        {
            var nameParts = fullname.Split('/');
            if (nameParts.Length < 2)
                return null;

            var repository = await GetRepository(nameParts[0], nameParts[1]);
            if (repository == null)
                return null;

            var packages = await GetRepoPackage(repository);

            return new Repo
                {
                    ForksCount = repository.ForksCount,
                    Name = repository.Name,
                    Owner = repository.Owner.Login,
                    RepoId = repository.Id.ToString(),
                    Updated = repository.UpdatedAt.UtcDateTime,
                    WatchersCount = repository.WatchersCount,
                    Packages = packages
                };
        }

        public IEnumerable<Task<IEnumerable<Repo>>> GetStarredRepositories()
        {
            var searchRepositoriesRequest = new SearchRepositoriesRequest("")
                {
                    Language = Language.CSharp,
                    Fork = ForkQualifier.ExcludeForks,
                    Sort = RepoSearchSort.Stars,
                    Order = SortDirection.Descending,
                    Page = 0,
                    PerPage = 50
                };

            //SearchRepositoryResult searchRepositoryResult;

            int count = 0;
            do
            {
                var searchRepositoryTask = _github.Search.SearchRepo(searchRepositoriesRequest);

                yield return searchRepositoryTask.ContinueWith(
                    x =>
                        {
                            return x.Result.Items.Select(repository => new Repo
                                {
                                    ForksCount = repository.ForksCount,
                                    Name = repository.Name,
                                    Owner = repository.Owner.Login,
                                    RepoId = repository.Id.ToString(),
                                    Updated = repository.UpdatedAt.UtcDateTime,
                                    WatchersCount = repository.WatchersCount
                                });
                        });

                searchRepositoriesRequest.Page++;
                count += searchRepositoriesRequest.PerPage;

            } while (count < 1000);
        }



        private async Task<List<PackageRef>> GetRepoPackage(Repository repo)
        {
            var codeSearch = new SearchCodeRequest("packages.config")
                {
                    Repo = repo.FullName,
                    In = new[] { CodeInQualifier.Path }
                };

            var codeResult = await SearchCode(codeSearch);
            if (codeResult == null || codeResult.Items == null)
                return new List<PackageRef>(0);

            var allPackages = codeResult.Items
                .AsParallel()
                .Where(p => p.Name == "packages.config")
                .Select(packageFile => _github.GitDatabase.Blob.Get(repo.Owner.Login, repo.Name, packageFile.Sha).Result)
                .Select(blob => blob.Content)
                .Select(Convert.FromBase64String)
                .SelectMany(GetPackages)
                .ToList();

            var packages = allPackages
                .OrderBy(p => p.Name).ThenByDescending(p => p.Version, StringComparer.OrdinalIgnoreCase)
                .Distinct(_packageEqualityComparer)
                .ToList();

            return packages;
        }

        private IEnumerable<PackageRef> GetPackages(byte[] arg)
        {
            using (var memoryStream = new MemoryStream(arg))
            {
                var xdoc = XDocument.Load(memoryStream);

                var packages = xdoc.Descendants("package");
                foreach (var package in packages)
                {
                    var id = package.Attribute("id").Value;
                    var version = package.Attribute("version").Value;

                    yield return new PackageRef
                        {
                            Name = id,
                            Version = version
                        };
                }

            }
        }

        private async Task<Repository> GetRepository(string owner, string name)
        {
            for (int i = 0; i < NumRetries; i++)
            {
                try
                {
                    var repo = await _github.Repository.Get(owner, name);
                    return repo;
                }
                catch (ApiException)
                {}
            }

            return null;
        }

        private async Task<SearchCodeResult> SearchCode(SearchCodeRequest searchCodeRequest)
        {
            for (int i = 0; i < NumRetries; i++)
            {
                try
                {
                    var codeResult = await _github.Search.SearchCode(searchCodeRequest);
                    return codeResult;
                }
                catch (ApiException)
                { }
            }

            return null;
        }
    }
}