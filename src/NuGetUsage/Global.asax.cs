using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NuGetUsage.RavenDb;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace NuGetUsage
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            InitDocumentStore();
        }

        private static void InitDocumentStore()
        {
            DocumentStore = new EmbeddableDocumentStore().Initialize();

            DocumentStore.ExecuteIndex(new Repos_PackagesUsage());
        }

        //private static readonly Lazy<IDocumentStore> LazyDocumentStore = new Lazy<IDocumentStore>(InitDocumentStore);

        public static IDocumentStore DocumentStore { get; private set; }
    }
}
