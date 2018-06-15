using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace netcore.client2.Filter
{
    public class IsLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var controller = filterContext.RouteData.Values["Controller"];
            var action = filterContext.RouteData.Values["Action"];

            if (controller.ToString().ToLower() == "home")
            {
                return;
            }

            //检测当前请求中含有等路人信息
            var cookies = filterContext.HttpContext.Request.Cookies;
            if (cookies == null || cookies.Count <= 0)
            {
                //重定向到LOGIN
                filterContext.Result = new RedirectResult("~/Home/Login");
            }
            if (cookies["CurrentUser"] == null)
            {
                filterContext.Result = new RedirectResult("~/Home/Login");
            }
        }
    }
}