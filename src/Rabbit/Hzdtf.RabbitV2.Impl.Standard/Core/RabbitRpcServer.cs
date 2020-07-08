using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.AMQP.Model.Standard.BusinessException;
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
using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Model.Return;
using Hzdtf.Utility.Standard;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit RPC服务端
    /// @ 黄振东
    /// </summary>
    public class RabbitRpcServer : RabbitCoreBase, IRpcServer
    {
        #region 属性与字段

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
        public RabbitRpcServer(IModel channel, AmqpQueueInfo amqpQueue)
            : base(channel, amqpQueue, true)
        {
        }

        #endregion

        #region IRpcServer 接口

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        public void Receive(Func<byte[], byte[]> receiveMessageFun)
        {
            channel.QueueDeclare(queue: amqpQueue.Queue.Name, durable: amqpQueue.Persistent, exclusive: false, autoDelete: amqpQueue.Queue.AutoDelQueue, arguments: null);
            if (amqpQueue.Queue.Qos != null)
            {
                channel.BasicQos(0, amqpQueue.Queue.Qos.GetValueOrDefault(), false);
            }

            channel.QueueBind(amqpQueue.Queue.Name, amqpQueue.ExchangeName, amqpQueue.Queue.Name);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: amqpQueue.Queue.Name, autoAck: false, consumer: consumer);

            consumer.Received += (model, ea) =>
            {
                // 错误返回信息
                BasicReturnInfo errorReturn = null;

                // 返回给客户端的数据
                byte[] outData = null;

                // 关联ID
                string correlationId = null;
                IBasicProperties props = null;
                IBasicProperties replyProps = null;
                try
                {
                    props = ea.BasicProperties;
                    replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    correlationId = props.CorrelationId;

                    byte[] inData = ea.Body.IsEmpty ? null : ea.Body.ToArray();
                    try
                    {
                        outData = receiveMessageFun(inData);
                    }
                    catch (Exception ex)
                    {
                        PublishExceptionQueue(ex, null);

                        Log.ErrorAsync("RpcServer回调业务处理出现异常", ex, typeof(RabbitRpcServer).Name, correlationId);

                        errorReturn = new BasicReturnInfo();
                        errorReturn.SetFailureMsg("业务处理出现异常", ex.Message);

                        outData = Encoding.UTF8.GetBytes(JsonUtil.SerializeIgnoreNull(errorReturn));
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorAsync("RpcServer接收消息处理出现异常", ex, typeof(RabbitRpcServer).Name, correlationId);

                    errorReturn = new BasicReturnInfo();
                    errorReturn.SetFailureMsg("RpcServer接收消息处理出现异常", ex.Message);

                    outData = Encoding.UTF8.GetBytes(JsonUtil.SerializeIgnoreNull(errorReturn));
                }
                finally
                {
                    if (props != null && replyProps != null)
                    {
                        channel.BasicPublish(exchange: amqpQueue.ExchangeName, routingKey: props.ReplyTo, basicProperties: replyProps, body: outData);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                }
            };

            InitExceptionHandle();
        }

        #endregion

        #region 重写父类的方法

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
        protected bool PublishExceptionQueue(Exception ex, object queueMessage, string desc = null)
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
