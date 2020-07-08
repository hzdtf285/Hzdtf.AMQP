using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hzdtf.RabbitV2.AspNet.Core;
using Hzdtf.Platform.Impl.Core;
using Hzdtf.RabbitV2.Consul.AspNet.Core;

namespace Hzdtf.RabbitV2.WebServerExample.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            PlatformCodeTool.Config = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //var conn = services.AddRabbitConnectionAndOpen("host1");  // 作为服务端应该使用此注入方法
            var conn = services.AddRabbitConnectionAndOpenConsulConfigCenter("host1");
            var consumer = conn.CreateConsumer("TestExchange", "TestQueue1"); // 作为消费者服务，需要输入监听的交换机和队列
            consumer.Subscribe((string msg) =>
            {
                Console.WriteLine("接收到消息：" + msg);

                return true; // 返回是否处理成功；如果为false，默认会把此消息转发到下一个消费者上
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
