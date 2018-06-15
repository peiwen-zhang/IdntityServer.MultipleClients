using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using netcore.client4.Filter;

namespace netcore.client1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(option =>
            {
                option.DefaultScheme = "Cookies";
                option.DefaultSignInScheme = "Cookies";
                option.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", option => {
                //option.Events.OnRedirectToAccessDenied += 
            })
            .AddOpenIdConnect("oidc", config =>
            {
                config.SignInScheme = "Cookies";
                config.RequireHttpsMetadata = false;
                config.Authority = "http://10.37.11.12:8000/";
                config.ClientId = "netcore.client";
                config.ClientSecret = "secret";
                config.Scope.Add("netcore.api");
                config.Scope.Add("netfx.api");
                config.Scope.Add("openid");
                config.Scope.Add("profile");
                config.Scope.Add("offline_access");
                config.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                //这句设置当前httpcontext中是否包含Token
                config.SaveTokens = true;
                //标识是否自动请求Claims用户信息
                config.GetClaimsFromUserInfoEndpoint = true;
            })
             ;

            services.AddMvc(option =>
            {
                option.Filters.Add(typeof(IsLoginAttribute));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
