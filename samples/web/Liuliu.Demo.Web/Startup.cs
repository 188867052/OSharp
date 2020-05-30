using DependencyInjection.Analyzer;
using Liuliu.Demo.Authorization;
using Liuliu.Demo.Identity;
using Liuliu.Demo.Systems;
using Liuliu.Demo.Web.Startups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OSharp.AspNetCore;
using OSharp.AspNetCore.Routing;
using OSharp.AutoMapper;
using OSharp.Log4Net;
using OSharp.Swagger;

namespace Liuliu.Demo.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOSharp()
                .AddPack<Log4NetPack>()
                .AddPack<AutoMapperPack>()
                .AddPack<EndpointsPack>()
                .AddPack<SwaggerPack>()
                //.AddPack<RedisPack>()
                .AddPack<AuthenticationPack>()
                .AddPack<FunctionAuthorizationPack>()
                .AddPack<DataAuthorizationPack>()
                .AddPack<SqlServerDefaultDbContextMigrationPack>()
                .AddPack<AuditPack>();
            services.AddDependencyInjectionAnalyzer();
            services.AddRouteAnalyzer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/#/500");
                app.UseHsts();
                //app.UseHttpsRedirection(); // 启用HTTPS
            }

            //app.UseMiddleware<HostHttpCryptoMiddleware>();
            //app.UseMiddleware<JsonNoFoundHandlerMiddleware>();
            app.UseMiddleware<JsonExceptionHandlerMiddleware>();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseOSharp();
        }
    }
}