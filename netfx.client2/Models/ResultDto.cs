using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace netfx.client2.Models
{
    internal class ResultDto
    {
        public string id_token { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
    }
}