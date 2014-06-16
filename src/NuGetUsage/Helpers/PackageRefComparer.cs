using System;
using System.Collections.Generic;
using NuGetUsage.Models;

namespace NuGetUsage.Helpers
{
    public class PackageRefEqualityComparer : IEqualityComparer<PackageRef>
    {
        #region Implementation of IEqualityComparer<in Package>

        public bool Equals(PackageRef x, PackageRef y)
        {
            return string.Compare(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public int GetHashCode(PackageRef obj)
        {
            return obj.Id.GetHashCode();
        }

        #endregion
    }
}