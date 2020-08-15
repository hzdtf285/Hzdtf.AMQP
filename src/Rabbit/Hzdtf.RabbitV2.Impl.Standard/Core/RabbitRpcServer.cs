using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Hzdtf.AMQP.Model.Standard.Config;
using System.Linq;
using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Model.Return;
using Hzdtf.Logger.Contract.Standard;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.AMQP.Impl.Standard;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit RPC服务端
    /// @ 黄振东
    /// </summary>
    public class RabbitRpcServer : RabbitCoreBase, IRpcServer, ISetObject<IExceptionHandle>
    {
        #region 属性与字段

        /// <summary>
        /// 异常处理
        /// </summary>
        private IExceptionHandle exceptionHandle;

        /// <summary>
        /// 异常处理
        /// </summary>
        private IExceptionHandle ExceptionHandle
        {
            get
            {
                if (exceptionHandle == null)
                {
                    exceptionHandle = new RabbitExceptionHandle(amqpQueue, log);
                }

                return exceptionHandle;
            }
        }

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
        /// <param name="exceptionHandle">异常处理</param>
        public RabbitRpcServer(IModel channel, AmqpQueueInfo amqpQueue, ILogable log = null, IExceptionHandle exceptionHandle = null)
            : base(channel, amqpQueue, true, log)
        {
            this.exceptionHandle = exceptionHandle;
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
                        var busEx = AmqpUtil.BuilderBusinessException(ex, null, amqpQueue, log, ex.Message);
                        ExceptionHandle.Handle(busEx);

                        log.ErrorAsync("RpcServer回调业务处理出现异常", ex, typeof(RabbitRpcServer).Name, correlationId);

                        errorReturn = new BasicReturnInfo();
                        errorReturn.SetFailureMsg("业务处理出现异常", ex.Message);

                        outData = Encoding.UTF8.GetBytes(JsonUtil.SerializeIgnoreNull(errorReturn));
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorAsync("RpcServer接收消息处理出现异常", ex, typeof(RabbitRpcServer).Name, correlationId);

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
        }

        #endregion

        /// <summary>
        /// 设置对象
        /// </summary>
        /// <param name="obj">对象</param>
        public void Set(IExceptionHandle obj)
        {
            this.exceptionHandle = obj;
        }
    }
}
