using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace netfx.api.Filter
{
    public class IsLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            //检测当前请求中含有等路人信息
            var cookies = filterContext.HttpContext.Request.Cookies;
            if (cookies == null || cookies.Count <= 0)
            {
                //重定向到LOGIN
                filterContext.Result = new RedirectResult("/Home/Login");
            }
            if (cookies["CurrentUser"] == null)
            {
                filterContext.Result = new RedirectResult("/Home/Login");
            }
        }
    }
}