// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Api.Controllers
{
    public class IdentityController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var dicClient = new DiscoveryClient("http://10.37.11.12:7000");
            dicClient.Policy.RequireHttps = false;
            var doc = await dicClient.GetAsync();
            var token = Request.Headers["Authorization"].ToString().Trim().Substring(6).Trim();
            var introspectionEndPoint = doc.IntrospectionEndpoint;

            var keyPairs = new List<KeyValuePair<string, string>>();
            keyPairs.Add(new KeyValuePair<string, string>("token_type_hint", "access_token"));
            keyPairs.Add(new KeyValuePair<string, string>("token", token));
            keyPairs.Add(new KeyValuePair<string, string>("client_id", "netfx.api.TEST"));
            keyPairs.Add(new KeyValuePair<string, string>("client_secret", "123"));

            var content = new FormUrlEncodedContent(keyPairs);
            var client = new HttpClient();
            client.SetBearerToken(token);
            var response = await client.PostAsync(introspectionEndPoint, content);
            var result = await response.Content.ReadAsStringAsync();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}