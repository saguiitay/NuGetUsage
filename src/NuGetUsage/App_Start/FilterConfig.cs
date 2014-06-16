using System.Web;
using System.Web.Mvc;
using NuGetUsage.Helpers;

namespace NuGetUsage
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new IsAdminFilter());
        }
    }
}
