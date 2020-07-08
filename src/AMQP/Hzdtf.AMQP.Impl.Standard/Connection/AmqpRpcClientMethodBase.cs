using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Contract.Standard.Connection;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Release;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Hzdtf.Utility.Standard.Utils;

namespace Hzdtf.AMQP.Impl.Standard.Connection
{
    /// <summary>
    /// AMQP RPC 客户端方法基类
    /// @ 黄振东
    /// </summary>
    public abstract class AmqpRpcClientMethodBase : IRpcClientMethod, IClose, IDisposable
    {
        /// <summary>
        /// AMQP配置读取
        /// </summary>
        private readonly IAmqpConfigReader amqpConfigReader;

        /// <summary>
        /// JSON字节流序列化
        /// </summary>
        private static readonly IBytesSerialization jsonSerialation = new JsonBytesSerialization();

        /// <summary>
        /// MessagePack字节流序列化
        /// </summary>
        private static readonly IBytesSerialization messagePackSerialztion = new MessagePackBytesSerialization();

        /// <summary>
        /// 映射RPC客户端字典（key: RPC客户端标识, value：RPC客户端）
        /// </summary>
        private readonly static IDictionary<string, IRpcClient> dicMapRpcClient = new Dictionary<string, IRpcClient>();

        /// <summary>
        /// 同步映射RPC客户端字典
        /// </summary>
        private readonly static object syncDicMapRpcClient = new object();

        /// <summary>
        /// 连接列表
        /// </summary>
        private readonly static IList<IAmqpConnection> connections = new List<IAmqpConnection>();

        /// <summary>
        /// 同步连接列表
        /// </summary>
        private readonly static object syncConnections = new object();

        /// <summary>
        /// 连接工厂
        /// </summary>
        private readonly IAmqpConnectionFactory connectionFactory;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="connectionFactory">连接工厂</param>
        public AmqpRpcClientMethodBase(IAmqpConnectionFactory connectionFactory)
            : this(null, connectionFactory)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="amqpConfigReader">AMQP配置读取</param>
        /// <param name="connectionFactory">连接工厂</param>
        public AmqpRpcClientMethodBase(IAmqpConfigReader amqpConfigReader, IAmqpConnectionFactory connectionFactory)
        {
            if (amqpConfigReader == null)
            {
                this.amqpConfigReader = AmqpUtil.GetConfigReader(amqpConfigReader);
            }
            else
            {
                this.amqpConfigReader = amqpConfigReader;
            }
            this.connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="message">消息</param>
        /// <returns>返回数据</returns>
        public virtual object Call(MethodInfo method, object message)
        {
            var assemblyName = method.DeclaringType.Assembly.GetName().Name;
            var configInfo = amqpConfigReader.Reader();
            if (configInfo == null)
            {
                throw new Exception("找不到AMQP配置信息");
            }

            var assemblyQueue = configInfo.ToRpcClientAssemblyQueue(assemblyName);
            if (assemblyQueue == null)
            {
                throw new Exception($"找不到程序集[{assemblyName}]的RPC客户端配置信息");
            }

            var byteSeri = GetByteSerialization(assemblyQueue.RpcClientAssembly.DataType);
            var byteData = byteSeri.Serialize(message);

            IRpcClient rpcClient = GetAvailableRpcClient(assemblyQueue);
            var reData = rpcClient.Call(byteData);

            // 如果返回值是空或者类型是void，则直接返回null
            if (reData.IsNullOrLength0() || method.ReturnType.IsTypeVoid())
            {
                return null;
            }
            else
            {
                Type targetType = method.GetReturnValueType();
                if (targetType == null)
                {
                    return null;
                }

                return byteSeri.Deserialize(reData, targetType);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            foreach (var item in dicMapRpcClient)
            {
                item.Value.Close();
            }
            foreach (var item in connections)
            {
                item.Close();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        ~AmqpRpcClientMethodBase()
        {
            Close();
        }

        /// <summary>
        /// 根据数据类型获取字节流序列化
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <returns>字节流序列化</returns>
        private IBytesSerialization GetByteSerialization(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
            {
                return jsonSerialation;
            }

            var type = dataType.ToLower();
            switch (type)
            {
                case "json":

                    return jsonSerialation;

                case "messagepack":

                    return messagePackSerialztion;

                default:

                    throw new NotSupportedException($"不支持数据类型[{dataType}]的字节流序列化");
            }
        }

        /// <summary>
        /// 获取RPC客户端标识
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>RPC客户端标识</returns>
        private string GetRpcClientId(string exchange, string queue) => $"{exchange}_{queue}";

        /// <summary>
        /// 获取可用的RPC客户端
        /// </summary>
        /// <param name="rpcClientAssemblyQueue">RPC客户端程序集队列</param>
        /// <returns>RPC客户端</returns>
        private IRpcClient GetAvailableRpcClient(RpcClientAssemblyQueueInfo rpcClientAssemblyQueue)
        {
            var rpcClientId = GetRpcClientId(rpcClientAssemblyQueue.ExchangeName, rpcClientAssemblyQueue.QueueName);

            // 先从缓存里找RPC客户端
            if (dicMapRpcClient.ContainsKey(rpcClientId))
            {
                return dicMapRpcClient[rpcClientId];
            }

            // 再从连接缓存里找
            var conn = connections.Where(p => p.HostId == rpcClientAssemblyQueue.HostId).FirstOrDefault();
            if (conn == null)
            {                
                lock (syncConnections)
                {
                    conn = connections.Where(p => p.HostId == rpcClientAssemblyQueue.HostId).FirstOrDefault();
                    // 双重锁定，打开远程要耗时，目的是为了防止多线程并集性打开远程
                    if (conn == null)
                    {
                        conn = connectionFactory.CreateAndOpen(new AmqpConnectionWrapInfo()
                        {
                            HostId = rpcClientAssemblyQueue.HostId
                        });

                        connections.Add(conn);
                    }
                }
            }

            if (dicMapRpcClient.ContainsKey(rpcClientId))
            {
                return dicMapRpcClient[rpcClientId];
            }

            IRpcClient rpcClient = null;
            lock (syncDicMapRpcClient)
            {
                rpcClient = conn.CreateRpcClient(rpcClientAssemblyQueue.ExchangeName, rpcClientAssemblyQueue.QueueName);
                try
                {
                    dicMapRpcClient.Add(rpcClientId, rpcClient);
                }
                catch (ArgumentException) { }
            }

            return rpcClient;
        }
    }
}
