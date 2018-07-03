using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.DB
{
    [Table("UserInfo")]
    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Sex { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// 设置指定用户可被请求访问的权限列表  以逗号隔开 0：Id 1:Name 2:Sex
        /// </summary>
        public string ProfileType { get; set; }


        //待定处理
        [NotMapped]
        public IEnumerable<Claim> Cliams { get; set; }
    }
}
