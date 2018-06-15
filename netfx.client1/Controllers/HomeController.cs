using IdentityModel;
using IdentityModel.Client;
using netfx.client1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace netfx.hybrid.client.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
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
            return Json("账号密码错误，请重新输入", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 远程登录
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> RemoteLogin()
        {
            var client = new DiscoveryClient("http://10.37.11.12:6000");
            client.Policy.RequireHttps = false;
            var doc = await client.GetAsync();
            var request = new RequestUrl(doc.AuthorizeEndpoint);
            var url = request.CreateAuthorizeUrl(
                        clientId: "netfx.client",
                        responseType: OidcConstants.ResponseTypes.CodeIdToken,
                        responseMode: OidcConstants.ResponseModes.FormPost,
                        redirectUri: "http://10.37.11.12:6001/Home/CallBack",
                        scope: "netfx.api netcore.api openid",
                        state: CryptoRandom.CreateUniqueId(),
                        nonce: CryptoRandom.CreateUniqueId());

            return Redirect(url);//302重定向到远程服务器登录
        }

        /// <summary>
        /// 远程登录id_token和token的回写请求
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> CallBack()
        {
            //回写的时候验证id_token 待通过之后 使用code换取token  写入currentUser对应cookie  同时将当前token id_token写入cookie 【生产环境需要加密存储】

            //1.检测id_token
            var id_token = Request.Form["id_token"];
            //************************************************************待使用jwks_uri对id_token进行校验*****************************************


            //2.使用code获取Token
            var dicClient = new DiscoveryClient("http://10.37.11.12:6000");
            dicClient.Policy.RequireHttps = false;
            var doc = await dicClient.GetAsync();
            var code = Request.Form["code"];

            #region 组织参数

            var tokenEndPoint = doc.TokenEndpoint;
            var keyPairs = new List<KeyValuePair<string, string>>();
            keyPairs.Add(new KeyValuePair<string, string>("client_id", "netfx.client"));
            keyPairs.Add(new KeyValuePair<string, string>("client_secret", "secret"));
            keyPairs.Add(new KeyValuePair<string, string>("code", code));
            keyPairs.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            keyPairs.Add(new KeyValuePair<string, string>("redirect_uri", "http://10.37.11.12:6001/Home/CallBack"));
            var content = new FormUrlEncodedContent(keyPairs);
            #endregion

            var client = new HttpClient();
            var response = await client.PostAsync(tokenEndPoint, content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<ResultDto>(result);
                if (tokenResponse != null)
                {
                    //组织当前用户
                    var user = await GetUserInfoByToken(tokenResponse.access_token);

                    //3.写入待写入COOKIE
                    WriteCookie("id_token", id_token, DateTime.Now.AddYears(1));
                    WriteCookie("code", code, DateTime.Now.AddYears(1));
                    WriteCookie("token", result, DateTime.Now.AddYears(1));
                    WriteCookie("CurrentUser", JsonConvert.SerializeObject(user), DateTime.Now.AddYears(1));


                    return Redirect("~/Main/Index");
                }
            }
            return Redirect("~/Home/Error");

        }

        /// <summary>
        /// 本地退出登录
        /// </summary>
        /// <returns></returns>
        public async Task LocalLogout()
        {
            if (Request.Cookies["id_token"] != null)
            {
                Response.Cookies["id_token"].Expires = DateTime.Now.AddSeconds(-1);
            }
            if (Request.Cookies["code"] != null)
            {
                Response.Cookies["code"].Expires = DateTime.Now.AddSeconds(-1);
            }
            if (Request.Cookies["token"] != null)
            {
                Response.Cookies["token"].Expires = DateTime.Now.AddSeconds(-1);
            }
            if (Request.Cookies["CurrentUser"] != null)
            {
                Response.Cookies["CurrentUser"].Expires = DateTime.Now.AddSeconds(-1);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Logout()
        {

            //退出远程COOKIE
            var id_token = Response.Cookies["id_token"].Value;
            var state = CryptoRandom.CreateUniqueId();
            var signouturl = "http://10.37.11.12:6000/connect/endsession?";
            var logoutCallBack = @"http://10.37.11.12:6001/Home/LogoutCallBack";
            signouturl += $"id_token={id_token}&post_logout_redirect_uri={logoutCallBack}&state={state}";


            if (Request.Cookies["id_token"] != null)
            {
                Response.Cookies["id_token"].Expires = DateTime.Now.AddSeconds(-1);
            }
            if (Request.Cookies["code"] != null)
            {
                Response.Cookies["code"].Expires = DateTime.Now.AddSeconds(-1);
            }
            if (Request.Cookies["token"] != null)
            {
                Response.Cookies["token"].Expires = DateTime.Now.AddSeconds(-1);
            }
            if (Request.Cookies["CurrentUser"] != null)
            {
                Response.Cookies["CurrentUser"].Expires = DateTime.Now.AddSeconds(-1);
            }

            return Redirect(signouturl);
        }


        public async Task<ActionResult> LogoutCallBack()
        {
            return Json("远程退出回调本地！");
        }

        /// <summary>
        /// 根据远程TOKEN获取当前用户信息
        /// 有则匹配，无则新增
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<UserInfo> GetUserInfoByToken(string token)
        {
            var userInfoClient = new UserInfoClient(@"http://10.37.11.12:6000/connect/userinfo");
            var userinfoResponse = await userInfoClient.GetAsync(token);
            var claims = userinfoResponse.Claims;

            var userinfo = new UserInfo();
            var remoteName = (claims.FirstOrDefault(x=>x.Type == "name")??new System.Security.Claims.Claim("name",string.Empty)).Value;
            var localUser = StaticLocalUsers.LocalUsers.Find(x=>x.RemoteName == remoteName);
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
            var cookie = new HttpCookie(cookieName)
            {
                Value = cookieValue,
                Expires = expireTime
            };
            HttpContext.Response.Cookies.Add(cookie);
        }
    }
}