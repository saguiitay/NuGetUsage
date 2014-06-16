using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using NuGetUsage.Models;
using NuGetUsage.NuGet;
using Raven.Client;

namespace NuGetUsage.Service
{
    public class DataService : IDataService
    {
        private readonly IGitHubService _gitHubService;
        private readonly Func<IDocumentSession> _documentSessionFunc;

        public DataService()
            : this(new GitHubService(), MvcApplication.DocumentStore.OpenSession)
        {
        }

        public DataService(IGitHubService gitHubService, Func<IDocumentSession> documentSessionFunc)
        {
            _gitHubService = gitHubService;
            _documentSessionFunc = documentSessionFunc;
        }

        #region Implementation of IDataService

        public async Task AddRepositoryInfo(string fullname)
        {
            var isValid = await _gitHubService.IsValidRepositoryName(fullname);
            if (!isValid)
                return;

            using (var session = _documentSessionFunc())
            {
                var existing = session.Query<Repo>().FirstOrDefault(x => x.FullName == fullname);
                if (existing != null &&
                    existing.UpdatedFromGitHub != null &&
                    existing.UpdatedFromGitHub.Value.AddDays(5) > DateTime.UtcNow)
                {
                    return;
                }
            }

            var repository = await _gitHubService.GetRepository(fullname);
            if (repository == null)
                return;


            await AddRepositoryInfo(repository);
        }

        public async Task AddRepositoryInfo(Repo repository)
        {
            repository.UpdatedFromGitHub = DateTime.UtcNow;

            using (var session = _documentSessionFunc())
            {
                session.Store(repository);

                var nugetContext = new V1FeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc"));

                var packages = repository.Packages
                    //.AsParallel()
                    .Select(packageRef =>
                        {
                            var existing = session.Load<Package>("Packages/" + packageRef.Name);
                            if (existing != null && existing.UpdatedFromNuget != null && existing.UpdatedFromNuget.Value.AddDays(5) > DateTime.UtcNow)
                                return null;

                            var nugetPackage = nugetContext.GetPackageVersionOrLatest(packageRef);
                            if (nugetPackage == null)
                                return null;
                            return CreatePackage(nugetPackage, existing);
                        })
                    .Where(x => x != null);

                foreach (var package in packages)
                    session.Store(package);


                session.SaveChanges();
            }
        }

        private static Package CreatePackage(V1FeedPackage package, Package existing = null)
        {
            if (existing == null)
                existing = new Package();

            existing.IconUrl = package.IconUrl;
            existing.Name = package.Id;
            existing.Version = package.Version;
            existing.Updated = package.LastUpdated;
            existing.Summary = !string.IsNullOrEmpty(package.Summary) ? package.Summary : package.Description;
            existing.Tags = package.Tags != null ? package.Tags.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) : new string[0];

            return existing;
        }

        public async Task GetStarredRepositories()
        {
            foreach (var starredTask in _gitHubService.GetStarredRepositories())
            {
                var repos = await starredTask;

                Task.WaitAll(repos.Select(x => AddRepositoryInfo(x.FullName)).ToArray());
            }
        }

        #endregion
    }
}