using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace netcore.client2.Models
{
    public static class StaticLocalUsers
    {
        public static List<UserInfo> LocalUsers { get; set; }
        static StaticLocalUsers()
        {
            LocalUsers = new List<UserInfo>
            {
                new UserInfo(){ Name="zhangsan",Password = "123"}
            };
        }
    }
}