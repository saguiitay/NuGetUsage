using System;
using System.Collections.Generic;
using NuGetUsage.Models;

namespace NuGetUsage.Helpers
{
    public class PackageRefNameEqualityComparer : IEqualityComparer<PackageRef>
    {
        #region Implementation of IEqualityComparer<in Package>

        public bool Equals(PackageRef x, PackageRef y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public int GetHashCode(PackageRef obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}