using System.Linq;
using NuGetUsage.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace NuGetUsage.RavenDb
{
    public class Repos_PackagesUsage : AbstractIndexCreationTask<Repo, PackageRefUsage>
    {
        public Repos_PackagesUsage()
        {
            Map = docs => from repo in docs
                from packageRef in repo.Packages
                let package = LoadDocument<Package>(packageRef.Id)
                select new
                    {
                        PackageName = packageRef.Name,
                        Repos = new string[] {repo.FullName},
                        Tags = package.Tags,
                        Count = 1
                    };

            Reduce = results => from r in results
                group r by r.PackageName
                into g
                select new
                    {
                        PackageName = g.Key,
                        Repos = g.SelectMany(x => x.Repos).Distinct(),
                        Tags = g.SelectMany(x => x.Tags).Distinct(),
                        Count = g.Sum(x => x.Count)
                    };

            Sort(x => x.Count, SortOptions.Int);
            Store(x=> x.Tags, FieldStorage.Yes);
        }
    }
}