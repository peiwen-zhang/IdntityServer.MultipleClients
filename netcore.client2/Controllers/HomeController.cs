using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using netcore.client2.Models;
using Newtonsoft.Json;

namespace netcore.client2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult Login()
        {
            return View();
        }

        public void LocalLogout()
        {
            if (Request.Cookies.TryGetValue("token", out string tokenCookies))
            {
                Response.Cookies.Delete("token");
            }
            if (Request.Cookies.TryGetValue("CurrentUser", out string currentUser))
            {
                Response.Cookies.Delete("CurrentUser");
            }

        }

        /// <summary>
        /// 本地账号密码登录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ActionResult LoalLogin(string userId, string password)
        {
            var localUsers = StaticLocalUsers.LocalUsers;
            var loginUser = localUsers.Find(x => x.Name == userId && x.Password == password);
            if (loginUser != null)
            {
                var json = JsonConvert.SerializeObject(loginUser);
                WriteCookie("CurrentUser", json, DateTime.Now.AddYears(1));
                return Redirect("~/Main/Index");
            }
            return Json("账号密码错误，请重新输入");
        }

        /// <summary>
        /// 远程登录
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> RemoteLogin()
        {
            //*************代码能进到这里  说明远程完成了登录**********
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            WriteCookie("token", accessToken, DateTime.Now.AddYears(1));
            Console.WriteLine("callback token:" + accessToken);

            var userinfo = GetUserInfoByToken(accessToken);
            WriteCookie("CurrentUser", JsonConvert.SerializeObject(userinfo.Result), DateTime.Now.AddYears(1));

            return Redirect("~/Main/Index");
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {

            if (Request.Cookies.TryGetValue("token", out string tokenCookies))
            {
                Response.Cookies.Delete("token");
            }
            if (Request.Cookies.TryGetValue("CurrentUser", out string currentUser))
            {
                Response.Cookies.Delete("CurrentUser");
            }

            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }


        /// <summary>
        /// 根据远程TOKEN获取当前用户信息
        /// 有则匹配，无则新增
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<UserInfo> GetUserInfoByToken(string token)
        {
            var dicClient = new DiscoveryClient("http://10.37.11.12:6000");
            dicClient.Policy.RequireHttps = false;
            var doc = await dicClient.GetAsync();

            var userInfoClient = new UserInfoClient(doc.UserInfoEndpoint);
            var userinfoResponse = await userInfoClient.GetAsync(token);
            var claims = userinfoResponse.Claims;

            var userinfo = new UserInfo();
            var remoteName = (claims.FirstOrDefault(x => x.Type == "name") ?? new System.Security.Claims.Claim("name", string.Empty)).Value;
            var localUser = StaticLocalUsers.LocalUsers.Find(x => x.RemoteName == remoteName);
            if (localUser == null && !string.IsNullOrEmpty(remoteName))
            {
                userinfo.Name = remoteName;
                userinfo.Password = "123";
                userinfo.RemoteName = remoteName;
                StaticLocalUsers.LocalUsers.Add(userinfo);//远程用户同步到本地
            }
            else
            {
                userinfo = localUser;
            }
            return userinfo;
        }


        /// <summary>
        /// 为当前请求编写COOKIE
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        /// <param name="expireTime"></param>
        private void WriteCookie(string cookieName, string cookieValue, DateTime expireTime)
        {
            HttpContext.Response.Cookies.Append(cookieName, cookieValue, new Microsoft.AspNetCore.Http.CookieOptions() { Expires = expireTime });
        }
    }
}
