using Hzdtf.AMQP.Contract.Standard.Channel;
using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.AMQP.Impl.Standard.Connection;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.RabbitV2.Impl.Standard.Core;
using Hzdtf.Utility.Standard.Connection;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Release;
using Hzdtf.Utility.Standard.Safety;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.RabbitV2.Impl.Standard.Connection
{
    /// <summary>
    /// Rabbit连接
    /// @ 黄振东
    /// </summary>
    public class RabbitConnection : AmqpConnectionBase, IAmqpChannel
    {
        #region 属性与字段

        /// <summary>
        /// 连接
        /// </summary>
        private RabbitMQ.Client.IConnection connection;

        /// <summary>
        /// 状态
        /// </summary>
        public override ConnectionStatusType Status
        {
            get
            {
                if (connection != null)
                {
                    return connection.IsOpen ? ConnectionStatusType.OPENED : ConnectionStatusType.CLOSED;
                }

                return ConnectionStatusType.CLOSED;
            }
        }

        /// <summary>
        /// 渠道列表
        /// </summary>
        private readonly IList<IModel> channels = new List<IModel>();

        #endregion

        #region 构造方法
        
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="amqpConfigReader">AMQP配置读取</param>
        /// <param name="symmetricalEncryption">加密</param>
        public RabbitConnection(IAmqpConfigReader amqpConfigReader = null, ISymmetricalEncryption symmetricalEncryption = null)
            : base(amqpConfigReader, new RabbitConnectionStringParse(), symmetricalEncryption)
        {
        }

        #endregion

        #region 基础连接相关

        /// <summary>
        /// 执行打开
        /// </summary>
        /// <param name="connectionModel">连接模型</param>
        protected override void ExecOpen(AmqpConnectionInfo connectionModel)
        {
            connection = GetConnectionFactory(connectionModel).CreateConnection();
        }

        /// <summary>
        /// 执行关闭
        /// </summary>
        /// <returns>事件数据</returns>
        protected override object ExecClose()
        {
            if (channels.Count > 0)
            {
                foreach (IModel channel in channels)
                {
                    if (channel.IsOpen)
                    {
                        channel.Close();
                        channel.Dispose();
                    }
                }

                channels.Clear();
            }

            if (Status == ConnectionStatusType.OPENED)
            {
                connection.Close();
                connection.Dispose();
            }

            return null;
        }

        /// <summary>
        /// 获取默认端口
        /// </summary>
        /// <returns>默认端口</returns>
        protected override int GetDefaultPort() => RabbitConnectionUtil.DEFAULT_PORT;

        #endregion

        #region Rabbit 相关

        /// <summary>
        /// 根据连接信息获取连接工厂
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        /// <returns>连接工厂</returns>
        private ConnectionFactory GetConnectionFactory(AmqpConnectionInfo connectionInfo)
        {
            var factory = new ConnectionFactory()
            {
                HostName = connectionInfo.Host,
                VirtualHost = connectionInfo.VirtualPath,
                Password = connectionInfo.Password,
                UserName = connectionInfo.User,
                Port = connectionInfo.Port,
                AutomaticRecoveryEnabled = connectionInfo.AutoRecovery,
                RequestedHeartbeat = TimeSpan.FromSeconds(connectionInfo.Heartbeat)
            };

            return factory;
        }

        /// <summary>
        /// 创建渠道并添加到渠道列表里
        /// </summary>
        /// <returns>渠道</returns>
        private IModel CreateChannel()
        {
            IModel channel = connection.CreateModel();
            channels.Add(channel);

            return channel;
        }

        /// <summary>
        /// 为渠道添加关闭后事件处理
        /// </summary>
        /// <param name="channel">渠道</param>
        private void AddClosedEventHandler(ICloseable channel)
        {
            channel.Closed += Channel_Closed;
        }

        /// <summary>
        /// 渠道关闭后
        /// </summary>
        /// <param name="o">引发对象</param>
        /// <param name="e">对象事件参数</param>
        private void Channel_Closed(object o, DataEventArgs e)
        {
            if (o != null && o is ICloseable)
            {
                ((ICloseable)o).Closed -= Channel_Closed;
            }
            if (e == null || e.Data == null)
            {
                return;
            }

            if (e.Data is IModel)
            {
                IModel channel = e.Data as IModel;
                if (channels.Contains(channel))
                {
                    if (channel.IsOpen)
                    {
                        channel.Close();
                    }

                    channels.Remove(channel);
                }
            }
        }

        /// <summary>
        /// 关闭渠道集合
        /// </summary>
        /// <param name="topCount">前几个要关闭的，如果为-1则表示全部</param>
        public void CloseChannels(int topCount = -1)
        {
            if (channels.Count == 0)
            {
                return;
            }
            if (topCount == -1)
            {
                topCount = channels.Count;
            }
            else if (topCount > channels.Count)
            {
                topCount = channels.Count;
            }

            for (int i = 0; i < topCount; i++)
            {
                IModel channel = channels[0];

                if (channel.IsOpen)
                {
                    channel.Close();
                    channel.Dispose();
                }
                channels.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取渠道数
        /// </summary>
        /// <returns>渠道数</returns>
        public int GetChannelCount()
        {
            return channels.Count;
        }

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>生产者</returns>
        protected override IProducer CreateProducer(AmqpQueueInfo amqpQueue) => new RabbitProducer(CreateChannel(), amqpQueue);

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>消费者</returns>
        protected override IConsumer CreateConsumer(AmqpQueueInfo amqpQueue) => new RabbitConsumer(CreateChannel(), amqpQueue);

        /// <summary>
        /// 创建Rpc客户端
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>Rpc客户端</returns>
        protected override IRpcClient CreateRpcClient(AmqpQueueInfo amqpQueue) => new RabbitRpcClient(CreateChannel(), amqpQueue);

        /// <summary>
        /// 创建Rpc服务端
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>Rpc服务端</returns>
        protected override IRpcServer CreateRpcServer(AmqpQueueInfo amqpQueue) => new RabbitRpcServer(CreateChannel(), amqpQueue);
        
        #endregion

        /// <summary>
        /// 析构方法
        /// </summary>
        ~RabbitConnection()
        {
            Dispose();
        }
    }
}
