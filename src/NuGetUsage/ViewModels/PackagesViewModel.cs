using System.Collections.Generic;
using NuGetUsage.Models;

namespace NuGetUsage.ViewModels
{
    public class PackageViewModel
    {
        public Package Package { get; set; }

        public IEnumerable<Repo> Repos { get; set; }

        public IEnumerable<string> Suggestions { get; set; }
    }
}