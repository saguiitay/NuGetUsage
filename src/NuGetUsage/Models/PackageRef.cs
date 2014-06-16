namespace NuGetUsage.Models
{
    public class PackageRef
    {
        public string Id { get { return "Packages/" + Name; } }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}