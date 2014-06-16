using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGetUsage.RavenDb;
using NuGetUsage.Service;
using Raven.Client.Embedded;

namespace ImporterTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var documentStore = new EmbeddableDocumentStore
                {
                    DataDirectory = "C:\\CodeItay\\NuGetUsage\\src\\NuGetUsage\\Data"
                }.Initialize();
            documentStore.ExecuteIndex(new Repos_PackagesUsage());

            var githubService = new GitHubService();

            var dataService = new DataService(githubService, () =>
                {
                    var x = documentStore.OpenSession();
                    x.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;
                    return x;
                });
            foreach (var starredTask in githubService.GetStarredRepositories())
            {
                var repos = starredTask.Result;

                foreach (var repo in repos)
                {
                    Console.WriteLine("Importing " + repo.FullName);
                    dataService.AddRepositoryInfo(repo.FullName).Wait();
                }
            }
        }
    }
}
