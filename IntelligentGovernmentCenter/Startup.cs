using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.ResponseHandling;
using IntelligentGovernmentCenter.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntelligentGovernmentCenter
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
            services.AddDbContext<IntelGovContext>(option =>
            {
                option.UseSqlServer(@"Data Source=ZHANGPW-PC\SQLSERVER_ZPW; Initial Catalog = TEST; Integrated Security = True; User Id = sa; Pwd = 123");
            });

            //services.AddIdentityServer().AddDeveloperSigningCredential()
            //    .AddInMemoryIdentityResources(Config.GetIdentityResources())
            //    .AddInMemoryApiResources(Config.GetApiResources())
            //    .AddInMemoryClients(Config.GetClients()).AddResourceOwnerValidator<ValidUser>().AddProfileService<ProfileService>();

            var connectionString = @"Data Source=ZHANGPW-PC\SQLSERVER_ZPW; Initial Catalog = TEST; Integrated Security = True; User Id = sa; Pwd = 123;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer(o =>
            {
                //自定义授权服务器交互部分的数据
                o.UserInteraction = new IdentityServer4.Configuration.UserInteractionOptions()//改配置可以修改登录地址  授权地址   静态授权页面等
                {
                    //LoginUrl = "http://LocalHost:5001/Account/Login"

                };
            })
            .AddDeveloperSigningCredential()
            //.AddTestUsers(Config.GetUsers())
            // this adds the config data from DB (clients, resources)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600;
            })
            .AddResourceOwnerValidator<ValidUser>()
            .AddProfileService<ProfileService>()
            .AddAuthorizeInteractionResponseGenerator<MyInteractionResponseGenerator>();
            //services.TryAddTransient<IAuthorizeInteractionResponseGenerator, MyInteractionResponseGenerator>();

            services.AddMvc();
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
            app.UseIdentityServer();
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
