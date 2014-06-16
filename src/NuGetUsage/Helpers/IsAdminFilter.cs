using System;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using NuGetUsage.Controllers;

namespace NuGetUsage.Helpers
{
    public class IsAdminFilter : ActionFilterAttribute
    {
        private const string IsAdminKey = "adminKey";

        #region Overrides of ActionFilterAttribute

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            var baseController = filterContext.Controller as BaseController;
            if (baseController == null)
            {
                if (filterContext.Controller != null)
                    filterContext.Controller.ViewBag.IsAdmin = false;
                return;
            }

            var configuredAdminKey = CloudConfigurationManager.GetSetting("AdminKey");

            var queryString = filterContext.HttpContext.Request.QueryString;
            var adminKey = queryString[IsAdminKey];

            if (string.Compare(adminKey, configuredAdminKey, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                baseController.IsAdmin = true;
                baseController.ViewBag.IsAdmin = true;
            }
            else
            {
                baseController.ViewBag.IsAdmin = false;
            }

            base.OnActionExecuting(filterContext);
        }

        #endregion
    }
}