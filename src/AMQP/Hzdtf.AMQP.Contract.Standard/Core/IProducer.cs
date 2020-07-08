using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.Release;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Standard.Core
{
    /// <summary>
    /// 生产者接口
    /// @ 黄振东
    /// </summary>
    public interface IProducer : ICloseable, IDisposable
    {
        /// <summary>
        /// 字节数组序列化
        /// </summary>
        IBytesSerialization BytesSerialization
        {
            get;
            set;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        void Publish(string message, string routingKey = null);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        void Publish(object message, string routingKey = null);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        void Publish(byte[] message, string routingKey = null);
    }
}
