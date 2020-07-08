using Hzdtf.BusinessDemo.Contract.Standard;
using Hzdtf.Configuration.Impl.Config.Core.App;
using Hzdtf.Platform.Contract.Standard;
using Hzdtf.RabbitV2.Impl.Standard.Connection;
using Hzdtf.RabbitV2.Impl.Standard.Core;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Reflection;
using System.Text;

namespace Hzdtf.RabbitV2.RpcClientExample.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            PlatformTool.AppConfig = new AppConfiguration();

            // SimpleRpcClient();

            BusinessCallRpcClient();
        }

        /// <summary>
        /// 简单RPC客户端
        /// </summary>
        private static void SimpleRpcClient()
        {
            var conn = new RabbitConnection();
            conn.OpenByHostId("host1");
            var rpcClient = conn.CreateRpcClient("RpcExchange", "RpcQueue");

            Console.WriteLine("请输入要给RPC服务端发送的消息:");
            while (true)
            {
                var msg = Console.ReadLine();
                if ("exit".Equals(msg))
                {
                    break;
                }

                var re = rpcClient.Call(Encoding.UTF8.GetBytes(msg));
                var reStr = re.IsNullOrLength0() ? null : Encoding.UTF8.GetString(re);
                Console.WriteLine("服务端返回:" + reStr);
            }
        }

        /// <summary>
        /// 业务调用RPC服务客户端
        /// </summary>
        private static void BusinessCallRpcClient()
        {
            var proxy = new RabbitRpcDispatchProxyClient();
            var service = proxy.Create<IPersonService>();
            var re = service.Get(10);
            Console.WriteLine("Get:" + re);
            var re1 = service.Query();
            Console.WriteLine("Query:" + re1);
        }
    }
}
