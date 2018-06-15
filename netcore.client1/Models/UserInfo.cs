using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace netcore.client1.Models
{
    public class UserInfo
    {
        /// <summary>
        /// 本地名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 本地登录密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 远程名称【为了与本地建立连接】
        /// </summary>
        public string RemoteName { get; set; }
    }
}