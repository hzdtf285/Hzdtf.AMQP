using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Contract.Standard.Connection;
using Hzdtf.AMQP.Contract.Standard.Core;
using Hzdtf.AMQP.Impl.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Connection;
using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Safety;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard.Connection
{
    /// <summary>
    /// AMQP连接基类
    /// @ 黄振东
    /// </summary>
    public abstract class AmqpConnectionBase : ConnectionBase<AmqpConnectionInfo>, IAmqpConnection
    {
        #region 属性与字段

        /// <summary>
        /// 主机ID
        /// </summary>
        protected string hostId;

        /// <summary>
        /// 主机ID
        /// </summary>
        public string HostId
        {
            get => hostId;
        }

        /// <summary>
        /// AMQP配置读取
        /// </summary>
        public IAmqpConfigReader AmqpConfigReader
        {
            get;
            set;
        }

        /// <summary>
        /// 连接字符串解析
        /// </summary>
        protected readonly IConnectionStringParse<AmqpConnectionInfo> connectionStringParse;

        /// <summary>
        /// 加密
        /// </summary>
        protected readonly ISymmetricalEncryption symmetricalEncryption;

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// </summary>
        public AmqpConnectionBase()
            : this(AmqpUtil.GetConfigReader())
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="amqpConfigReader">AMQP配置读取</param>
        public AmqpConnectionBase(IAmqpConfigReader amqpConfigReader)
            : this (amqpConfigReader, AmqpConnectionUtil.DefaultConnectionStringParse, null)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="amqpConfigReader">AMQP配置读取</param>
        /// <param name="connectionStringParse">l连接字符串解析</param>
        /// <param name="symmetricalEncryption">加密</param>
        public AmqpConnectionBase(IAmqpConfigReader amqpConfigReader, IConnectionStringParse<AmqpConnectionInfo> connectionStringParse, ISymmetricalEncryption symmetricalEncryption)
        {
            this.AmqpConfigReader = AmqpUtil.GetConfigReader(amqpConfigReader);
            this.connectionStringParse = connectionStringParse;
            symmetricalEncryption = SymmetricalEncryptionUtil.GetSymmetricalEncryption(symmetricalEncryption);
        }

        #endregion

        #region 连接相关

        /// <summary>
        /// 根据主机ID打开
        /// </summary>
        /// <param name="hostId">主机ID</param>
        public virtual void OpenByHostId(string hostId)
        {
            if (string.IsNullOrWhiteSpace(hostId))
            {
                throw new ArgumentNullException("主机ID不能为空");
            }

            var amqpConfig = AmqpConfigReader.Reader();
            if (amqpConfig == null)
            {
                throw new Exception("未找到AMQP配置信息");
            }

            var connInfo = amqpConfig.ToAmqpConnectionInfo(hostId, connectionStringParse, symmetricalEncryption);
            if (connInfo == null)
            {
                throw new KeyNotFoundException($"找不到主机ID:{hostId}的配置");
            }
            Open(connInfo);
        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="connectionModel">连接模型</param>
        public override void Open(AmqpConnectionInfo connectionModel)
        {
            SetDefaultConnectionInfo(connectionModel);
            ValidateOpenParams(connectionModel);

            base.Open(connectionModel);

            var amqpConfig = AmqpConfigReader.Reader();
            if (amqpConfig == null)
            {
                throw new Exception("未找到AMQP配置信息");
            }

            hostId = amqpConfig.ToHostId(connectionModel, connectionStringParse, symmetricalEncryption);
        }

        /// <summary>
        /// 获取默认的连接字符串解析器
        /// </summary>
        /// <returns>默认的连接字符串解析器</returns>
        protected override IConnectionStringParse<AmqpConnectionInfo> GetDefaultConnectionStringParse() => connectionStringParse;

        /// <summary>
        /// 获取默认的连接字符串
        /// </summary>
        /// <returns>默认的连接字符串</returns>
        protected override string GetDefaultConnectionString()
        {
            var amqpConfig = AmqpConfigReader.Reader();
            if (amqpConfig == null)
            {
                throw new Exception("未找到AMQP配置信息");
            }

            var connString = amqpConfig.ToFristConnectionString(symmetricalEncryption);
            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new KeyNotFoundException("未找到连接配置");
            }

            return connString;
        }

        /// <summary>
        /// 验证其他打开参数
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        protected override void ValidateOtherOpenParams(AmqpConnectionInfo connectionInfo)
        {
            if (string.IsNullOrWhiteSpace(connectionInfo.VirtualPath))
            {
                throw new ArgumentNullException("虚拟路径不能为空");
            }
        }

        /// <summary>
        /// 获取默认端口
        /// </summary>
        /// <returns>默认端口</returns>
        protected virtual int GetDefaultPort() => 0;

        /// <summary>
        /// 设置默认连接信息
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        protected virtual void SetDefaultConnectionInfo(AmqpConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                return;
            }

            if (connectionInfo.Port == 0)
            {
                connectionInfo.Port = GetDefaultPort();
            }
        }

        #endregion

        #region AMQP 相关

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <returns>生产者</returns>
        public virtual IProducer CreateProducer(string exchange)
        {
            var exchangeInfo = GetExchange(exchange);
            var amqpQueue = AmqpQueueInfo.CreateAmqpQueue(hostId, exchangeInfo, null);

            return CreateProducer(amqpQueue);
        }

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>消费者</returns>
        public virtual IConsumer CreateConsumer(string exchange, string queue)
        {
            var exchangeInfo = GetExchange(exchange);
            var queueInfo = GetQueue(exchangeInfo, queue);
            var amqpQueue = AmqpQueueInfo.CreateAmqpQueue(hostId, exchangeInfo, queueInfo);

            return CreateConsumer(amqpQueue);
        }

        /// <summary>
        /// 创建Rpc客户端
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>Rpc客户端</returns>
        public virtual IRpcClient CreateRpcClient(string exchange, string queue)
        {
            var exchangeInfo = GetExchange(exchange);
            var queueInfo = GetQueue(exchangeInfo, queue);
            var amqpQueue = AmqpQueueInfo.CreateAmqpQueue(hostId, exchangeInfo, queueInfo);

            return CreateRpcClient(amqpQueue);
        }

        /// <summary>
        /// 创建Rpc服务端
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <returns>Rpc服务端</returns>
        public virtual IRpcServer CreateRpcServer(string exchange, string queue)
        {
            var exchangeInfo = GetExchange(exchange);
            var queueInfo = GetQueue(exchangeInfo, queue);
            var amqpQueue = AmqpQueueInfo.CreateAmqpQueue(hostId, exchangeInfo, queueInfo);

            return CreateRpcServer(amqpQueue);
        }

        /// <summary>
        /// 获取交换机数组
        /// </summary>
        /// <returns>交换机数组</returns>
        protected virtual ExchangeInfo[] GetExchanges()
        {
            var amqpConfig = AmqpConfigReader.Reader();
            if (amqpConfig == null)
            {
                throw new NullReferenceException("AMQP配置不存在");
            }
            
            var exchanges = amqpConfig.ToExchanges(hostId);
            if (exchanges == null)
            {
                throw new Exception($"找不到主机ID[{hostId}],虚拟路径[{ConnectionInfo.VirtualPath}]的对应的交换机数组");
            }

            return exchanges;
        }

        /// <summary>
        /// 获取交换机
        /// </summary>
        /// <param name="exchangeName">交换名</param>
        /// <returns>交换机</returns>
        protected virtual ExchangeInfo GetExchange(string exchangeName)
        {
            var exchanges = GetExchanges();
            var e = exchanges.Where(p => p.Name == exchangeName).FirstOrDefault();
            if (e == null)
            {
                throw new Exception($"找不到主机ID[{hostId}],虚拟路径[{ConnectionInfo.VirtualPath}],交换机名[{exchangeName}]的对应的交换机");
            }

            return e;
        }

        /// <summary>
        /// 获取队列
        /// </summary>
        /// <param name="exchange">交换机</param>
        /// <param name="queueName">队列名</param>
        /// <returns>队列</returns>
        protected virtual QueueInfo GetQueue(ExchangeInfo exchange, string queueName)
        {
            if (exchange.Queues.IsNullOrLength0())
            {
                throw new Exception($"交换机[{exchange.Name}]下没有任何队列");
            }

            var q = exchange.Queues.Where(p => p.Name == queueName).FirstOrDefault();
            if (q == null)
            {
                throw new Exception($"找不到交换机[{exchange.Name}],队列[{queueName}]");
            }

            return q;
        }

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>生产者</returns>
        protected abstract IProducer CreateProducer(AmqpQueueInfo amqpQueue);

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>消费者</returns>
        protected abstract IConsumer CreateConsumer(AmqpQueueInfo amqpQueue);

        /// <summary>
        /// 创建Rpc客户端
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>Rpc客户端</returns>
        protected abstract IRpcClient CreateRpcClient(AmqpQueueInfo amqpQueue);

        /// <summary>
        /// 创建Rpc服务端
        /// </summary>
        /// <param name="amqpQueue">AMQP队列</param>
        /// <returns>Rpc服务端</returns>
        protected abstract IRpcServer CreateRpcServer(AmqpQueueInfo amqpQueue);

        #endregion
    }
}
