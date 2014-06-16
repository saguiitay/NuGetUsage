using System.Web.Mvc;
using Raven.Client;

namespace NuGetUsage.Controllers
{
    public class BaseController : Controller
    {
        public bool IsAdmin { get; set; }
        public IDocumentSession DocumentSession { get; private set; }

        #region Overrides of Controller

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            DocumentSession = MvcApplication.DocumentStore.OpenSession();
            DocumentSession.Advanced.MaxNumberOfRequestsPerSession = 1000;

            base.OnActionExecuting(filterContext);
        }

        #region Overrides of Controller

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            DocumentSession.Dispose();
        }

        #endregion

        #endregion
    }
}