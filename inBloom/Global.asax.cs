using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using inBloomApiLibrary;

namespace inBloom
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalFilters.Filters.Add(new oAuthActionFilter());
        }       
    }

    public class oAuthActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;

            // Check for a token in the session already, and if found, no action is required
            if (context.Session["access_token"] != null)
            {
                //RedirectToHome(filterContext);
                return;
            }

            // Init oAuth
            var oAuth = new OAuth();

            // We get a code back from the first leg of OAuth process.  If we don't have one, let's get it.
            if (context.Request.QueryString["code"] == null)
            {
                string path = oAuth.CallAuthorization(null, null);

                //context.Response.Redirect(path);
                filterContext.Result = new RedirectResult(path);

                return;
            }
            
            // Otherwise, we have a code, we can run the second leg of OAuth process.
            string code = context.Request.QueryString["code"];
            string authorization = oAuth.CallAuthorization(null, code);

            // OAuth successful so get values, store in session and continue
            if (authorization == "OAuthSuccess")
            {
                // Ensure that all required values were retrieved from the OAuth login
                if (oAuth.AccessToken != null && oAuth.UserFullName != null && oAuth.UserSLIRoles != null && oAuth.UserId != null)
                {
                    // Authorization successful; set session variables
                    context.Session.Add("access_token", oAuth.AccessToken);
                    context.Session.Add("user_FullName", oAuth.UserFullName);
                    context.Session.Add("user_SLIRoles", oAuth.UserSLIRoles);
                    context.Session.Add("user_ID", oAuth.UserId);

                    // Redirect to default page
                    RedirectToHome(filterContext);
                }
            }
        }

        private static void RedirectToHome(ActionExecutingContext filterContext)
        {
            var redirectAction = new RouteValueDictionary();
            redirectAction.Add("action", "Index");
            redirectAction.Add("controller", "Home");

            filterContext.Result = new RedirectToRouteResult(redirectAction);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //throw new NotImplementedException();
        }
    }
}