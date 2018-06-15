using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using netcore.client3.Models;
using Newtonsoft.Json;

namespace netcore.client3.Controllers
{
    //[Authorize]
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            var usercache = Request.Cookies["CurrentUser"].ToString();

            var userInfo = JsonConvert.DeserializeObject<UserInfo>(usercache);
            return View(userInfo);
        }

        [Authorize]
        public async Task<ActionResult> RemoteCallAPI()
        {
            //发起远程调用
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            Console.WriteLine($"access_token:{accessToken}");
            var client = new HttpClient();
            client.SetBearerToken(accessToken);


            //访问netfx对应的identity标识信息
            var netfxUrl = @"http://10.37.11.12:6061/Identity/Get";
            var netfxResponse = await client.GetAsync(netfxUrl);
            var netfxContent = await netfxResponse.Content.ReadAsStringAsync();

            //访问netcore对应的identity标识
            var netcoreUrl = @"http://10.37.11.12:6062/Identity";
            var netcoreResponse = await client.GetAsync(netcoreUrl);
            var netcoreContent = await netcoreResponse.Content.ReadAsStringAsync();

            return Json(new { netfxContent, netcoreContent });
        }
    }
}