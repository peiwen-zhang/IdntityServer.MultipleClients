using netfx.api.Filter;
using netfx.hybrid.client.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace netfx.hybrid.client.Controllers
{
    [IsLogin]
    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Index()
        {
            var usercache = Request.Cookies["CurrentUser"].Value;
            usercache = Server.UrlDecode(usercache);
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(usercache);

            return View(userInfo);
        }

        public async Task<ActionResult> RemoteCallAPI()
        {
            if (Request.Cookies["token"] == null)
            {
                return Redirect("~/Home/Login");
            }

            //发起远程调用
            var token = JsonConvert.DeserializeObject<ResultDto>(Request.Cookies["token"].Value);
            //var token = Request.Cookies["token"];
            var client = new HttpClient();
            client.SetBearerToken(token.access_token);

            //访问netfx对应的identity标识信息
            var netfxUrl = @"http://10.37.11.12:6061/Identity/Get";
            var netfxResponse = await client.GetAsync(netfxUrl);
            var netfxContent = await netfxResponse.Content.ReadAsStringAsync();

            //访问netcore对应的identity标识
            var netcoreUrl = @"http://10.37.11.12:6062/Identity";
            var netcoreResponse = await client.GetAsync(netcoreUrl);
            var netcoreContent = await netcoreResponse.Content.ReadAsStringAsync();

            return Json(new { netfxContent, netcoreContent }, JsonRequestBehavior.AllowGet);
        }
    }
}