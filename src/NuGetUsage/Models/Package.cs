using System;
using System.Collections.Generic;

namespace NuGetUsage.Models
{
    public class Package : PackageRef
    {
        public string Summary { get; set; }
        public DateTime? Updated { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public string IconUrl { get; set; }

        public DateTime? UpdatedFromNuget { get; set; }

    }
}