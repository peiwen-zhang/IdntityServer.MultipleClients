﻿using netfx.client1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace netfx.client1.Models
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