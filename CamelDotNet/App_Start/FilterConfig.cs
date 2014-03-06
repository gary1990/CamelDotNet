using CamelDotNet.Filter;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new PermissionAuthorizeAttribute());
        }
    }
}
