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
            //var conn = services.AddRabbitConnectionAndOpen("host1");  // ��Ϊ�����Ӧ��ʹ�ô�ע�뷽��
            var conn = services.AddRabbitConnectionAndOpenConsulConfigCenter("host1");
            var consumer = conn.CreateConsumer("TestExchange", "TestQueue1"); // ��Ϊ�����߷�����Ҫ��������Ľ������Ͷ���
            consumer.Subscribe((string msg) =>
            {
                Console.WriteLine("���յ���Ϣ��" + msg);

                return true; // �����Ƿ���ɹ������Ϊfalse��Ĭ�ϻ�Ѵ���Ϣת������һ����������
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
