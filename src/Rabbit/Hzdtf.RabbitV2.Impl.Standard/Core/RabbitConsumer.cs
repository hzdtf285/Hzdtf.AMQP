using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.AMQP.Model.Standard.BusinessException;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.AMQP.Contract.Standard.Connection;
using Hzdtf.RabbitV2.Impl.Standard.Connection;
using System.Linq;
using Hzdtf.Utility.Standard;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit消费者
    /// @ 黄振东
    /// </summary>
    public class RabbitConsumer : RabbitCoreBase, IConsumer
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

        /// <summary>
        /// 异常处理连接数组
        /// </summary>
        private IAmqpConnection[] exceptionHandleConnections;

        /// <summary>
        /// 异常处理生产者字典；key：生产者，value：路由键
        /// </summary>
        private IDictionary<IProducer, string> dicExceptionHandleProducers;

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// 初始化各个对象以便就绪
        /// 只初始化交换机与基本属性，队列定义请重写Init方法进行操作
        /// </summary>
        /// <param name="channel">渠道</param>
        /// <param name="amqpQueue">AMQP队列信息</param>
        public RabbitConsumer(IModel channel, AmqpQueueInfo amqpQueue)
            : base(channel, amqpQueue, true)
        {
        }

        #endregion

        #region IConsumer 接口

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe(Func<string, bool> receiveMessageFun, bool isAutoAck = false)
        {
            Subscribe((byte[] x) =>
            {
                if (receiveMessageFun != null)
                {
                    string msg = null;
                    try
                    {
                        msg = Encoding.UTF8.GetString(x);
                    }
                    catch (Exception ex)
                    {
                        string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},Encoding.UTF8.GetString发生异常,返回应答:true";
                        Log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                        return true;
                    }

                    try
                    {
                        return receiveMessageFun(msg);
                    }
                    catch (Exception ex)
                    {
                        var isAck = PublishExceptionQueue(ex, msg);

                        string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                        Log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                        return isAck;
                    }
                }

                return true;
            }, isAutoAck);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe<T>(Func<T, bool> receiveMessageFun, bool isAutoAck = false)
        {
            Subscribe((byte[] x) =>
            {
                if (receiveMessageFun != null)
                {
                    T data = default(T);
                    try
                    {
                        data = BytesSerialization.Deserialize<T>(x);
                    }
                    catch (Exception ex)
                    {
                        string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},BytesSerialization.Deserialize发生异常(返回应答为true)，认为是不符合业务规范的数据，应删除消息";
                        Log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                        // 反序列异常则返回true
                        return true;
                    }

                    try
                    {
                        return receiveMessageFun(data);
                    }
                    catch (Exception ex)
                    {
                        var isAck = PublishExceptionQueue(ex, data);

                        string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                        Log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                        return isAck;
                    }
                }

                return true;
            }, isAutoAck);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe(Func<byte[], bool> receiveMessageFun, bool isAutoAck = false)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += (o, e) =>
            {
                bool isAck = true;
                if (receiveMessageFun != null && !e.Body.IsEmpty)
                {
                    string logMsg = $"{GetLogTitleMsg()}.接收到消息";
                    Log.DebugAsync(logMsg, null, tags: GetLogTags());

                    try
                    {
                        isAck = receiveMessageFun(e.Body.ToArray());
                    }
                    catch (Exception ex)
                    {
                        isAck = PublishExceptionQueue(ex, "字节数组数据");

                        logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                        Log.ErrorAsync(logMsg, ex, tags: GetLogTags());
                    }
                }

                // 如果自动回答，则什么都不用干
                if (isAutoAck)
                {
                    return;
                }

                // 如果业务端返回应答，则返回MQ已成功处理，否则返回未处理成功，需要将该消息放回队列进行重试
                if (isAck)
                {
                    channel.BasicAck(e.DeliveryTag, false);
                }
                else
                {
                    channel.BasicNack(e.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(amqpQueue.Queue.Name, isAutoAck, consumer);
        }

        #endregion

        #region 重写父类的方法

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Init()
        {
            channel.QueueDeclare(amqpQueue.Queue.Name, amqpQueue.Persistent, false, amqpQueue.Queue.AutoDelQueue, null);
            if (amqpQueue.Queue.RoutingKeys.IsNullOrLength0())
            {
                return;
            }

            foreach (string key in amqpQueue.Queue.RoutingKeys)
            {
                channel.QueueBind(amqpQueue.Queue.Name, amqpQueue.ExchangeName, key);
            }

            InitExceptionHandle();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (!dicExceptionHandleProducers.IsNullOrCount0())
            {
                foreach (var item in dicExceptionHandleProducers)
                {
                    item.Key.Close();
                }
            }

            if (!exceptionHandleConnections.IsNullOrLength0())
            {
                foreach (var item in exceptionHandleConnections)
                {
                    item.Close();
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 推送异常队列
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="queueMessage">队列消息</param>
        /// <param name="desc">描述</param>
        /// <returns>是否推送成功</returns>
        private bool PublishExceptionQueue(Exception ex, object queueMessage, string desc = null)
        {
            if (dicExceptionHandleProducers.IsNullOrCount0())
            {
                return false;
            }

            string queueMessageJson = null;
            if (queueMessage != null)
            {
                try
                {
                    queueMessageJson = JsonUtil.SerializeIgnoreNull(queueMessage);
                }
                catch (Exception ex1)
                {
                    Log.ErrorAsync("JSON序列化业务异常信息出错", ex1, typeof(RabbitConsumer).Name, GetLogTags());
                }
            }

            var busEx = new BusinessExceptionInfo()
            {
                Time = DateTime.Now,
                ServiceName = string.IsNullOrWhiteSpace(amqpQueue.ExceptionHandle.ServiceName) ? UtilTool.AppServiceName : amqpQueue.ExceptionHandle.ServiceName,
                ExceptionString = ex.ToString(),
                ExceptionMessage = ex.Message,
                Exchange = amqpQueue.ExchangeName,
                Queue = amqpQueue.Queue.Name,
                QueueMessageJsonString = queueMessageJson,
                Desc = desc,
                ServerMachineName = Environment.MachineName,
                ServerIP = NetworkUtil.LocalIP
            };

            foreach (var item in dicExceptionHandleProducers)
            {
                item.Key.Publish(busEx, item.Value);
            }

            return true;
        }

        /// <summary>
        /// 初始化异常处理
        /// </summary>
        private void InitExceptionHandle()
        {
            // 如果有定义异常处理
            if (amqpQueue.ExceptionHandle == null || amqpQueue.ExceptionHandle.PublishConsumers.IsNullOrLength0())
            {
                return;
            }

            // 查找不重复的主机ID数组
            var hostIds = amqpQueue.ExceptionHandle.PublishConsumers.Select(p => p.HostId).Distinct().ToArray();
            exceptionHandleConnections = new RabbitConnection[hostIds.Length];
            for (var i = 0; i < hostIds.Length; i++)
            {
                exceptionHandleConnections[i] = new RabbitConnection();
                exceptionHandleConnections[i].OpenByHostId(hostIds[i]);
            }

            dicExceptionHandleProducers = new Dictionary<IProducer, string>(amqpQueue.ExceptionHandle.PublishConsumers.Length);
            foreach (var pc in amqpQueue.ExceptionHandle.PublishConsumers)
            {
                var conn = exceptionHandleConnections.Where(p => p.HostId == pc.HostId).FirstOrDefault();
                var producer = conn.CreateProducer(pc.Exchange);

                dicExceptionHandleProducers.Add(producer, pc.RoutingKey);
            }
        }

        #endregion
    }
}
