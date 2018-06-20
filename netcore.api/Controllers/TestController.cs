using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace netcore.api.Controllers
{
    [Produces("application/json")]
    //[Route("api/Test")]
    public class TestController : Controller
    {
        [HttpPost,Route("api/Test/GetTestName")]
        public string GetTestName()
        {
            return "你猜猜我是谁？";
        }
    }
}