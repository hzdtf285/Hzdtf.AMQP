using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.Logger.Contract.Standard;
using Hzdtf.Utility.Standard.Data;
using Hzdtf.Utility.Standard.Release;
using Hzdtf.Utility.Standard.Utils;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit核心基类
    /// @ 黄振东
    /// </summary>
    public abstract class RabbitCoreBase : ICloseable, IDisposable
    {
        #region 属性与字段

        /// <summary>
        /// 渠道
        /// </summary>
        protected readonly IModel channel;

        /// <summary>
        /// AMQP队列信息
        /// </summary>
        protected readonly AmqpQueueInfo amqpQueue;

        /// <summary>
        /// 基本属性集合
        /// </summary>
        protected IBasicProperties basicProperties;

        /// <summary>
        /// 日志
        /// </summary>
        public ILogable Log
        {
            get;
            set;
        } = LogTool.DefaultLog;

        /// <summary>
        /// 关闭后事件
        /// </summary>
        public event DataHandler Closed;

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// 初始化各个对象以便就绪
        /// 只初始化交换机与基本属性，队列定义请重写Init方法进行操作
        /// </summary>
        /// <param name="channel">渠道</param>
        /// <param name="amqpQueue">AMQP队列信息</param>
        /// <param name="isDeclare">是否定义</param>
        public RabbitCoreBase(IModel channel, AmqpQueueInfo amqpQueue, bool isDeclare)
        {
            ValidateUtil.ValidateNull(channel, "渠道");
            ValidateUtil.ValidateNull(amqpQueue, "AMQP队列信息");

            this.channel = channel;
            this.amqpQueue = amqpQueue;

            if (isDeclare)
            {
                channel.ExchangeDeclare(amqpQueue.ExchangeName, amqpQueue.Type, amqpQueue.Persistent);
                if (amqpQueue.Queue.Qos != null)
                {
                    channel.BasicQos(0, amqpQueue.Queue.Qos.GetValueOrDefault(), false);
                }
            }

            basicProperties = channel.CreateBasicProperties();
            basicProperties.Persistent = amqpQueue.Persistent;

            Init();
        }

        #endregion

        #region ICloseable 接口

        /// <summary>
        /// 关闭
        /// </summary>
        public virtual void Close()
        {
            if (channel != null && channel.IsOpen)
            {
                channel.Close();
                channel.Dispose();
            }

            OnClosed();
        }

        #endregion

        #region 虚方法

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Init()
        {
        }

        #endregion

        #region 受保护的方法

        /// <summary>
        /// 执行关闭事件
        /// </summary>
        /// <param name="data">数据</param>
        protected void OnClosed(object data = null)
        {
            if (Closed != null)
            {
                Closed(this, new DataEventArgs(data));
            }
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 析构方法
        /// </summary>
        ~RabbitCoreBase()
        {
            Dispose();
        }

        /// <summary>
        /// 获取日志标题消息
        /// </summary>
        /// <returns>日志标题消息</returns>
        protected string GetLogTitleMsg()
        {
            var msg = $"Rabbit信息:[主机ID:{amqpQueue.HostId}],虚拟路径:{amqpQueue.VirtualPath},交换机:{amqpQueue.ExchangeName}]";
            if (amqpQueue.Queue != null)
            {
                msg += $"[队列:{amqpQueue.Queue.Name}]";
            }

            return msg;
        }

        /// <summary>
        /// 获取日志标签集合
        /// </summary>
        /// <returns>日志标签集合</returns>
        protected string[] GetLogTags()
        {
            return new string[] { typeof(RabbitConsumer).Name, amqpQueue.HostId, amqpQueue.ExchangeName, amqpQueue.VirtualPath, amqpQueue.Queue != null ? amqpQueue.Queue.Name : null };
        }
    }
}
