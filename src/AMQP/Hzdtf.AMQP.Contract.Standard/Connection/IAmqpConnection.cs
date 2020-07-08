using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Connection;
using Hzdtf.Utility.Standard.ProcessCall;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Standard.Connection
{
    /// <summary>
    /// AMQP连接接口
    /// @ 黄振东
    /// </summary>
    public interface IAmqpConnection : IConnection<AmqpConnectionInfo>
    {
        /// <summary>
        /// 主机ID
        /// </summary>
        string HostId
        {
            get;
        }

        /// <summary>
        /// 根据主机名打开
        /// </summary>
        /// <param name="hostId">主机ID</param>
        void OpenByHostId(string hostId);

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <returns>生产者</returns>
        IProducer CreateProducer(string exchange);

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>消费者</returns>
        IConsumer CreateConsumer(string exchange, string queue);

        /// <summary>
        /// 创建Rpc客户端
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>Rpc客户端</returns>
        IRpcClient CreateRpcClient(string exchange, string queue);

        /// <summary>
        /// 创建Rpc服务端
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>Rpc服务端</returns>
        IRpcServer CreateRpcServer(string exchange, string queue);
    }
}
