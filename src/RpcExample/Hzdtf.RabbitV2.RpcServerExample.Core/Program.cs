using Hzdtf.RabbitV2.Impl.Standard.Connection;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.Data.Dic;
using Hzdtf.Utility.Standard.InterfaceImpl;
using Hzdtf.Utility.Standard.ProcessCall;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.RabbitV2.RpcServerExample.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            //SimpleRpcServer();

            BusinessCallRpcServer();

            Console.Read();
        }

        /// <summary>
        /// 简单RPC服务端
        /// </summary>
        private static void SimpleRpcServer()
        {
            var conn = new RabbitConnection();
            conn.OpenByHostId("host1");
            var rpcServer = conn.CreateRpcServer("RpcExchange", "RpcQueue");

            Console.WriteLine("监听Rpc服务的请求：");

            rpcServer.Receive(msg =>
            {
                var str = Encoding.UTF8.GetString(msg);

                Console.WriteLine(str);

                if ("exception".Equals(str))
                {
                    throw new Exception("RPC Server 业务处理异常");
                }

                return Encoding.UTF8.GetBytes("已收到");
            });
        }

        /// <summary>
        /// 业务调用RPC服务端
        /// </summary>
        private static void BusinessCallRpcServer()
        {
            // 创建Rabbit连接和Rpc服务端
            var conn = new RabbitConnection();
            conn.OpenByHostId("host1");
            var rpcServer = conn.CreateRpcServer("RpcExchange", "RpcQueue");

            // 创建Rpc监听服务
            var listen = new RpcServerListen();

            // 设置接口映射配置文件
            var mapImplCache = new InterfaceMapImplCache();
            mapImplCache.Set(new DictionaryJson("Config/interfaceAssemblyMapImplAssemblyConfig.json"));

            // 将映射赋值到监听中
            listen.InterfaceMapImpl = mapImplCache;

            // 将RPC服务设置到监听中
            listen.RpcServer = rpcServer;
            listen.BytesSerialization = new MessagePackBytesSerialization();

            // 注册错误事件
            listen.ReceivingError += Listen_ReceivingError;

            // 开始监听
            Console.WriteLine("监听Rpc服务的请求：");
            listen.ListenAsync();
        }
        
        /// <summary>
        /// 监听接收出错
        /// </summary>
        /// <param name="arg1">错误消息</param>
        /// <param name="arg2">异常</param>
        private static void Listen_ReceivingError(string arg1, Exception arg2)
        {
            Console.WriteLine(arg1 + "," + arg2 ?? arg2.ToString());
        }
    }
}
