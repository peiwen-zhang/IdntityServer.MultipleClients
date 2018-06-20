using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace IntelligentGovernmentCenter.DB
{
    public class ValidUser : IResourceOwnerPasswordValidator
    {
        private IntelGovContext _userDb = null;


        public ValidUser(IntelGovContext userDB) {
            _userDb = userDB;
        }


        /// <summary>
        /// 上下文验证执行  就是针对client发过来的请求  进行数据校验
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var validUserName = context.UserName;
            var validPwd = context.Password;

            var sql = $@"select * from UserInfo where Name = '{validUserName}' And Password = '{validPwd}'";
            var user = _userDb.UserDb.FromSql(sql).FirstOrDefault();

            if (user != null)
            {
                var currentTime = DateTime.UtcNow;

                //------------------------------------这一步相当于于给当前请求附加声明，对应的可以在客户端Authorize("***=***")中同步配置
                var testClaims = new Claim[] { new Claim("Sex", "测试男") };
                context.Result = new GrantValidationResult(user.Id.ToString(), AuthenticationMethods.Password, currentTime, testClaims);
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant,"invalid user and password line1");
            }
            return Task.CompletedTask;
        }
    }
}
