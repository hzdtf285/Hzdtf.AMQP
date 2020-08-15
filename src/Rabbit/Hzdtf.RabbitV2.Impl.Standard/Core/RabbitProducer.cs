using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.Logger.Contract.Standard;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit生产者
    /// @ 黄振东
    /// </summary>
    public class RabbitProducer : RabbitCoreBase, IProducer
    {
        #region 属性与字段

        /// <summary>
        /// 字节数组序列化，默认为JSON序列化
        /// </summary>
        public IBytesSerialization BytesSerialization
        {
            get;
            set;
        } = new JsonBytesSerialization();

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// 初始化各个对象以便就绪
        /// 只初始化交换机与基本属性，队列定义请重写Init方法进行操作
        /// </summary>
        /// <param name="channel">渠道</param>
        /// <param name="amqpQueue">AMQP队列信息</param>
        /// <param name="log">日志</param>
        public RabbitProducer(IModel channel, AmqpQueueInfo amqpQueue, ILogable log = null)
            : base(channel, amqpQueue, false, log)
        {
        }

        #endregion

        #region IProducer 接口

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        public void Publish(string message, string routingKey = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Publish(Encoding.UTF8.GetBytes(message), routingKey);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        public void Publish(object message, string routingKey = null)
        {
            if (message == null)
            {
                return;
            }

            Publish(BytesSerialization.Serialize(message), routingKey);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        public void Publish(byte[] message, string routingKey = null)
        {
            if (message.IsNullOrLength0())
            {
                return;
            }

            string logMsg = string.Format("给路由键:{0},交换机:{1} 发送消息", routingKey, amqpQueue.ExchangeName);
            log.DebugAsync(logMsg, null, typeof(RabbitProducer).Name, amqpQueue.ExchangeName, routingKey);

            channel.BasicPublish(amqpQueue.ExchangeName, routingKey, basicProperties, message);
        }

        #endregion
    }
}
