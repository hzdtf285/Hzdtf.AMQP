using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Hzdtf.AMQP.Model.Standard.Config;
using System.Linq;
using Hzdtf.Utility.Standard.ProcessCall;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit RPC客户端
    /// @ 黄振东
    /// </summary>
    public class RabbitRpcClient : RabbitCoreBase, IRpcClient
    {
        #region 属性与字段

        /// <summary>
        /// 回复队列名
        /// </summary>
        private string replyQueueName;

        /// <summary>
        /// 回复消费者
        /// </summary>
        private EventingBasicConsumer consumer;

        /// <summary>
        /// 回调映射
        /// </summary>
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// 初始化各个对象以便就绪
        /// 只初始化交换机与基本属性，队列定义请重写Init方法进行操作
        /// </summary>
        /// <param name="channel">渠道</param>
        /// <param name="amqpQueue">AMQP队列信息</param>
        public RabbitRpcClient(IModel channel, AmqpQueueInfo amqpQueue)
            : base(channel, amqpQueue, false)
        {
        }

        #endregion

        #region 重写父类的方法

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Init()
        {
            replyQueueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(replyQueueName, amqpQueue.ExchangeName, replyQueueName);
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<byte[]> tcs))
                {
                    return;
                }

                if (ea.Body.IsEmpty)
                {
                    tcs.TrySetResult(null);
                }
                else
                {
                    tcs.TrySetResult(ea.Body.ToArray());
                }
            };
        }

        #endregion

        #region IRpcClient 接口

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>返回字节流</returns>
        public byte[] Call(byte[] message)
        {
            return CallAsync(message).Result;
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>返回字节流任务</returns>
        public Task<byte[]> CallAsync(byte[] message)
        {
            CancellationToken cancellationToken = default(CancellationToken);
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = StringUtil.NewShortGuid();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
            var tcs = new TaskCompletionSource<byte[]>();
            callbackMapper.TryAdd(correlationId, tcs);

            channel.BasicPublish(
                exchange: amqpQueue.ExchangeName,
                routingKey: amqpQueue.Queue.Name,
                basicProperties: props,
                body: message);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
            return tcs.Task;
        }

        #endregion
    }
}
