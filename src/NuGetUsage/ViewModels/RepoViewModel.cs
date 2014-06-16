using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NuGetUsage.Models;

namespace NuGetUsage.ViewModels
{
    public class RepoViewModel : ICloneable
    {
        [Required]
        public string Query { get; set; }

        public SortField SortField { get; set; }
        public SortOrder SortOrder { get; set; }

        public Repo Repo { get; set; }
        public IEnumerable<PackageRef> Packages { get; set; }
        
        public bool AllowImport { get; set; }
        public IEnumerable<string> Suggestions { get; set; }

        public string AdminKey { get; set; }

        #region Implementation of ICloneable

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public RepoViewModel Clone()
        {
            return (RepoViewModel)MemberwiseClone();
        }

        #endregion
    }
}