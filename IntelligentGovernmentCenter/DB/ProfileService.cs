using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelligentGovernmentCenter.DB
{
    public class ProfileService : IProfileService
    {
        private IntelGovContext _userDb = null;


        public ProfileService(IntelGovContext userDB)
        {
            _userDb = userDB;
        }


        /// <summary>
        /// Token创建和调用期间抓取该数据
        /// 只要有关用户的身份信息单元被请求（例如在令牌创建期间或通过用户信息终点），就会调用此方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            //检测该请求是否需要抓取Claim  
            if (context.RequestedClaimTypes.Any())
            {
                //筛选原请求需要的类型
                var requestTypes = context.RequestedClaimTypes;

                //--------------------------以下内容仅供实现参考------------------------------------------------
                //depending on the scope accessing the user data.
                if (!string.IsNullOrEmpty(context.Subject.Identity.Name))
                {
                    //get user from db (in my case this is by email)
                    var user = _userDb.UserDb.AsQueryable().FirstOrDefault(x => x.Name == context.Subject.Identity.Name);

                    if (user != null)
                    {
                        //检索数据库  根据当前的subject找到所有的claims并且进行过滤
                        Claim claim = new Claim("name", user.Name);
                        //set issued claims to return
                        context.IssuedClaims.Add(claim);
                    }
                }
                else
                {
                    //get subject from context (this was set ResourceOwnerPasswordValidator.ValidateAsync),
                    //where and subject was set to my user id.
                    var userId = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub");

                    if (!string.IsNullOrEmpty(userId?.Value) && long.Parse(userId.Value) > 0)
                    {
                        //get user from db (find user by user id)
                        var user = _userDb.UserDb.Find(long.Parse(userId.Value));

                        // issue the claims for the user
                        if (user != null)
                        {
                            Claim claim = new Claim("name", user.Name);
                            context.IssuedClaims.Add(claim);
                        }
                    }
                }

            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// 验证用户是否已经激活【判断用户是否登录】
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            //因为只有subjectid在上下文中能取到   取出方法如下
            var keyId = context.Subject.Claims.First(x => x.Type == "sub").Value;
            var user = new User();
            if (long.TryParse(keyId, out long keyNum))//OIDC模式下传入的该值为主键
            {
                user = _userDb.UserDb.Find(keyNum);
            }
            else//PWD模式下传入的是账号
            {
                var sql = $@"select * from UserInfo where Name = '{keyId}'";
                user = _userDb.UserDb.FromSql(new RawSqlString(sql)).FirstOrDefault();
            }
            ////**********************************************-------待确定逻辑  此处是否有效------------------------***********
            context.IsActive = user != null;
            return Task.CompletedTask;
        }
    }
}
