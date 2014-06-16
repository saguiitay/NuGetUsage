using System.Collections.Generic;

namespace NuGetUsage.Models
{
    public class PackageRefUsage
    {
        public string PackageName { get; set; }
        public IEnumerable<string> Repos { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public int Count { get; set; }
    }
}