using System;
using System.Collections.Generic;

namespace NuGetUsage.Models
{
    public class Repo
    {
        public string Id { get { return "Repositories/" + RepoId; } }

        public DateTime? UpdatedFromGitHub { get; set; }

        public string RepoId { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string FullName { get { return Owner + "/" + Name; }}
        public DateTime Updated { get; set; }

        public int ForksCount { get; set; }
        public int WatchersCount { get; set; }

        public IEnumerable<PackageRef> Packages { get; set; }
    }
}
