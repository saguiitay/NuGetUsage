using System.Data.Services.Client;
using System.Linq;
using NuGetUsage.Models;

namespace NuGetUsage.NuGet
{
    public partial class V1FeedContext
    {
        public V1FeedPackage GetPackageVersionOrLatest(PackageRef packageRef)
        {
            V1FeedPackage specificVersion = null;
            try
            {
                specificVersion = Packages
                    .Where(p => p.Id == packageRef.Name)
                    .Where(p => p.IsLatestVersion)
                    .SingleOrDefault();

                if (specificVersion == null)
                {
                    specificVersion = Packages
                        .Where(p => p.Id == packageRef.Name)
                        .Where(p => p.Version == packageRef.Version)
                        .SingleOrDefault();
                }
            }
            catch (DataServiceQueryException)
            {
            }

            return specificVersion;

        }
    }
}