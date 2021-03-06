﻿using CamelDotNet.Lib;
using CamelDotNet.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Filter
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : FilterAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {    
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            string temp = httpContext.Request.RequestContext.RouteData.GetRequiredString("controller") + "_" + httpContext.Request.RequestContext.RouteData.GetRequiredString("action");

            AllowPermisson allowPermisson = new AllowPermisson();

            if(allowPermisson.Allowed.Contains(temp))
            {
                return;
            }

            if (httpContext.Request.RequestContext.RouteData.GetRequiredString("controller") == "Account" && httpContext.Request.RequestContext.RouteData.GetRequiredString("action") == "Login")
            {
                return;
            }

            IPrincipal user = httpContext.User;
            if(!user.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }
            //var s = Common.GetCurSystemRole();
            //if (Common.GetCurSystemRole() == "Admin")
            //{
            //    return;
            //}

            //permission
            //var s = Common.GetCurPermissionList().Contains(temp);
            if (Common1.GetCurPermissionList().Contains(temp))
            {
                return;
            }
            else 
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }
        }
    }
}